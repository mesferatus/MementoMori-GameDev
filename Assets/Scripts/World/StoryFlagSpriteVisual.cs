using MementoMori.Core;
using UnityEngine;

namespace MementoMori.World
{
    // Presentation only: observes an existing flag without changing progression.
    public sealed class StoryFlagSpriteVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private StoryFlag flag;
        [SerializeField] private Sprite before;
        [SerializeField] private Sprite after;

        public void Configure(SpriteRenderer renderer, StoryFlag observedFlag, Sprite initial, Sprite changed)
        {
            target = renderer; flag = observedFlag; before = initial; after = changed;
            Refresh();
        }

        private void OnEnable() => Refresh();
        private void LateUpdate() => Refresh();
        private void Refresh()
        {
            if (!target) return;
            var state = GameState.Instance;
            var next = state != null && state.HasFlag(flag) ? after : before;
            if (next && target.sprite != next) target.sprite = next;
        }
    }
}
