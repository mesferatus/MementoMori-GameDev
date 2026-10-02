using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.Interaction;
using UnityEngine;
using System.Collections;
using MementoMori.Audio;
using UnityEngine.UI;
using TMPro;

namespace MementoMori.World
{
    public sealed class BedController : MonoBehaviour, IInteractable
    {
        private string nextScene = "Labirinto";
        private int attempts;
        private DialogueData lockedDialogue;
        private bool transitioning;
        private bool choicePending;
        private Vector2 checkpointPosition;
        private GameObject choicePanel;
        [SerializeField, Range(30f, 50f)] private float dreamDuration = 30f;
        public void Configure(string targetScene) { nextScene = targetScene; }
        public string InteractionVerb => "Deitar";
        public int InteractionPriority => 20;
        public bool CanInteract(InteractionContext context) => true;
        public static bool HasRoomRequirements(GameState state) => state != null
            && state.RitualCompleted;
        public void Interact(InteractionContext context)
        {
            if (context.Interactor != null) checkpointPosition = context.Interactor.transform.position;
            var state = GameState.Instance;
            var ready = HasRoomRequirements(state);
            if (!ready)
            {
                attempts++;
                lockedDialogue = Resources.Load<DialogueData>(attempts == 1 ? "Dialogue/DLG_Q_BED_EARLY_01" : "Dialogue/DLG_Q_BED_EARLY_02");
                DialogueManager.Instance?.StartDialogue(lockedDialogue);
                return;
            }
            if (transitioning || choicePending) return;
            ShowSleepChoice();
        }

        private void Update()
        {
            if (!choicePending) return;
            var dialogueOpen = DialogueManager.Instance != null && DialogueManager.Instance.IsOpen;
            if (choicePanel != null) choicePanel.SetActive(!dialogueOpen);
            if (dialogueOpen) return;
            if (Input.GetKeyDown(KeyCode.Y)) StartSleep();
            if (Input.GetKeyDown(KeyCode.N)) CancelSleepChoice();
        }

        private void ShowSleepChoice()
        {
            choicePending = true;
            DialogueManager.Instance?.StartDialogue(Resources.Load<DialogueData>("Dialogue/DLG_Q_SLEEP_CONFIRM"));
            if (choicePanel == null) CreateChoicePanel();
            if (choicePanel != null) choicePanel.SetActive(DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
        }

        private void StartSleep()
        {
            choicePending = false;
            if (choicePanel != null) choicePanel.SetActive(false);
            transitioning = true;
            GameState.Instance?.SetFlag(StoryFlag.RoomSleepUnlocked);
            DialogueManager.Instance?.StartDialogue(Resources.Load<DialogueData>("Dialogue/DLG_Q_DREAM_TRANSITION"));
            StartCoroutine(SleepRoutine());
        }

        public void ConfirmSleep()
        {
            if (!HasRoomRequirements(GameState.Instance)) return;
            StartSleep();
        }

        private void CancelSleepChoice()
        {
            choicePending = false;
            if (choicePanel != null) choicePanel.SetActive(false);
        }

        private void CreateChoicePanel()
        {
            var sprites = Resources.LoadAll<Sprite>("UI/Grimoire/GRIMOIRE_UI_SHEET_V2");
            Sprite FindSprite(string name) => System.Array.Find(sprites, sprite => sprite.name == name);
            choicePanel = new GameObject("SleepChoice", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = choicePanel.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;
            var scaler = choicePanel.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;

            var panel = new GameObject("OrnatePanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(choicePanel.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(.5f, .5f);
            panelRect.sizeDelta = new Vector2(790f, 420f);
            panelRect.anchoredPosition = new Vector2(0f, 70f);
            var panelImage = panel.GetComponent<Image>();
            panelImage.sprite = FindSprite("TAB_NORMAL");
            panelImage.raycastTarget = false;

            var font = FindAnyObjectByType<TextMeshProUGUI>()?.font ?? TMP_Settings.defaultFontAsset;
            ChoiceText("Title", panel.transform, "Escolha antes de dormir", font, 32, new Color(.95f, .84f, .93f),
                new Vector2(.10f, .72f), new Vector2(.90f, .87f));
            CreateChoiceButton(panel.transform, "Deitar [Y]", font, FindSprite("ENTRY_NORMAL"), FindSprite("ENTRY_SELECTED"),
                new Vector2(.22f, .43f), new Vector2(.78f, .68f), StartSleep);
            CreateChoiceButton(panel.transform, "Verificar o quarto mais uma vez [N]", font, FindSprite("ENTRY_NORMAL"), FindSprite("ENTRY_SELECTED"),
                new Vector2(.22f, .17f), new Vector2(.78f, .42f), CancelSleepChoice);
        }

        private static void CreateChoiceButton(Transform parent, string caption, TMP_FontAsset font,
            Sprite normal, Sprite selected, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(caption, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var image = buttonObject.GetComponent<Image>();
            image.sprite = normal;
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            ChoiceText("Label", buttonObject.transform, caption, font, 24, new Color(.23f, .13f, .24f),
                new Vector2(.12f, .10f), new Vector2(.88f, .90f));
            var button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState { highlightedSprite = selected, pressedSprite = selected, selectedSprite = selected };
            button.onClick.AddListener(action);
        }

        private static void ChoiceText(string name, Transform parent, string caption, TMP_FontAsset font,
            float size, Color color, Vector2 min, Vector2 max)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(parent, false);
            label.font = font;
            label.text = caption;
            label.fontSize = size;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true;
            label.fontSizeMin = size - 4f;
            label.fontSizeMax = size;
            label.raycastTarget = false;
            label.rectTransform.anchorMin = min;
            label.rectTransform.anchorMax = max;
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
        }

        private IEnumerator SleepRoutine()
        {
            InputGate.Instance?.Block("SleepTransition");
            try
            {
                var transition = gameObject.GetComponent<DreamTransitionController>() ?? gameObject.AddComponent<DreamTransitionController>();
                yield return transition.Play(dreamDuration);
                GameState.Instance?.SetFlag(StoryFlag.DreamTransitionComplete);
                GameState.Instance?.SaveCheckpoint(nextScene, checkpointPosition);
                StoryProgression.Instance?.SaveCheckpoint(CheckpointId.Sleep);
                SceneLoader.Instance?.LoadScene(nextScene);
            }
            finally
            {
                InputGate.Instance?.Release("SleepTransition");
            }
        }
    }
}
