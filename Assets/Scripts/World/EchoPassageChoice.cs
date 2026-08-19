using MementoMori.Interaction;
using UnityEngine;

namespace MementoMori.World
{
    public sealed class EchoPassageChoice : MonoBehaviour, IInteractable
    {
        [SerializeField] private EchoCorridorPuzzle puzzle;
        [SerializeField] private int passage;

        public string InteractionVerb => "Seguir voz";
        public int InteractionPriority => 6;

        public void Configure(EchoCorridorPuzzle owner, int index)
        {
            puzzle = owner;
            passage = index;
        }

        public bool CanInteract(InteractionContext context) => puzzle != null && !puzzle.Solved;

        public void Interact(InteractionContext context) =>
            puzzle?.Select(passage, context.Interactor == null ? null : context.Interactor.transform);
    }
}
