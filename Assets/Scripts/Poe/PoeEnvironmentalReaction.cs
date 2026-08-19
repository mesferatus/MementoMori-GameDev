using UnityEngine;

namespace MementoMori.Poe
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class PoeEnvironmentalReaction : MonoBehaviour
    {
        [SerializeField] private PoeFollower poe;
        [SerializeField] private string eventId;
        [SerializeField] private bool oneShot = true;
        private bool used;

        public void Configure(PoeFollower follower, string id, bool isOneShot)
        {
            poe = follower; eventId = id; oneShot = isOneShot;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (used && oneShot || !other.CompareTag("Player")) return;
            used = true;
            poe?.ReactToEnvironment(eventId);
        }
    }
}
