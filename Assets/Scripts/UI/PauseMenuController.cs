using System.Collections.Generic;
using MementoMori.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.UI
{
    public sealed class PauseMenuController : MonoBehaviour
    {
        private const string PauseGate = "Pause";
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject dimmer;
        private bool paused;
        private readonly Dictionary<Canvas, bool> siblingCanvasStates = new Dictionary<Canvas, bool>();
        public void ConfigurePanel(GameObject pausePanel) => panel = pausePanel;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)
                && !MementoMori.World.GrimoireScreen.AnyOpen
                && MementoMori.World.GrimoireScreen.EscapeConsumedFrame != Time.frameCount)
                Toggle();
        }
        public void Toggle()
        {
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
            if (paused) InputGate.Instance?.Block(PauseGate); else InputGate.Instance?.Release(PauseGate);
            if (paused) HideSiblingCanvases(); else RestoreSiblingCanvases();
            if (panel != null) panel.SetActive(paused);
            if (dimmer != null) dimmer.SetActive(paused);
        }
        private void HideSiblingCanvases()
        {
            siblingCanvasStates.Clear();
            if (transform.parent == null) return;
            foreach (Transform sibling in transform.parent)
            {
                if (sibling == transform) continue;
                var canvas = sibling.GetComponent<Canvas>();
                if (canvas == null) continue;
                siblingCanvasStates[canvas] = canvas.enabled;
                canvas.enabled = false;
            }
        }
        private void RestoreSiblingCanvases()
        {
            foreach (var entry in siblingCanvasStates)
                if (entry.Key != null) entry.Key.enabled = entry.Value;
            siblingCanvasStates.Clear();
        }
        private void OnDestroy()
        {
            RestoreSiblingCanvases();
            if (!paused) return;
            Time.timeScale = 1f;
            InputGate.Instance?.Release(PauseGate);
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
