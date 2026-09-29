using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Read-only snapshot of the loaded scenes before the V3 visual pass.
[InitializeOnLoad]
internal static class AstraSceneInspection
{
    static AstraSceneInspection() { EditorApplication.delayCall += Capture; }
    private static void Capture()
    {
        var output = new StringBuilder();
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            output.AppendLine($"SCENE {scene.path} dirty={scene.isDirty}");
            foreach (var root in scene.GetRootGameObjects())
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Floor" || t.name == "FloorCell") continue;
                    var sr = t.GetComponent<SpriteRenderer>();
                    output.AppendLine($"{PathOf(t)} | pos={t.position} scale={t.lossyScale} active={t.gameObject.activeSelf} | sprite={(sr == null ? "-" : AssetDatabase.GetAssetPath(sr.sprite))} | {string.Join(",", t.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name))}");
                }
        }
        Directory.CreateDirectory("Temp/Astra");
        File.WriteAllText("Temp/Astra/loaded-scenes.txt", output.ToString());
    }
    private static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;
}
