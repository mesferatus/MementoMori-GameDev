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
        private void Awake() => visual = GetComponent<SpriteRenderer>();
        private void LateUpdate()
        {
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            var y = groundAnchor != null ? groundAnchor.position.y : transform.position.y;
            visual.sortingOrder = Mathf.Clamp(5000 - Mathf.RoundToInt(y * 100f), -32000, 32000);
        }
    }
}
