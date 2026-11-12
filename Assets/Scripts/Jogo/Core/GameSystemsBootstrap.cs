using UnityEngine;
using UnityEngine.SceneManagement;
using MementoMori.Audio;
using MementoMori.UI;

namespace MementoMori.Core
{
    /// <summary>Ensures all core session services exist in every bootstrap scene.</summary>
    public sealed class GameSystemsBootstrap : MonoBehaviour
    {
        private static bool attemptedRestore;
        private void Awake()
        {
            var persistentSystems = GameObject.Find("MementoMoriSystems");
            if (persistentSystems == null)
            {
                persistentSystems = new GameObject("MementoMoriSystems");
                DontDestroyOnLoad(persistentSystems);
            }

            if (GameState.Instance == null)
                persistentSystems.AddComponent<GameState>();
            if (StoryProgression.Instance == null)
                persistentSystems.AddComponent<StoryProgression>();
            if (AccessibilitySettings.Instance == null)
                persistentSystems.AddComponent<AccessibilitySettings>();
            if (InputGate.Instance == null)
                persistentSystems.AddComponent<InputGate>();
            if (SceneLoader.Instance == null)
                persistentSystems.AddComponent<SceneLoader>();
            if (GameManager.Instance == null)
                persistentSystems.AddComponent<GameManager>();
            if (ObjectiveToastController.Instance == null)
                persistentSystems.AddComponent<ObjectiveToastController>();
        }

        private void Start()
        {
#if UNITY_EDITOR
            var evidencePath = UnityEditor.EditorPrefs.GetString("MementoMori.CtEvidencePath", string.Empty);
            if (!string.IsNullOrWhiteSpace(evidencePath))
            {
                UnityEditor.EditorPrefs.DeleteKey("MementoMori.CtEvidencePath");
                // Local acceptance tooling; absent from the published repository.
                System.Type.GetType("MementoMori.Verification.CtEvidenceRunner, MementoMori.Runtime")
                    ?.GetMethod("StartEditorEvidence")?.Invoke(null, new object[] { evidencePath });
            }
#endif
            if (!attemptedRestore)
            {
                attemptedRestore = true;
                GameState.Instance?.RestoreCheckpoint();
            }
            EnsureSharedSceneInfrastructure();
        }

        private static void EnsureSharedSceneInfrastructure()
        {
            GameplayFrame.EnsureForScene();
            MementoMori.Interaction.InteractionHighlighter.EnsureForScene();
            var camera = Camera.main ?? Object.FindAnyObjectByType<Camera>();
            if (camera != null && Object.FindAnyObjectByType<AudioListener>() == null)
                camera.gameObject.AddComponent<AudioListener>();

            var loop = SceneManager.GetActiveScene().name switch
            {
                "Quarto" => "Music/music_quarto_gothic_piano",
                "Labirinto" => "Music/music_labirinto_gothic",
                "DominioLua" => "Music/music_lua_gothic_bells",
                "MainMenu" => "Music/music_menu_gothic_organ",
                "FinalBeta" => "Music/music_credits_memoria",
                _ => null
            };
            if (loop != null)
                RuntimeAudio.PlayLoop(loop);
        }
    }
}
