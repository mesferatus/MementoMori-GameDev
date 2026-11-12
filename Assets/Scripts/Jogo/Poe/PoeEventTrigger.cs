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
            // The scene briefly creates Melantha at its authored spawn before placing
            // her at the saved position. That temporary overlap is not a route visit.
            if (MementoMori.Core.SceneLoader.Instance != null && MementoMori.Core.SceneLoader.Instance.IsLoading) return;
            if (used && oneShot || !other.CompareTag("Player") || poe == null || !poe.isActiveAndEnabled || point == null) return;
            used = true;
            poe?.MoveTo(point);
        }
    }
}

