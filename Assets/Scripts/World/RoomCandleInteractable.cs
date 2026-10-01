using MementoMori.Interaction;
using UnityEngine;

namespace MementoMori.World
{
    public sealed class RoomCandleInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField, Range(0, 3)] private int orderIndex;
        [SerializeField] private RoomCandlePuzzle puzzle;

        public void Configure(RoomCandlePuzzle owner, int index)
        {
            puzzle = owner;
            orderIndex = index;
        }

        public string InteractionVerb => "Acender vela";
        public int OrderIndex => orderIndex;
        public int InteractionPriority => 25;
        public bool CanInteract(InteractionContext context) => puzzle != null && !puzzle.Completed;
        public void Interact(InteractionContext context) => puzzle?.Activate(orderIndex);
    }
}
