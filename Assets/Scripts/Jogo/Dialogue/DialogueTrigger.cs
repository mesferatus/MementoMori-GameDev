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
            var intro = RoomIntroSegment(gameObject.name);
            if (intro != null) dialogue = intro;
            var galleryId = GalleryEventId(gameObject.name);
            if (galleryId != null)
            {
                repeatDialogue = dialogue;
                dialogue = V61EventCatalog.CreateDialogue(galleryId);
                oneShot = false;
            }
            if (gameObject.name == "EmptyChamber") dialogue = V61EventCatalog.CreateDialogue("L02");
            // V3 makes Poe's bowl and toy optional; legacy prefab requirements must not gate the grimoire.
            if (gameObject.name == "Grimoire") requiredFlags = new[] { StoryFlag.RoomPhotoExamined };
        }

        private void Start()
        {
            if (dialogue != null && (dialogue.SequenceId.StartsWith("Q01.")
                || dialogue.SequenceId.StartsWith("GALLERY_"))
                && GameState.Instance != null && GameState.Instance.GetCounter("event." + dialogue.SequenceId) > 0)
            {
                used = true;
                Destroy(dialogue);
                dialogue = null;
            }
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
        public bool CanInteract(InteractionContext context) =>
            !(gameObject.name == "EmptyChamber" && GameState.Instance != null
                && GameState.Instance.HasFlag(StoryFlag.EmptyChamberComplete))
            && (!used || !oneShot || repeatDialogue != null || CanUseAlteredDialogue());

        private bool CanUseAlteredDialogue()
        {
            var state = GameState.Instance;
            return state != null && state.HasFlag(StoryFlag.RoomGrimoireRead)
                && (gameObject.name == "Photo" || gameObject.name == "Window"
                    || gameObject.name == "PoeBowl" || gameObject.name == "PoeToy");
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
            // V6.1 Q03_D003: the window is shut at the end of its examination, with Melantha's own line,
            // so the visual change (StoryFlagSpriteVisual) and the flag happen together.
            var closesWindow = unlocked && gameObject.name == "Window"
                && GameState.Instance != null && !GameState.Instance.HasFlag(StoryFlag.RoomWindowSecured);
            var altered = unlocked ? AlteredDialogue() : null;
            var selected = !unlocked ? lockedDialogue ?? dialogue
                : altered ?? (wasUsed && repeatDialogue != null ? repeatDialogue : dialogue);
            if (!DialogueManager.Instance.TryStartDialogue(selected))
            {
                if (selected != null && selected.SequenceId == "Q02") Destroy(selected);
                return;
            }
            if (unlocked && gameObject.name != "EmptyChamber") { MarkStoryState(); used = true; }
            void Complete()
            {
                DialogueManager.Instance.OnDialogueCompleted -= Complete;
                if (selected != null && selected.SequenceId == "Q02")
                {
                    GameState.Instance?.SetCounter("event.Q02", 1);
                    Destroy(selected);
                }
                if (selected != null && selected.SequenceId == "L02")
                {
                    used = true;
                    GameState.Instance?.SetFlag(StoryFlag.EmptyChamberComplete);
                    GameState.Instance?.SetCounter("event.L02", 1);
                }
                if (selected != null && selected.SequenceId.StartsWith("Q01."))
                {
                    GameState.Instance?.SetCounter("event." + selected.SequenceId, 1);
                    var state = GameState.Instance;
                    if (state != null && state.GetCounter("event.Q01.Photo") > 0
                        && state.GetCounter("event.Q01.Window") > 0
                        && state.GetCounter("event.Q01.PoeToy") > 0
                        && state.GetCounter("event.Q01.Desk") > 0)
                        state.SetCounter("event.Q01", 1);
                    if (selected == dialogue) { dialogue = null; Destroy(selected); }
                }
                if (selected != null && selected.SequenceId.StartsWith("GALLERY_"))
                {
                    GameState.Instance?.SetCounter("event." + selected.SequenceId, 1);
                    if (selected == dialogue) { dialogue = null; Destroy(selected); }
                }
                if (gameObject.name == "Photo" && !wasUsed)
                    V61ChoicePresenter.Show("CH_Q_PHOTO");
                if (closesWindow)
                {
                    GameState.Instance?.SetFlag(StoryFlag.RoomWindowSecured);
                    DialogueManager.Instance.TryStartDialogue(V61EventCatalog.CreateDialogueSegment("Q03", "Q03.WindowClosed", 2, 1));
                }
                onCompleted?.Invoke();
            }
            DialogueManager.Instance.OnDialogueCompleted += Complete;
        }

        private DialogueData AlteredDialogue()
        {
            var state = GameState.Instance;
            if (state == null || !state.HasFlag(StoryFlag.RoomGrimoireRead)) return null;
            if ((gameObject.name == "Photo" || gameObject.name == "Window")
                && state.GetCounter("event.Q02") == 0)
                return V61EventCatalog.CreateDialogue("Q02");
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
                case "Window": break; // closed when its dialogue ends (see closesWindow)
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

        private static DialogueData RoomIntroSegment(string objectName) => objectName switch
        {
            "Window" => V61EventCatalog.CreateDialogueSegment("Q01", "Q01.Window", 2, 2),
            "Photo" => V61EventCatalog.CreateDialogueSegment("Q01", "Q01.Photo", 4, 2),
            "PoeToy" => V61EventCatalog.CreateDialogueSegment("Q01", "Q01.PoeToy", 6, 4),
            "Desk" => V61EventCatalog.CreateDialogueSegment("Q01", "Q01.Desk", 10, 2),
            _ => null
        };

        private static string GalleryEventId(string objectName) => objectName switch
        {
            "GalleryDoor_Nova" => "GALLERY_01",
            "GalleryDoor_Crescente" => "GALLERY_02",
            "GalleryDoor_Cheia" => "GALLERY_03",
            "GalleryDoor_Minguante" => "GALLERY_04",
            "GalleryDoor_Gibosa" => "GALLERY_05",
            "GalleryDoor_Quarto" => "GALLERY_06",
            "GalleryDoor_Eco" => "GALLERY_07",
            "GalleryDoor_SemSímbolo" => "GALLERY_08",
            _ => null
        };
    }
}
