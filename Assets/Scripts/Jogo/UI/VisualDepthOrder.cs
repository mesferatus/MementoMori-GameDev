using UnityEngine;

namespace MementoMori.UI
{
    /// <summary>Sort each actor by its own ground contact. Scenery owns separate ground anchors.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class VisualDepthOrder : MonoBehaviour
    {
        [SerializeField] private Transform groundAnchor;
        private SpriteRenderer visual;
        public void Configure(Transform anchor) => groundAnchor = anchor;
        private CircleCollider2D feet; private Transform feetOwner;
        private void Awake() => visual = GetComponent<SpriteRenderer>();
        // An actor's ground contact is its solid feet circle, not its transform (Melantha's sits .22 lower).
        // Sorting by the transform drew her behind a facade while she stood in front of its base.
        private CircleCollider2D Feet(Transform anchor)
        {
            if (anchor == feetOwner) return feet;
            feetOwner = anchor; feet = null;
            if (anchor != null) foreach (var c in anchor.GetComponents<CircleCollider2D>()) if (!c.isTrigger) { feet = c; break; }
            return feet;
        }
        private void LateUpdate()
        {
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            var anchor = groundAnchor != null ? groundAnchor : transform;
            var contact = Feet(anchor);
            var y = contact != null ? anchor.TransformPoint(contact.offset).y : anchor.position.y;
            visual.sortingOrder = Mathf.Clamp(5000 - Mathf.RoundToInt(y * 100f), -32000, 32000);
        }
    }
}
