using UnityEngine;
using UnityEngine.SceneManagement;
using MementoMori.Audio;

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
        }

        private void Start()
        {
#if UNITY_EDITOR
            var evidencePath = UnityEditor.EditorPrefs.GetString("MementoMori.CtEvidencePath", string.Empty);
            if (!string.IsNullOrWhiteSpace(evidencePath))
            {
                UnityEditor.EditorPrefs.DeleteKey("MementoMori.CtEvidencePath");
                MementoMori.Verification.CtEvidenceRunner.StartEditorEvidence(evidencePath);
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
            var camera = Camera.main ?? Object.FindAnyObjectByType<Camera>();
            if (camera != null && Object.FindAnyObjectByType<AudioListener>() == null)
                camera.gameObject.AddComponent<AudioListener>();

            var loop = SceneManager.GetActiveScene().name switch
            {
                "Quarto" => "01_room_ambience",
                "Labirinto" => "02_labyrinth_drone",
                "DominioLua" => "03_moon_domain_loop",
                "MainMenu" => "20_menu_theme",
                "FinalBeta" => "19_beta_ending_stinger",
                _ => null
            };
            if (loop != null)
                RuntimeAudio.PlayLoop(loop);
        }
    }
}
