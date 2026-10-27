using UnityEngine;

namespace MementoMori.UI
{
    /// <summary>Presentation only: sort a moving sprite at its actor's ground contact point.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class VisualDepthOrder : MonoBehaviour
    {
        [SerializeField] private Transform groundAnchor;
        private SpriteRenderer visual;
        private BoxCollider2D[] roomProps;

        public void Configure(Transform anchor) => groundAnchor = anchor;
        private void Awake()
        {
            visual = GetComponent<SpriteRenderer>();
            if (gameObject.scene.name == "Quarto")
                roomProps = FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None);
        }
        private void LateUpdate()
        {
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            var y = groundAnchor != null ? groundAnchor.position.y : transform.position.y;
            visual.sortingOrder = Mathf.Clamp(5000 - Mathf.RoundToInt(y * 100f), -32000, 32000);
            if (roomProps == null) return;
            foreach (var prop in roomProps)
            {
                if (prop == null || !prop.enabled || prop.isTrigger || prop.gameObject.scene != gameObject.scene) continue;
                var sprite = prop.GetComponent<SpriteRenderer>();
                if (sprite == null || sprite == visual) continue;
                sprite.sortingOrder = Mathf.Clamp(5000 - Mathf.RoundToInt(prop.bounds.min.y * 100f), -32000, 32000);
            }
        }
    }
}
