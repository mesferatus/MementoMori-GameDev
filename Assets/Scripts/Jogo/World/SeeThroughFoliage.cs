using UnityEngine;

namespace MementoMori.World
{
    /// <summary>
    /// Foliage the player can walk through (or behind) fades while it covers her, so she never disappears
    /// inside a bush or behind a tree canopy. Flame mode does the reverse case: when Melantha's body is drawn
    /// over a flame, the flame is lifted in front of her, half transparent, instead of vanishing.
    /// Presentation only: colliders and gameplay state are untouched.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SeeThroughFoliage : MonoBehaviour
    {
        [SerializeField, Range(.15f, 1f)] private float coveredAlpha = .38f;
        [SerializeField] private bool flame;
        private const float BodyHeight = 1.35f;
        private static Transform player;
        private static SpriteRenderer playerSprite;
        private static float nextLookup;
        private SpriteRenderer sprite;
        private float baseAlpha = 1f, alpha = 1f;
        private int baseOrder, baseLayer;
        private bool lifted;

        public void ConfigureAsFlame(float alphaWhileOverPlayer = .6f)
        {
            flame = true;
            coveredAlpha = alphaWhileOverPlayer;
        }

        private void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            baseAlpha = sprite.color.a;
            baseOrder = sprite.sortingOrder;
            baseLayer = sprite.sortingLayerID;
        }

        private void LateUpdate()
        {
            if (flame) return; // FlameDepthLink keeps the flame above its holder, without jumping in front of actors.
            if (player == null)
            {
                if (Time.unscaledTime < nextLookup) return;
                nextLookup = Time.unscaledTime + 1f;
                var controller = FindAnyObjectByType<Player.PlayerController>();
                if (controller == null) return;
                player = controller.transform;
                playerSprite = controller.GetComponentInChildren<SpriteRenderer>();
            }
            var bounds = sprite.bounds;
            var feet = player.position;
            bool covered;
            if (flame)
            {
                // Her body (feet up to ~1.35 above) overlaps the flame while she stands just in front of it.
                covered = feet.x > bounds.min.x - .3f && feet.x < bounds.max.x + .3f
                    && feet.y < bounds.max.y && feet.y > bounds.min.y - BodyHeight;
                SetLifted(covered);
            }
            else
            {
                // Covered when her feet are inside the foliage body (the bush or canopy is drawn over her there).
                covered = feet.x > bounds.min.x + .08f && feet.x < bounds.max.x - .08f
                    && feet.y > bounds.min.y - .05f && feet.y < bounds.max.y - .1f;
            }
            float target = covered ? coveredAlpha : 1f;
            if (Mathf.Approximately(alpha, target)) return;
            alpha = Mathf.MoveTowards(alpha, target, Time.deltaTime * 3.5f);
            var color = sprite.color;
            color.a = baseAlpha * alpha;
            sprite.color = color;
        }

        private void SetLifted(bool value)
        {
            if (value == lifted) return;
            lifted = value;
            if (!value || playerSprite == null) { sprite.sortingLayerID = baseLayer; sprite.sortingOrder = baseOrder; return; }
            sprite.sortingLayerID = playerSprite.sortingLayerID;
            sprite.sortingOrder = playerSprite.sortingOrder + 1;
        }

        private void OnDisable()
        {
            if (sprite == null) return;
            sprite.sortingLayerID = baseLayer;
            sprite.sortingOrder = baseOrder;
            var color = sprite.color; color.a = baseAlpha; sprite.color = color;
            alpha = 1f; lifted = false;
        }
    }
}
