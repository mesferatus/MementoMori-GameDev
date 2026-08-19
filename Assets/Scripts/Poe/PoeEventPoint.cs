using UnityEngine;

namespace MementoMori.Poe
{
    public sealed class PoeEventPoint : MonoBehaviour
    {
        [SerializeField] private Vector2 lookDirection = Vector2.down;
        [SerializeField, Min(0f)] private float waitDuration = 1.5f;
        [SerializeField] private bool resumeFollow = true;
        public Vector2 LookDirection => lookDirection;
        public float WaitDuration => waitDuration;
        public bool ResumeFollow => resumeFollow;

        public void Configure(Vector2 direction, float duration, bool resume)
        {
            lookDirection = direction;
            waitDuration = Mathf.Max(0f, duration);
            resumeFollow = resume;
        }
    }
}
