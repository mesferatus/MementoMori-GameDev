using MementoMori.Player;
using UnityEngine;

namespace MementoMori.Interaction
{
    /// <summary>Front edge of the Moon Garden pool: Melantha enters or leaves the water here.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class GardenSwimPoint : MonoBehaviour, IInteractable, IInteractionSurface
    {
        [SerializeField] private Vector2 shorePoint = new(110.4f, -78.75f);
        [SerializeField] private Vector2 waterPoint = new(110.4f, -77.8f);

        public string InteractionVerb => Swimmer() != null && Swimmer().IsSwimming ? "Sair da água" : "Nadar";
        public int InteractionPriority => 6;

        public void Configure(Vector2 shore, Vector2 water) { shorePoint = shore; waterPoint = water; }

        public bool CanInteract(InteractionContext context) => context.Interactor != null
            && context.Interactor.GetComponentInParent<PlayerController>() != null;

        public void Interact(InteractionContext context)
        {
            var player = context.Interactor == null ? null : context.Interactor.GetComponentInParent<PlayerController>();
            if (player == null) return;
            var target = player.IsSwimming ? shorePoint : waterPoint;
            var body = player.GetComponent<Rigidbody2D>();
            if (body != null) body.position = target;
            player.transform.position = new Vector3(target.x, target.y, player.transform.position.z);
            Physics2D.SyncTransforms();
        }

        // The pool rim between the shore and the water is the point's own edge, never an obstacle.
        public bool IsOwnSurface(Collider2D collider) => true;

        private static PlayerController cached;
        private static PlayerController Swimmer()
        {
            if (cached == null) cached = FindAnyObjectByType<PlayerController>();
            return cached;
        }
    }
}
