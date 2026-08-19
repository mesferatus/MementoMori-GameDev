using UnityEngine;

namespace MementoMori.Poe
{
    public sealed class PoeEventTrigger : MonoBehaviour
    {
        [SerializeField] private PoeFollower poe;
        [SerializeField] private PoeEventPoint point;
        [SerializeField] private bool oneShot = true;
        private bool used;

        public void Configure(PoeFollower follower, PoeEventPoint eventPoint, bool isOneShot)
        {
            poe = follower;
            point = eventPoint;
            oneShot = isOneShot;
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (used && oneShot || !other.CompareTag("Player")) return;
            used = true;
            poe?.MoveTo(point);
        }
    }
}
