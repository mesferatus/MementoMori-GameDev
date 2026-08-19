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
        public bool CanInteract(InteractionContext context) => puzzle != null && puzzle.CanCollect(context.Interactor == null ? null : context.Interactor.transform);
        public void Configure(GardenPetalPuzzle targetPuzzle, string expectedTarget) { puzzle = targetPuzzle; target = expectedTarget; }
        public void Interact(InteractionContext context)
        {
            if (CanInteract(context)) puzzle.Place(target, context.Interactor.transform);
        }
    }
}
