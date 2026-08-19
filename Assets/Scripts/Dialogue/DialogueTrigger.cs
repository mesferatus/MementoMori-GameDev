using MementoMori.Interaction;
using MementoMori.Core;
using UnityEngine;
using UnityEngine.Events;
using MementoMori.Audio;

namespace MementoMori.Dialogue
{
    public sealed class DialogueTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueData dialogue;
        [SerializeField] private DialogueData lockedDialogue;
        [SerializeField] private DialogueData repeatDialogue;
        [SerializeField] private StoryFlag[] requiredFlags;
        [SerializeField] private bool triggerOnEnter;
        [SerializeField] private bool oneShot = true;
        [SerializeField] private string interactionVerb = "Falar";
        [SerializeField] private UnityEvent onCompleted;
        private bool used;

        public void Configure(DialogueData data, bool onEnter, bool isOneShot, string verb)
        {
            dialogue = data;
            triggerOnEnter = onEnter;
            oneShot = isOneShot;
            interactionVerb = verb;
        }

        public void ConfigureRequirements(DialogueData locked, params StoryFlag[] flags)
        {
            lockedDialogue = locked;
            requiredFlags = flags;
        }
        public void ConfigureRepeatDialogue(DialogueData repeat) => repeatDialogue = repeat;

        public string InteractionVerb => interactionVerb;
        public int InteractionPriority => 0;
        public bool CanInteract(InteractionContext context) => !used || !oneShot || repeatDialogue != null;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggerOnEnter && other.CompareTag("Player"))
                TriggerDialogue();
        }

        public void Interact(InteractionContext context) => TriggerDialogue();

        private void TriggerDialogue()
        {
            if (!CanInteract(default) || DialogueManager.Instance == null)
                return;
            var unlocked = RequirementsMet();
            var wasUsed = used;
            if (unlocked) { MarkStoryState(); used = true; }
            void Complete() { DialogueManager.Instance.OnDialogueCompleted -= Complete; onCompleted?.Invoke(); }
            DialogueManager.Instance.OnDialogueCompleted += Complete;
            var selected = !unlocked ? lockedDialogue ?? dialogue : wasUsed && repeatDialogue != null ? repeatDialogue : dialogue;
            DialogueManager.Instance.StartDialogue(selected);
        }

        private bool RequirementsMet()
        {
            if (requiredFlags == null || requiredFlags.Length == 0) return true;
            var state = GameState.Instance;
            if (state == null) return false;
            foreach (var flag in requiredFlags) if (!state.HasFlag(flag)) return false;
            return true;
        }

        private void MarkStoryState()
        {
            var state = GameState.Instance;
            if (state == null) return;
            switch (gameObject.name)
            {
                case "PoeBowl": state.SetFlag(StoryFlag.RoomBowlExamined); break;
                case "PoeToy": state.SetFlag(StoryFlag.RoomToyExamined); break;
                case "Photo": state.SetFlag(StoryFlag.RoomPhotoExamined); break;
                case "Window": state.SetFlag(StoryFlag.RoomWindowSecured); break;
                case "Candles": state.SetFlag(StoryFlag.RoomCandlesDone); RuntimeAudio.PlayOneShot("18_candle_extinguish", .55f); break;
                case "RitualItem": state.SetFlag(StoryFlag.RoomRitualItemStored); break;
                case "Grimoire": state.SetFlag(StoryFlag.RoomGrimoireRead); break;
                case "AndrealphusAlcove": state.SetFlag(StoryFlag.AndrealphusMeeting01Complete); break;
                case "EchoCorridor": state.SetFlag(StoryFlag.EchoTrial01Complete); break;
                case "EmptyChamber": state.SetFlag(StoryFlag.EmptyChamberComplete); break;
                case "GalleryHiddenWall": state.SetFlag(StoryFlag.HiddenDoorRevealed); break;
            }
        }
    }
}
