using MementoMori.Interaction;
using UnityEngine;

namespace MementoMori.Puzzles
{
    public sealed class SigilPart : MonoBehaviour, IInteractable
    {
        [SerializeField] private string partId;
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private Color activeColor = Color.cyan;
        [SerializeField] private PuzzleSigilSequence puzzle;
        private Color initialColor;
        public string PartId => partId;
        public bool IsActive { get; private set; }
        public string InteractionVerb => "Tocar";
        public int InteractionPriority => 1;
        private void Awake() { if (visual != null) initialColor = visual.color; }
        public void Configure(string id, PuzzleSigilSequence owner, SpriteRenderer renderer)
        {
            partId = id;
            puzzle = owner;
            visual = renderer;
            initialColor = renderer != null ? renderer.color : Color.white;
        }
        public bool CanInteract(InteractionContext context) => puzzle != null && puzzle.CanAcceptInput;
        public void Interact(InteractionContext context) => puzzle?.Activate(this);
        public void SetActive(bool active)
        {
            IsActive = active;
            if (visual != null) visual.color = active ? activeColor : initialColor;
        }
    }
}
