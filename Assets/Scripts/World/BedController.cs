using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.Interaction;
using UnityEngine;
using System.Collections;
using MementoMori.Audio;
using UnityEngine.UI;

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
            && state.HasFlag(StoryFlag.RoomBowlExamined)
            && state.HasFlag(StoryFlag.RoomToyExamined)
            && state.HasFlag(StoryFlag.RoomPhotoExamined)
            && state.HasFlag(StoryFlag.RoomGrimoireRead)
            && state.HasFlag(StoryFlag.RoomWindowSecured)
            && state.HasFlag(StoryFlag.RoomRitualItemStored);
        public void Interact(InteractionContext context)
        {
            if (context.Interactor != null) checkpointPosition = context.Interactor.transform.position;
            var state = GameState.Instance;
            var ready = HasRoomRequirements(state);
            if (!ready)
            {
                attempts++;
                lockedDialogue = Resources.Load<DialogueData>(attempts == 1 ? "Dialogue/DLG_ROOM_BED_LOCKED_01" : "Dialogue/DLG_ROOM_BED_LOCKED_02");
                DialogueManager.Instance?.StartDialogue(lockedDialogue);
                return;
            }
            if (transitioning || choicePending) return;
            ShowSleepChoice();
        }

        private void Update()
        {
            if (!choicePending || DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return;
            if (Input.GetKeyDown(KeyCode.Y)) StartSleep();
            if (Input.GetKeyDown(KeyCode.N)) CancelSleepChoice();
        }

        private void ShowSleepChoice()
        {
            choicePending = true;
            DialogueManager.Instance?.StartDialogue(Resources.Load<DialogueData>("Dialogue/DLG_ROOM_SLEEP_CONFIRM"));
            if (choicePanel == null) CreateChoicePanel();
            if (choicePanel != null) choicePanel.SetActive(true);
        }

        private void StartSleep()
        {
            choicePending = false;
            if (choicePanel != null) choicePanel.SetActive(false);
            transitioning = true;
            GameState.Instance?.SetFlag(StoryFlag.RoomSleepUnlocked);
            GameState.Instance?.SetRitualCompleted();
            DialogueManager.Instance?.StartDialogue(Resources.Load<DialogueData>("Dialogue/DLG_DREAM_TRANSITION"));
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
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;
            choicePanel = new GameObject("SleepChoice", typeof(RectTransform), typeof(Image));
            choicePanel.transform.SetParent(canvas.transform, false);
            var image = choicePanel.GetComponent<Image>(); image.color = new Color(.06f, .04f, .1f, .96f);
            var rect = choicePanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(.28f, .38f); rect.anchorMax = new Vector2(.72f, .55f);
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var label = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            label.transform.SetParent(choicePanel.transform, false);
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 18; label.color = Color.white;
            label.alignment = TextAnchor.MiddleCenter; label.text = "Escolha antes de dormir";
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(.05f, .67f); labelRect.anchorMax = new Vector2(.95f, .96f); labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;
            CreateChoiceButton("Deitar [Y]", new Vector2(.08f, .36f), new Vector2(.92f, .61f), StartSleep);
            CreateChoiceButton("Verificar o quarto mais uma vez [N]", new Vector2(.08f, .08f), new Vector2(.92f, .32f), CancelSleepChoice);
        }

        private void CreateChoiceButton(string caption, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(caption, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(choicePanel.transform, false);
            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(.26f, .20f, .38f, 1f);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var text = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            text.transform.SetParent(buttonObject.transform, false);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = 14; text.color = Color.white; text.alignment = TextAnchor.MiddleCenter; text.text = caption;
            text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one; text.rectTransform.offsetMin = Vector2.zero; text.rectTransform.offsetMax = Vector2.zero;
            buttonObject.GetComponent<Button>().onClick.AddListener(action);
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
