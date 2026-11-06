using UnityEngine;

namespace MementoMori.World
{
    /// <summary>
    /// Foliage the player can walk through fades while it covers her, so she never disappears inside a bush.
    /// Presentation only: no collider, no gameplay state.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SeeThroughFoliage : MonoBehaviour
    {
        [SerializeField, Range(.15f, 1f)] private float coveredAlpha = .38f;
        private static Transform player;
        private static float nextLookup;
        private SpriteRenderer sprite;
        private float baseAlpha = 1f, alpha = 1f;

        private void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            baseAlpha = sprite.color.a;
        }

        private void LateUpdate()
        {
            if (player == null)
            {
                if (Time.unscaledTime < nextLookup) return;
                nextLookup = Time.unscaledTime + 1f;
                var controller = FindAnyObjectByType<Player.PlayerController>();
                if (controller == null) return;
                player = controller.transform;
            }
            var bounds = sprite.bounds;
            var feet = player.position;
            // Covered when her feet are inside the foliage body (the bush is drawn over her there).
            bool covered = feet.x > bounds.min.x + .08f && feet.x < bounds.max.x - .08f
                && feet.y > bounds.min.y - .05f && feet.y < bounds.max.y - .1f;
            float target = covered ? coveredAlpha : 1f;
            if (Mathf.Approximately(alpha, target)) return;
            alpha = Mathf.MoveTowards(alpha, target, Time.deltaTime * 3.5f);
            var color = sprite.color;
            color.a = baseAlpha * alpha;
            sprite.color = color;
        }
    }
}
