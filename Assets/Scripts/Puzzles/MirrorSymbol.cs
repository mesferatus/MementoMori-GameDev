using MementoMori.Interaction;
using UnityEngine;
using MementoMori.Audio;
using MementoMori.Dialogue;

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
        public bool CanInteract(InteractionContext context) => puzzle != null && puzzle.CanAcceptInput;
        public void Interact(InteractionContext context)
        {
            if (!CanInteract(context)) return;
            RuntimeAudio.PlayOneShot("11_mirror_shimmer", .45f);
            puzzle.Activate(this);
            if (dialogue != null) DialogueManager.Instance?.StartDialogue(dialogue);
        }
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
