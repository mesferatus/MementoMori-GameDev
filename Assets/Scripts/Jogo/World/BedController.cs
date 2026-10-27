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
            InputGate.Instance?.Block("SleepChoice");
            DialogueManager.Instance?.StartDialogue(Resources.Load<DialogueData>("Dialogue/DLG_Q_SLEEP_CONFIRM"));
            if (choicePanel == null) CreateChoicePanel();
            if (choicePanel != null) choicePanel.SetActive(DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
        }

        private void StartSleep()
        {
            if (transitioning || !HasRoomRequirements(GameState.Instance)) return;
            InputGate.Instance?.Release("SleepChoice");
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
            InputGate.Instance?.Release("SleepChoice");
            choicePending = false;
            if (choicePanel != null) choicePanel.SetActive(false);
        }

        private void CreateChoicePanel()
        {
            choicePanel = new GameObject("SleepChoice", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = choicePanel.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;
            var scaler = choicePanel.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(choicePanel.transform, false);
            var image = panel.GetComponent<Image>(); image.color = new Color(.11f, .065f, .17f, .98f);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(.31f, .35f); rect.anchorMax = new Vector2(.69f, .65f);
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            Border(panel.transform, "UpperBorder", new Vector2(.03f, .965f), new Vector2(.97f, .975f));
            Border(panel.transform, "LowerBorder", new Vector2(.03f, .025f), new Vector2(.97f, .035f));
            Border(panel.transform, "LeftBorder", new Vector2(.02f, .04f), new Vector2(.025f, .96f));
            Border(panel.transform, "RightBorder", new Vector2(.975f, .04f), new Vector2(.98f, .96f));
            var label = ChoiceText("Title", panel.transform, "Escolha antes de dormir", 30,
                new Vector2(.07f, .70f), new Vector2(.93f, .94f));
            label.fontStyle = FontStyles.SmallCaps;
            CreateChoiceButton(panel.transform, "Deitar [Y]", new Vector2(.09f, .39f), new Vector2(.91f, .65f), StartSleep);
            CreateChoiceButton(panel.transform, "Verificar o quarto mais uma vez [N]", new Vector2(.09f, .09f), new Vector2(.91f, .35f), CancelSleepChoice);
        }

        private void CreateChoiceButton(Transform parent, string caption, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(caption, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(.20f, .12f, .29f, 1f);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            Border(buttonObject.transform, "TopLine", new Vector2(0, .97f), new Vector2(1, 1));
            Border(buttonObject.transform, "BottomLine", new Vector2(0, 0), new Vector2(1, .03f));
            ChoiceText("Text", buttonObject.transform, caption, 25, new Vector2(.04f, .05f), new Vector2(.96f, .95f));
            var button = buttonObject.GetComponent<Button>();
            button.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(1.25f, 1.15f, 1.35f),
                pressedColor = new Color(.8f, .7f, .95f),
                selectedColor = new Color(1.25f, 1.15f, 1.35f),
                disabledColor = new Color(.5f, .5f, .5f),
                colorMultiplier = 1f,
                fadeDuration = .12f
            };
            button.onClick.AddListener(action);
        }

        private static void Border(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var line = new GameObject(name, typeof(RectTransform), typeof(Image));
            line.transform.SetParent(parent, false);
            var rect = line.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = line.GetComponent<Image>();
            image.color = new Color(.69f, .50f, .69f, .85f);
            image.raycastTarget = false;
        }

        private static TextMeshProUGUI ChoiceText(string name, Transform parent, string caption, float size, Vector2 min, Vector2 max)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(parent, false);
            var rect = label.rectTransform;
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var existing = FindAnyObjectByType<TextMeshProUGUI>();
            label.font = existing != null && existing.font != null ? existing.font : TMP_Settings.defaultFontAsset;
            label.text = caption;
            label.fontSize = size;
            label.color = new Color(.96f, .86f, .96f);
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true;
            label.fontSizeMin = size - 3f;
            label.fontSizeMax = size;
            label.raycastTarget = false;
            return label;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            InputGate.Instance?.Release("SleepChoice");
            InputGate.Instance?.Release("SleepTransition");
            InputGate.Instance?.Release("DreamTransition");
            choicePending = false;
            transitioning = false;
            if (choicePanel != null) Destroy(choicePanel);
        }

        private IEnumerator SleepRoutine()
        {
            InputGate.Instance?.Block("SleepTransition");
            try
            {
                while (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) yield return null;
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
