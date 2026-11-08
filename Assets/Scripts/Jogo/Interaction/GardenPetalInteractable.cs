using MementoMori.Interaction;
using MementoMori.Puzzles;
using UnityEngine;

namespace MementoMori.World
{
    public sealed class GardenPetalInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] GardenPetalPuzzle puzzle;
        [SerializeField] string target;
        public string InteractionVerb => "Colocar pétala";
        public int InteractionPriority => 7;
        // The flowers float in the pool: Melantha reaches them only while swimming.
        public bool CanInteract(InteractionContext context) => puzzle != null && GardenPetalPuzzle.IsSwimming(context.Interactor)
            && puzzle.CanCollect(context.Interactor == null ? null : context.Interactor.transform);
        public void Configure(GardenPetalPuzzle targetPuzzle, string expectedTarget) { puzzle = targetPuzzle; target = expectedTarget; }
        public void Interact(InteractionContext context)
        {
            if (CanInteract(context)) puzzle.Place(target, context.Interactor.transform);
        }
    }
}
