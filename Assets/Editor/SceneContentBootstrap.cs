using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.EditorTools
{
    public static class SceneContentBootstrap
    {
        [MenuItem("Memento Mori/Setup/Create Graybox Content")]
        public static void CreateGrayboxContent()
        {
            Create("MainMenu", new[] { "MoonBackdrop", "Logo_MementoMori", "Button_Play", "Button_Credits", "Button_Quit" });
            Create("Quarto", new[] { "Bed", "Desk", "Grimoire", "EmptyBowl", "PoeToy", "Window", "Portrait", "RitualCircle", "PortalToLabirinto" });
            Create("Labirinto", new[] { "PoeReveal", "AndrealphusAlcove", "MoonPortal" });
            Create("DominioLua", new[] { "Entrada", "JardimLunar", "SalaDosEspelhos", "CorredorIlusorio", "CamaraDoSigilo", "SalaDoFragmento", "Mirror_01", "Mirror_02", "Mirror_03", "Mirror_04", "Mirror_05", "Sigil_Moon", "Sigil_Eye", "Sigil_Spiral", "Fragment" });
            Create("FinalBeta", new[] { "FragmentGlow", "FinalText", "Credits", "Button_ReturnToMenu" });
            AssetDatabase.SaveAssets();
        }

        private static void Create(string sceneName, string[] names)
        {
            var scene = EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}.unity", OpenSceneMode.Single);
            var world = GameObject.Find("SceneRoot/_World")?.transform;
            if (world == null) return;
            foreach (var name in names)
            {
                if (world.Find(name) != null) continue;
                var item = GameObject.CreatePrimitive(PrimitiveType.Quad);
                item.name = name;
                item.transform.SetParent(world, false);
                item.transform.localPosition = new Vector3(Random.Range(-6f, 6f), Random.Range(-3f, 3f), 0f);
                item.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
                Object.DestroyImmediate(item.GetComponent<MeshCollider>());
            }
            EditorSceneManager.SaveScene(scene);
        }
    }
}
