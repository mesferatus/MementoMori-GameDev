using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using MementoMori.Core;
using MementoMori.World;

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
                configuration.Configure(sceneName, NextScene(sceneName), sceneName == "Quarto", Areas(sceneName), Mirrors(sceneName), new[] { "Delayed", "Ahead", "Absent" }, new[] { "Minguante", "Grimório", "SUSTENTAR" }, Dialogues(sceneName));
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            if (!string.IsNullOrEmpty(original) && System.IO.File.Exists(original))
                EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
            AssetDatabase.SaveAssets();
            Debug.Log("Memento Mori Inspector configuration saved for the five beta scenes.");
        }

        public static void RepairQuartoRitualBinding()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Quarto.unity", OpenSceneMode.Single);
            RitualController ritual = null;
            foreach (var transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (transform.name != "Q04_Ritual" || !transform.gameObject.activeInHierarchy) continue;
                ritual = transform.GetComponent<RitualController>();
                if (ritual == null) ritual = transform.gameObject.AddComponent<RitualController>();
                var trigger = transform.GetComponent<CircleCollider2D>();
                if (trigger == null) trigger = transform.gameObject.AddComponent<CircleCollider2D>();
                trigger.isTrigger = true;
                trigger.radius = 2.2f;
                break;
            }

            if (ritual == null)
                throw new System.InvalidOperationException("Active Q04_Ritual container was not found in Quarto.");

            ritual.Configure("Labirinto", 1f);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Memento Mori V3 Quarto ritual binding repaired.");
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
            ? new[] { "Entrada", "JardimLunar", "SalaDosEspelhos", "CorredorIlusorio", "GaleriaDosCiclos", "CamaraDoSigilo", "SalaDoFragmento", "PortalFinal" }
            : new string[0];

        private static string[] Mirrors(string sceneName) => sceneName == "DominioLua"
            ? new[] { "Present", "Delayed", "Ahead", "Absent", "Double", "Room", "Black" }
            : new string[0];

        private static string[] Dialogues(string sceneName)
        {
            switch (sceneName)
            {
                case "Quarto": return new[]
                {
                    "DLG_Q_OPENING_01", "DLG_Q_BED_EARLY_01", "DLG_Q_BED_EARLY_02", "DLG_Q_PHOTO_01",
                    "DLG_Q_WINDOW_01", "DLG_Q_DESK_01", "DLG_Q_VIAL_01", "DLG_Q_GRIMOIRE_01",
                    "DLG_Q_PHOTO_ALTERED_01", "DLG_Q_RITUAL_ITEM_01", "DLG_Q_CANDLE_FAIL_WIND",
                    "DLG_Q_WINDOW_SECURE", "DLG_Q_CIRCLE_NEEDS_ANCHOR", "DLG_Q_CANDLE_BEFORE_ANCHOR",
                    "DLG_Q_RITUAL_PROGRESS", "DLG_Q_RITUAL_COMPLETE", "DLG_Q_SLEEP_CONFIRM", "DLG_Q_DREAM_TRANSITION"
                };
                case "Labirinto": return new[]
                {
                    "DLG_L_WAKE_01", "DLG_L_POE_SIGNAL_01", "DLG_L_FALSE_DOOR_01", "DLG_L_FALSE_DOOR_02",
                    "DLG_L_FALSE_DOOR_03", "DLG_L_VOICE_WELL_01", "DLG_L_VOICE_WELL_02", "DLG_L_VOICE_WELL_03",
                    "DLG_L_VOICE_WELL_04", "DLG_L_POE_REVEAL", "DLG_L_EMPTY_CHAMBER", "DLG_L_ANDREALPHUS_01",
                    "DLG_L_ECHO_INTRO", "DLG_L_ECHO_WRONG", "DLG_L_ECHO_HINT_POE", "DLG_L_ECHO_COMPLETE",
                    "DLG_L_MOON_GATE_01", "DLG_L_MOON_GATE_OPEN"
                };
                case "DominioLua": return new[]
                {
                    "DLG_D_ENTRY_01", "DLG_D_INSCRIPTION_01", "DLG_D_GARDEN_CRESCENT_INTRO",
                    "DLG_D_GARDEN_CRESCENT_FAIL", "DLG_D_GARDEN_CRESCENT_SUCCESS", "DLG_D_GARDEN_FULL_INTRO",
                    "DLG_D_GARDEN_FULL_NEAR", "DLG_D_GARDEN_FULL_SUCCESS", "DLG_D_GARDEN_WANING_INTRO",
                    "DLG_D_GARDEN_WANING_WRONG", "DLG_D_GARDEN_WANING_SUCCESS", "DLG_D_GARDEN_COMPLETE",
                    "DLG_D_MIRROR_PRESENT", "DLG_D_MIRROR_DELAYED", "DLG_D_MIRROR_AHEAD", "DLG_D_MIRROR_NO_POE",
                    "DLG_D_MIRROR_TWO_POES", "DLG_D_MIRROR_ROOM", "DLG_D_MIRROR_WRONG", "DLG_D_MIRROR_HINT_1",
                    "DLG_D_MIRROR_SOLVED", "DLG_D_BLACK_MIRROR_MEMORY", "DLG_D_ANDREALPHUS_02", "DLG_D_SIGIL_INTRO",
                    "DLG_D_SIGIL_PHASE_WRONG", "DLG_D_SIGIL_PHASE_OK", "DLG_D_SIGIL_MEMORY_EYE_WRONG",
                    "DLG_D_SIGIL_MEMORY_OK", "DLG_D_SIGIL_INTENTION_WRONG", "DLG_D_SIGIL_INTENTION_OK",
                    "DLG_D_SIGIL_COMPLETE", "DLG_D_FRAGMENT_APPROACH", "DLG_D_FRAGMENT_TOUCH", "DLG_D_FRAGMENT_MEMORY",
                    "DLG_D_FRAGMENT_COLLECT", "DLG_D_POE_VANISH", "DLG_D_FINAL_PORTAL"
                };
                case "FinalBeta": return new[] { "DLG_F_WAKE", "DLG_F_ROOM_AFTER", "DLG_F_FRAGMENT_CHECK", "DLG_F_POE_SOUND", "DLG_F_END_CARD" };
                default: return new string[0];
            }
        }
    }
}
