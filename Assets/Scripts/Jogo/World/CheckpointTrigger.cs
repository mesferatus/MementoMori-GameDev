using MementoMori.Core;
using UnityEngine;

namespace MementoMori.World
{
    /// <summary>Connects a map location to the existing checkpoint/save services.</summary>
    public sealed class CheckpointTrigger : MonoBehaviour
    {
        [SerializeField] private CheckpointId checkpointId;
        [SerializeField] private Vector2 safePosition;
        private bool activated;

        public void Configure(CheckpointId id, Vector2 position)
        {
            checkpointId = id;
            safePosition = position;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activated || !other.CompareTag("Player")) return;
            activated = true;
            GameState.Instance?.SaveCheckpoint(gameObject.scene.name, safePosition);
            StoryProgression.Instance?.SaveCheckpoint(checkpointId);
        }
    }
}
