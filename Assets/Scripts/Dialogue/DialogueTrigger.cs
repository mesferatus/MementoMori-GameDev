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

        private void Awake()
        {
            var v3Id = V3DialogueId(gameObject.name);
            if (!string.IsNullOrEmpty(v3Id)) dialogue = Resources.Load<DialogueData>("Dialogue/" + v3Id);
            // V3 makes Poe's bowl and toy optional; legacy prefab requirements must not gate the grimoire.
            if (gameObject.name == "Grimoire") requiredFlags = new[] { StoryFlag.RoomPhotoExamined };
        }

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
        public bool CanInteract(InteractionContext context) => !used || !oneShot || repeatDialogue != null || CanUseAlteredDialogue();

        private bool CanUseAlteredDialogue()
        {
            var state = GameState.Instance;
            return state != null && state.HasFlag(StoryFlag.RoomGrimoireRead)
                && (gameObject.name == "Photo" || gameObject.name == "PoeBowl" || gameObject.name == "PoeToy");
        }

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
            var selected = !unlocked ? lockedDialogue ?? dialogue : wasUsed && repeatDialogue != null ? repeatDialogue : AlteredDialogue() ?? dialogue;
            DialogueManager.Instance.StartDialogue(selected);
        }

        private DialogueData AlteredDialogue()
        {
            var state = GameState.Instance;
            if (state == null || !state.HasFlag(StoryFlag.RoomGrimoireRead)) return null;
            var id = gameObject.name switch
            {
                "Photo" => "DLG_Q_PHOTO_ALTERED_01",
                "PoeBowl" => "DLG_Q_BOWL_ALTERED_01",
                "PoeToy" => "DLG_Q_TOY_ALTERED_01",
                _ => string.Empty
            };
            return string.IsNullOrEmpty(id) ? null : Resources.Load<DialogueData>("Dialogue/" + id);
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

        private static string V3DialogueId(string objectName) => objectName switch
        {
            "Photo" => "DLG_Q_PHOTO_01",
            "Grimoire" => "DLG_Q_GRIMOIRE_01",
            "Window" => "DLG_Q_WINDOW_01",
            "RitualItem" => "DLG_Q_RITUAL_ITEM_01",
            "PoeBowl" => "DLG_Q_BOWL_01",
            "PoeToy" => "DLG_Q_TOY_01",
            "Desk" => "DLG_Q_DESK_01",
            "Vial" => "DLG_Q_VIAL_01",
            "EmptyChamber" => "DLG_L_EMPTY_CHAMBER",
            "AndrealphusAlcove" => "DLG_L_ANDREALPHUS_01",
            _ => string.Empty
        };
    }
}
