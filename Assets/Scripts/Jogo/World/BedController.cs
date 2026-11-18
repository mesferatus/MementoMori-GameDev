using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.Interaction;
using MementoMori.UI;
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
        private DialogueData transitionDialogue;
        private bool transitioning;
        private bool choicePending;
        private GameObject choicePanel;
        [SerializeField, Range(6f, 10f)] private float dreamDuration = 10f;
        public void Configure(string targetScene) { nextScene = targetScene; }
        public string InteractionVerb => "Deitar";
        public int InteractionPriority => 20;
        public bool CanInteract(InteractionContext context) => true;
        public static bool HasRoomRequirements(GameState state) => state != null
            && state.RitualCompleted;
        public void Interact(InteractionContext context)
        {
            var state = GameState.Instance;
            var ready = HasRoomRequirements(state);
            if (!ready)
            {
                attempts++;
                if (state != null && state.GetCounter("event.Q01.Bed") == 0)
                {
                    var intro = V61EventCatalog.CreateDialogueSegment("Q01", "Q01.Bed", 0, 2);
                    var manager = DialogueManager.Instance;
                    if (intro != null && manager != null && manager.TryStartDialogue(intro))
                    {
                        void Complete()
                        {
                            manager.OnDialogueCompleted -= Complete;
                            state.SetCounter("event.Q01.Bed", 1);
                            Destroy(intro);
                        }
                        manager.OnDialogueCompleted += Complete;
                        return;
                    }
                    if (intro != null) Destroy(intro);
                }
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
            // A/B on a controller; the press that opened the choice must not also answer it.
            var fresh = Time.frameCount == choiceOpenedFrame;
            if (Input.GetKeyDown(KeyCode.Y) || !fresh && GamepadInput.ConfirmDown()) StartSleep();
            else if (Input.GetKeyDown(KeyCode.N) || GamepadInput.CancelDown()) CancelSleepChoice();
        }

        private int choiceOpenedFrame = -1;
        private void ShowSleepChoice()
        {
            choicePending = true;
            choiceOpenedFrame = Time.frameCount;
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
            transitionDialogue = V61EventCatalog.CreateDialogue("C03");
            DialogueManager.Instance?.StartDialogue(transitionDialogue);
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
            MenuPanelStyle.Veil(choicePanel.transform, .55f);
            var rect = MenuPanelStyle.Panel(choicePanel.transform, new Vector2(1000f, 420f));
            var panel = rect.gameObject;
            var label = ChoiceText("Title", panel.transform, "Escolha antes de dormir", 36,
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
            ChoiceText("Text", buttonObject.transform, caption, 31, new Vector2(.04f, .05f), new Vector2(.96f, .95f));
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
            MenuButtonStyle.Apply(button, null, .6f);
            button.onClick.AddListener(action);
        }

        private static TextMeshProUGUI ChoiceText(string name, Transform parent, string caption, float size, Vector2 min, Vector2 max)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(parent, false);
            var rect = label.rectTransform;
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            label.font = MenuPanelStyle.HeadingFont ?? TMP_Settings.defaultFontAsset;
            label.text = Loc.T(caption);
            LocalizedText.Bind(label, () => Loc.T(caption));
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
            if (transitionDialogue != null) Destroy(transitionDialogue);
        }

        private IEnumerator SleepRoutine()
        {
            InputGate.Instance?.Block("SleepTransition");
            try
            {
                while (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) yield return null;
                GameState.Instance?.SetCounter("event.C03", 1);
                if (transitionDialogue != null) { Destroy(transitionDialogue); transitionDialogue = null; }
                // The Labyrinth is a 100 MB map: read it while the dream veil plays.
                SceneLoader.Instance?.Preload(nextScene);
                GamepadInput.Rumble(.18f, 0f, .5f);
                var transition = gameObject.GetComponent<DreamTransitionController>() ?? gameObject.AddComponent<DreamTransitionController>();
                yield return transition.Play(dreamDuration);
                GameState.Instance?.SetFlag(StoryFlag.DreamTransitionComplete);
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
