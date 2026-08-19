using MementoMori.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.UI
{
    public sealed class PauseMenuController : MonoBehaviour
    {
        private const string PauseGate = "Pause";
        [SerializeField] private GameObject panel;
        private bool paused;
        public void ConfigurePanel(GameObject pausePanel) => panel = pausePanel;
        private void Update() { if (Input.GetKeyDown(KeyCode.Escape)) Toggle(); }
        public void Toggle()
        {
            if (SceneLoader.Instance == null) return;
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
            if (paused) InputGate.Instance?.Block(PauseGate); else InputGate.Instance?.Release(PauseGate);
            if (panel != null) panel.SetActive(paused);
        }
        public void RestartScene()
        {
            Time.timeScale = 1f;
            InputGate.Instance?.ClearAll();
            var state = GameState.Instance;
            if (state != null && state.RestoreCheckpoint() && !string.IsNullOrEmpty(state.CheckpointScene))
                SceneLoader.Instance?.LoadScene(state.CheckpointScene);
            else
                SceneLoader.Instance?.LoadScene(SceneManager.GetActiveScene().name);
        }
        public void ReturnToMenu() { Time.timeScale = 1f; InputGate.Instance?.ClearAll(); GameManager.Instance?.ReturnToMenu(); }
    }
}
