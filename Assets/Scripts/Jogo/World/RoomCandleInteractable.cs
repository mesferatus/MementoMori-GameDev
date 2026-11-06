using MementoMori.Interaction;
using UnityEngine;

namespace MementoMori.World
{
    public sealed class RoomCandleInteractable : MonoBehaviour, IInteractable, IInteractionReach, IInteractionSurface
    {
        // The candle's drawn body and its solid Footprint live under the visual foundation, not under
        // this trigger, so seen from the south they used to hide the candle from its own prompt.
        public bool IsOwnSurface(Collider2D collider)
        {
            if (collider == null || collider.isTrigger) return false;
            var own = GetComponent<Collider2D>();
            return own != null && own.bounds.Intersects(collider.bounds);
        }

        // The trigger sits 0.55 above a 0.7 x 0.7 solid base and Melantha's body radius is 0.5, so the
        // farthest blocked approach (from the south) stops ~1.61 from the trigger. 1.7 keeps all four
        // sides reachable while the 0.72 trigger plus the 1.4 detector used to reach ~2.12.
        public float MaxReach => 1.7f;
        [SerializeField, Range(0, 3)] private int orderIndex;
        [SerializeField] private RoomCandlePuzzle puzzle;

        public void Configure(RoomCandlePuzzle owner, int index)
        {
            puzzle = owner;
            orderIndex = index;
        }

        public string InteractionVerb => $"Acender vela: {RoomCandlePuzzle.NameFor(orderIndex)}";
        public int OrderIndex => orderIndex;
        public int InteractionPriority => 25;
        public bool CanInteract(InteractionContext context) => puzzle != null && !puzzle.Completed;
        public void Interact(InteractionContext context) => puzzle?.Activate(orderIndex);
    }
}
