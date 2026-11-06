using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace MementoMori.EditorTools
{
    /// <summary>
    /// Demo shortcuts: each one opens a single map (Labirinto or Dominio da Lua) and frames all of it in the
    /// Scene View. Ctrl+Shift+1 / Ctrl+Shift+2.
    /// </summary>
    public static class PresentationShortcuts
    {
        private const string Labirinto = "Assets/Scenes/Labirinto.unity";
        private const string DominioLua = "Assets/Scenes/DominioLua.unity";

        [MenuItem("Memento Mori/Apresentacao/Labirinto (visao geral) %#1")]
        public static void OpenLabirinto() => Show(Labirinto);

        [MenuItem("Memento Mori/Apresentacao/Dominio da Lua (visao geral) %#2")]
        public static void OpenDominioLua() => Show(DominioLua);

        private static void Show(string path)
        {
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
            // One map at a time: the other scene is closed, never merged into the same view.
            if (SceneManager.GetActiveScene().path != path || SceneManager.sceneCount > 1)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            }
            var target = SceneManager.GetActiveScene();
            SceneVisibilityManager.instance.Show(target.GetRootGameObjects(), true);
            Frame(target);
        }

        public static void Frame(Scene scene)
        {
            var view = SceneView.lastActiveSceneView ?? EditorWindow.GetWindow<SceneView>();
            view.in2DMode = true;
            var maps = Object.FindObjectsByType<TilemapRenderer>(FindObjectsSortMode.None)
                .Where(m => m.gameObject.scene == scene && m.name != "CollisionBoundary").ToList();
            if (maps.Count > 0)
            {
                var bounds = maps[0].bounds;
                foreach (var map in maps) bounds.Encapsulate(map.bounds);
                view.LookAt(bounds.center, Quaternion.identity, Mathf.Max(bounds.extents.x, bounds.extents.y) * 1.02f, true);
            }
            // Animated water, flames and particles keep moving in the Scene View.
            view.sceneViewState.alwaysRefresh = true;
            view.sceneViewState.showParticleSystems = true;
            view.Focus();
            view.Repaint();
        }

        public static void FrameActiveMap() => Frame(SceneManager.GetActiveScene());
    }
}
