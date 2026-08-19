using MementoMori.Puzzles;
using UnityEngine;

namespace MementoMori.Interaction
{
    /// <summary>One light in the waning-petal puzzle. The required order is reverse: 3, 2, 1.</summary>
    public sealed class WaningFlowerNode : MonoBehaviour, IInteractable
    {
        [SerializeField] GardenPetalPuzzle puzzle;
        [SerializeField] int flowerIndex;
        public string InteractionVerb => "Apagar flor";
        public int InteractionPriority => 7;
        public void Configure(GardenPetalPuzzle owner, int index) { puzzle = owner; flowerIndex = index; }
        public bool CanInteract(InteractionContext context) => puzzle != null && !puzzle.Solved;
        public void Interact(InteractionContext context) => puzzle?.ExtinguishWaning(flowerIndex);
    }
}
