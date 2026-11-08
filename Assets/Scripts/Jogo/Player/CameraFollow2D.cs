using UnityEngine;

namespace MementoMori.Player
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0.01f)] private float damping = 8f;
        [SerializeField] private bool useBounds;
        [SerializeField] private Vector2 minBounds;
        [SerializeField] private Vector2 maxBounds;

        public void Configure(Transform followTarget, Vector2 minimum, Vector2 maximum)
        {
            target = followTarget;
            useBounds = true;
            minBounds = minimum;
            maxBounds = maximum;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            var targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            }

            transform.position = Vector3.Lerp(transform.position, targetPosition, 1f - Mathf.Exp(-damping * Time.deltaTime));
        }
    }
}
