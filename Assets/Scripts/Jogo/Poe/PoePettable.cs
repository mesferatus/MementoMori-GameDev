using MementoMori.Core;
using MementoMori.Interaction;
using MementoMori.Player;
using UnityEngine;

namespace MementoMori.Poe
{
    /// <summary>Lets the player pet Poe on purpose (E / A) instead of only by waiting idle beside him.</summary>
    public sealed class PoePettable : MonoBehaviour, IInteractable
    {
        PoeFollower poe;
        float nextAt;

        public string InteractionVerb => "Fazer carinho";
        public int InteractionPriority => 1;

        /// <summary>Gives Poe a small reach zone once he exists in the scene.</summary>
        public static void Ensure(PoeFollower follower)
        {
            if (follower == null || follower.GetComponentInChildren<PoePettable>(true) != null) return;
            var zone = new GameObject("PetZone", typeof(CircleCollider2D), typeof(PoePettable));
            zone.transform.SetParent(follower.transform, false);
            var circle = zone.GetComponent<CircleCollider2D>();
            circle.isTrigger = true; circle.radius = 1.4f;
            zone.GetComponent<PoePettable>().poe = follower;
        }

        public bool CanInteract(InteractionContext context)
        {
            if (poe == null) poe = GetComponentInParent<PoeFollower>();
            if (poe == null || Time.time < nextAt) return false;
            if (GameState.Instance == null || !GameState.Instance.PoeRevealed) return false;
            if (poe.State != PoeState.Following && poe.State != PoeState.Waiting) return false;
            var life = context.Interactor == null ? null : context.Interactor.GetComponentInParent<MelanthaIdleLife>();
            // Offered only after she has lingered beside him a while, so the prompt is not in the way all the time.
            if (life == null || life.IsPetting || life.StillSeconds < 15f) return false;
            return ((Vector2)(poe.transform.position - life.transform.position)).sqrMagnitude <= 2.2f * 2.2f;
        }

        public void Interact(InteractionContext context)
        {
            var player = context.Interactor == null ? null : context.Interactor.GetComponentInParent<PlayerController>();
            var life = player == null ? null : player.GetComponent<MelanthaIdleLife>();
            if (player == null || life == null) return;
            nextAt = Time.time + 2.6f;
            // The petting frames expect Poe right beside her at hip height: he steps to that spot on the
            // side he is already on (or the other side when a wall is there).
            var side = poe.transform.position.x < player.transform.position.x ? -1f : 1f;
            foreach (var s in new[] { side, -side })
            {
                var spot = (Vector2)player.transform.position + new Vector2(.58f * s, 0f);
                if (Blocked(spot)) continue;
                var body = poe.GetComponent<Rigidbody2D>();
                if (body != null) { body.position = spot; body.linearVelocity = Vector2.zero; }
                poe.transform.position = new Vector3(spot.x, spot.y, poe.transform.position.z);
                break;
            }
            life.PetNow(poe);
            MementoMori.Audio.RuntimeAudio.PlayOneShot("15_poe_soft_call", .3f);
            GameState.Instance?.IncrementCounter("poe.pets");
        }

        static bool Blocked(Vector2 point)
        {
            foreach (var hit in Physics2D.OverlapCircleAll(point, .18f))
                if (hit != null && !hit.isTrigger && hit.GetComponentInParent<PlayerController>() == null
                    && hit.GetComponentInParent<PoeFollower>() == null) return true;
            return false;
        }
    }
}
