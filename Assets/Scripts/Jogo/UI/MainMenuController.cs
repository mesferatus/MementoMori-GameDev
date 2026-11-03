using MementoMori.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MementoMori.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject creditsPanel;
        private SettingsPanelView settingsView;
        private void Start()
        {
            // Reuse the approved menu button artwork and layout for Continue.
            var play = GameObject.Find("Button_Jogar")?.GetComponent<Button>();
            if (play == null || play.transform.parent == null || play.transform.parent.name != "MainMenuButtons") return;
            var button = Instantiate(play, play.transform.parent);
            button.name = "Button_Continuar";
            button.transform.SetSiblingIndex(play.transform.GetSiblingIndex() + 1);
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(Continue);
            var label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = "CONTINUAR";
            var saves = new SaveSlotStore();
            button.interactable = saves.Exists(0) || saves.Exists(1) || saves.Exists(2) || saves.Exists(3);
            settingsView = gameObject.AddComponent<SettingsPanelView>();
            settingsView.Configure(play.transform.parent.gameObject, play.transform.parent.gameObject,
                null, play, label);
            var shortcut = play.transform.parent.Find("Button_Configuracoes") as RectTransform;
            if (shortcut != null)
            {
                // The existing settings cog belongs to the canvas, outside the menu's vertical layout.
                shortcut.SetParent(play.transform.parent.parent, false);
                shortcut.anchorMin = shortcut.anchorMax = Vector2.one;
                shortcut.anchoredPosition = new Vector2(-78f, -74f);
            }
        }
        private void Update()
        {
            if (settingsView != null && settingsView.IsOpen && !settingsView.IsCapturing
                && settingsView.CaptureConsumedFrame != Time.frameCount && Input.GetKeyDown(KeyCode.Escape))
                settingsView.Close();
        }
        public void ConfigureCredits(GameObject panel) => creditsPanel = panel;
        public void Play() => GameManager.Instance?.StartNewGame();
        public void Continue()
        {
            var manager = GameManager.Instance;
            if (manager != null && !manager.ContinueGame())
                Debug.LogWarning(manager.LastSaveError, this);
        }
        public void ShowCredits() { if (creditsPanel != null) creditsPanel.SetActive(true); }
        public void HideCredits() { if (creditsPanel != null) creditsPanel.SetActive(false); }
        public void Quit()
        {
#if UNITY_EDITOR
            Debug.Log("Quit requested; ignored in the Unity Editor.");
#else
            Application.Quit();
#endif
        }
    }
}
