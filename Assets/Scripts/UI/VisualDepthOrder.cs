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
