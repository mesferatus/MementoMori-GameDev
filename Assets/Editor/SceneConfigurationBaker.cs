using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using MementoMori.Core;

namespace MementoMori.EditorTools
{
    public static class SceneConfigurationBaker
    {
        private static readonly string[] SceneNames = { "MainMenu", "Quarto", "Labirinto", "DominioLua", "FinalBeta" };

        [MenuItem("Memento Mori/Setup/Save Beta Inspector Configuration")]
        public static void Bake()
        {
            var original = SceneManager.GetActiveScene().path;
            foreach (var sceneName in SceneNames)
            {
                var scene = EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}.unity", OpenSceneMode.Single);
                var root = GameObject.Find("SceneRoot") ?? new GameObject("SceneRoot");
                var configuration = root.GetComponent<SceneConfiguration>() ?? root.AddComponent<SceneConfiguration>();
                configuration.Configure(sceneName, NextScene(sceneName), sceneName == "Quarto", Areas(sceneName), Mirrors(sceneName), new[] { "Delayed", "Ahead", "Absent" }, new[] { "Moon", "Eye", "Spiral" }, Dialogues(sceneName));
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            if (!string.IsNullOrEmpty(original) && System.IO.File.Exists(original))
                EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
            AssetDatabase.SaveAssets();
            Debug.Log("Memento Mori Inspector configuration saved for the five beta scenes.");
        }

        private static string NextScene(string sceneName)
        {
            switch (sceneName)
            {
                case "MainMenu": return "Quarto";
                case "Quarto": return "Labirinto";
                case "Labirinto": return "DominioLua";
                case "DominioLua": return "FinalBeta";
                default: return "MainMenu";
            }
        }

        private static string[] Areas(string sceneName) => sceneName == "DominioLua"
            ? new[] { "Entrada", "JardimLunar", "SalaDosEspelhos", "CorredorIlusorio", "CamaraDoSigilo", "SalaDoFragmento" }
            : new string[0];

        private static string[] Mirrors(string sceneName) => sceneName == "DominioLua"
            ? new[] { "Present", "Delayed", "Ahead", "Absent", "Double", "Room", "Black" }
            : new string[0];

        private static string[] Dialogues(string sceneName)
        {
            switch (sceneName)
            {
                case "Quarto": return new[] { "DLG_ROOM_OPENING", "DLG_ROOM_BOWL_01", "DLG_ROOM_GRIMOIRE_REVEAL", "DLG_DREAM_TRANSITION" };
                case "Labirinto": return new[] { "DLG_LABYRINTH_WAKE", "DLG_POE_REVEAL", "DLG_ANDREALPHUS_01", "DLG_ANDREALPHUS_02", "DLG_SIGIL_HINT_01" };
                case "DominioLua": return new[] { "DLG_MOON_DOMAIN_GATE" };
                default: return new string[0];
            }
        }
    }
}
