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
        [SerializeField] private Sprite[] transitionFrames;
        [SerializeField, Min(0.01f)] private float transitionDuration = .24f;
        [SerializeField] private SpriteRenderer[] beforeStateLights;
        private MaterialPropertyBlock lightProperties;
        private bool initialized;
        private float closedAmount;

        public void Configure(SpriteRenderer renderer, StoryFlag observedFlag, Sprite initial, Sprite changed)
        {
            target = renderer; flag = observedFlag; before = initial; after = changed;
            Refresh();
        }

        private void OnEnable() { initialized = false; Refresh(); }
        private void LateUpdate() => Refresh();
        private void Refresh()
        {
            if (!target) return;
            var state = GameState.Instance;
            var closed = state != null && state.HasFlag(flag);
            var destination = closed ? 1f : 0f;
            // Restore immediately on scene entry; animate only subsequent changes.
            closedAmount = !initialized || !Application.isPlaying ? destination
                : Mathf.MoveTowards(closedAmount, destination, Time.deltaTime / Mathf.Max(.01f, transitionDuration));
            initialized = true;
            var next = closed ? after : before;
            if (closedAmount > 0f && closedAmount < 1f && transitionFrames != null && transitionFrames.Length > 0)
                next = transitionFrames[Mathf.Clamp(Mathf.FloorToInt(closedAmount * transitionFrames.Length), 0, transitionFrames.Length - 1)];
            if (next && target.sprite != next) target.sprite = next;
            if (beforeStateLights == null) return;
            lightProperties ??= new MaterialPropertyBlock();
            foreach (var light in beforeStateLights)
            {
                if (!light || !light.sharedMaterial || !light.sharedMaterial.HasProperty("_Intensity")) continue;
                light.GetPropertyBlock(lightProperties);
                lightProperties.SetFloat("_Intensity", light.sharedMaterial.GetFloat("_Intensity") * (1f - closedAmount));
                light.SetPropertyBlock(lightProperties);
                light.enabled = closedAmount < 1f;
            }
        }
    }
}
