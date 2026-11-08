using MementoMori.Interaction;
using UnityEngine;
using MementoMori.Audio;
using MementoMori.Dialogue;
using MementoMori.Core;

namespace MementoMori.Puzzles
{
    public sealed class MirrorSymbol : MonoBehaviour, IInteractable
    {
        [SerializeField] private string symbolId;
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private Color inactiveColor = Color.white;
        [SerializeField] private Color activeColor = Color.cyan;
        [SerializeField] private Sprite inactiveSprite;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private PuzzleMirror puzzle;
        [SerializeField] private DialogueData dialogue;
        public string SymbolId => symbolId;
        public string InteractionVerb => "Tocar";
        public int InteractionPriority => 1;
        public bool IsActive { get; private set; }
        public void Configure(string id, PuzzleMirror owner, SpriteRenderer renderer)
        {
            symbolId = id;
            puzzle = owner;
            visual = renderer;
        }
        public void ConfigureDialogue(DialogueData value) => dialogue = value;
        public void ConfigureVisualStates(Sprite normal, Sprite active)
        {
            inactiveSprite = normal;
            activeSprite = active;
            if (visual != null) visual.sprite = inactiveSprite;
        }
        public bool CanInteract(InteractionContext context) => puzzle != null
            && (puzzle.CanAcceptInput || puzzle.State == PuzzleMirror.PuzzleState.Solved && ExtraSegmentStart(symbolId) >= 0);
        public void Interact(InteractionContext context)
        {
            if (!CanInteract(context)) return;
            RuntimeAudio.PlayOneShot("11_mirror_shimmer", .45f);
            if (ExtraSegmentStart(symbolId) >= 0
                && GameState.Instance != null && GameState.Instance.GetCounter("event.M03_EXTRA." + symbolId) == 0)
            {
                var extra = V61EventCatalog.CreateDialogueSegment("M03_EXTRA", "M03_EXTRA." + symbolId,
                    ExtraSegmentStart(symbolId), 2);
                var manager = DialogueManager.Instance;
                if (extra != null && manager != null && manager.TryStartDialogue(extra))
                {
                    void Complete()
                    {
                        manager.OnDialogueCompleted -= Complete;
                        var state = GameState.Instance;
                        state?.SetCounter("event.M03_EXTRA." + symbolId, 1);
                        if (state != null && state.GetCounter("event.M03_EXTRA.Present") > 0
                            && state.GetCounter("event.M03_EXTRA.Absent") > 0
                            && state.GetCounter("event.M03_EXTRA.Double") > 0
                            && state.GetCounter("event.M03_EXTRA.Black") > 0)
                            state.SetCounter("event.M03_EXTRA", 1);
                        Destroy(extra);
                    }
                    manager.OnDialogueCompleted += Complete;
                    return;
                }
                if (extra != null) Destroy(extra);
                return;
            }
            if (puzzle.State != PuzzleMirror.PuzzleState.Solved) puzzle.Activate(this);
            var narrativeDialogue = dialogue ?? Resources.Load<DialogueData>("Dialogue/" + NarrativeId(symbolId));
            if (narrativeDialogue != null) DialogueManager.Instance?.StartDialogue(narrativeDialogue);
        }

        private static int ExtraSegmentStart(string id) => id switch
        {
            "Present" => 0, "Absent" => 2, "Double" => 4, "Black" => 6, _ => -1
        };

        private static string NarrativeId(string id) => id switch
        {
            "Present" => "DLG_D_MIRROR_PRESENT",
            "Delayed" => "DLG_D_MIRROR_DELAYED",
            "Ahead" => "DLG_D_MIRROR_AHEAD",
            "Absent" => "DLG_D_MIRROR_NO_POE",
            "Double" => "DLG_D_MIRROR_TWO_POES",
            "Room" => "DLG_D_MIRROR_ROOM",
            "Black" => "DLG_D_BLACK_MIRROR_MEMORY",
            _ => string.Empty
        };
        public void SetActive(bool active)
        {
            IsActive = active;
            if (visual != null)
            {
                if (activeSprite != null && inactiveSprite != null) visual.sprite = active ? activeSprite : inactiveSprite;
                visual.color = active ? activeColor : inactiveColor;
            }
        }
    }
}
