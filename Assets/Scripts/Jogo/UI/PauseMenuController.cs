using System.Collections.Generic;
using MementoMori.Core;
using MementoMori.Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MementoMori.UI
{
    public sealed class PauseMenuController : MonoBehaviour
    {
        private const string PauseGate = "Pause";
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject dimmer;
        private bool paused;
        private string titleKey = "PAUSA";
        private GameObject mainButtons;
        private GameObject slotButtons;
        private TMP_Text title;
        private SettingsPanelView settingsView;
        private readonly Dictionary<Canvas, bool> siblingCanvasStates = new Dictionary<Canvas, bool>();
        public void ConfigurePanel(GameObject pausePanel) => panel = pausePanel;
        private void Start()
        {
            if (panel == null) panel = transform.Find("PauseContainer")?.gameObject;
            if (panel == null) return;
            title = panel.transform.Find("Title")?.GetComponent<TMP_Text>();
            var buttons = panel.transform.Find("ButtonsContainer");
            var template = buttons?.Find("Button_Continuar")?.GetComponent<Button>();
            if (buttons == null || template == null) return;
            mainButtons = buttons.gameObject;
            var save = MakeButton(template, buttons, "Button_Salvar", "SALVAR", ShowSlots);
            save.transform.SetSiblingIndex(1);
            var mainRect = (RectTransform)buttons;
            mainRect.anchoredPosition += new Vector2(0f, 35f);
            mainRect.sizeDelta = new Vector2(mainRect.sizeDelta.x, 303f);
            var mainGroup = mainButtons.GetComponent<VerticalLayoutGroup>();
            mainGroup.spacing = 7f;
            foreach (Transform child in buttons)
            {
                var element = child.GetComponent<LayoutElement>();
                if (element == null) continue;
                element.minHeight = 55f;
                element.preferredHeight = 55f;
            }
            slotButtons = new GameObject("SlotButtons", typeof(RectTransform), typeof(VerticalLayoutGroup));
            slotButtons.transform.SetParent(buttons.parent, false);
            var slotRect = (RectTransform)slotButtons.transform;
            var sourceRect = (RectTransform)buttons;
            slotRect.anchorMin = sourceRect.anchorMin;
            slotRect.anchorMax = sourceRect.anchorMax;
            slotRect.pivot = sourceRect.pivot;
            slotRect.anchoredPosition = sourceRect.anchoredPosition;
            slotRect.sizeDelta = sourceRect.sizeDelta;
            var group = slotButtons.GetComponent<VerticalLayoutGroup>();
            group.childAlignment = TextAnchor.UpperCenter;
            group.spacing = 7f;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            for (var slot = 1; slot <= 3; slot++)
            {
                var selectedSlot = slot;
                var slotButton = MakeButton(template, slotRect, "Button_Slot" + slot, "SLOT " + slot,
                    () => SelectSlot(selectedSlot));
                SetCompactHeight(slotButton);
            }
            SetCompactHeight(MakeButton(template, slotRect, "Button_Voltar", "VOLTAR", ShowMain));
            // Same button look as the main menu.
            MenuButtonStyle.ApplyAll(buttons, null, .6f);
            MenuButtonStyle.ApplyAll(slotRect, null, .6f);
            slotButtons.SetActive(false);
            settingsView = gameObject.AddComponent<SettingsPanelView>();
            settingsView.Configure(panel, mainButtons, slotButtons, template, title);
            // The scene-authored pause labels and the created SALVAR/VOLTAR follow language switches.
            foreach (var label in mainButtons.GetComponentsInChildren<TMP_Text>(true)) LocalizedText.Bind(label);
            if (slotButtons.transform.Find("Button_Voltar") is Transform backButton)
                LocalizedText.Bind(backButton.GetComponentInChildren<TMP_Text>(true));
            Loc.LanguageChanged += UpdateTitle;
            UpdateTitle();
        }
        private static void SetCompactHeight(Button button)
        {
            var element = button.GetComponent<LayoutElement>();
            if (element == null) return;
            element.minHeight = 55f;
            element.preferredHeight = 55f;
        }
        private static Button MakeButton(Button template, Transform parent, string name, string caption, UnityEngine.Events.UnityAction action)
        {
            var button = Instantiate(template, parent);
            button.name = name;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(action);
            button.GetComponent<MenuButtonFeedback>()?.Rebind();
            var label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) { LocalizedText.Unbind(label); label.text = caption; }
            return button;
        }
        private void UpdateTitle() { if (title != null) title.text = Loc.T(titleKey); }
        private void ShowSlots()
        {
            if (slotButtons == null || mainButtons == null) return;
            titleKey = "SALVAR"; UpdateTitle();
            var store = new SaveSlotStore();
            for (var slot = 1; slot <= 3; slot++)
            {
                var button = slotButtons.transform.Find("Button_Slot" + slot)?.GetComponent<Button>();
                if (button == null) continue;
                var exists = store.TryLoad(slot, out var snapshot, out _);
                button.interactable = true;
                var label = button.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = "SLOT " + slot + " - " + (exists ? Loc.T(snapshot.scene.ToUpperInvariant()) : Loc.T("VAZIO"));
            }
            mainButtons.SetActive(false);
            slotButtons.SetActive(true);
        }
        private void ShowMain()
        {
            if (slotButtons != null) slotButtons.SetActive(false);
            if (mainButtons != null) mainButtons.SetActive(true);
            titleKey = "PAUSA"; UpdateTitle();
        }
        private void SelectSlot(int slot)
        {
            var manager = GameManager.Instance;
            if (manager == null) return;
            var success = manager.SaveGame(slot);
            if (success) { ShowMain(); return; }
            titleKey = "ERRO"; UpdateTitle();
            Debug.LogWarning(manager.LastSaveError, this);
        }
        private void Update()
        {
            if (InputBindings.Down(GameAction.Pause) && (settingsView == null || !settingsView.IsCapturing && settingsView.CaptureConsumedFrame != Time.frameCount)
                && !MementoMori.Regions.V6Challenge.AnyOpen
                && (DialogueManager.Instance == null || !DialogueManager.Instance.IsCinematic)
                && DialogueManager.EscapeConsumedFrame != Time.frameCount
                && MementoMori.Regions.V6Challenge.EscapeConsumedFrame != Time.frameCount
                && !V61ChoicePresenter.IsOpen
                && !MementoMori.World.GrimoireScreen.AnyOpen
                && MementoMori.World.GrimoireScreen.EscapeConsumedFrame != Time.frameCount)
            {
                if (settingsView != null && settingsView.IsOpen) settingsView.Close();
                else Toggle();
            }
        }
        public void Toggle()
        {
            paused = !paused;
            if (!paused) { settingsView?.Close(); ShowMain(); }
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
            Loc.LanguageChanged -= UpdateTitle;
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

