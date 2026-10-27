using UnityEngine;

namespace MementoMori.Puzzles
{
    public sealed class SigilVisualState : MonoBehaviour
    {
        private SigilRingPuzzle puzzle;
        private SpriteRenderer visual;
        private Sprite inactiveSprite;
        private Sprite activeSprite;
        private Sprite completeSprite;
        private int lastState = -1;

        public void Configure(SigilRingPuzzle owner, SpriteRenderer target, Sprite inactive, Sprite active, Sprite complete)
        {
            puzzle = owner;
            visual = target;
            inactiveSprite = inactive;
            activeSprite = active;
            completeSprite = complete;
            Refresh(true);
        }

        private void Update() => Refresh(false);

        private void Refresh(bool force)
        {
            if (puzzle == null || visual == null) return;
            var progress = puzzle.GetProgress();
            var state = puzzle.Solved ? 2 : progress > 0 ? 1 : 0;
            if (!force && state == lastState) return;
            lastState = state;
            visual.sprite = state == 2 ? completeSprite : state == 1 ? activeSprite : inactiveSprite;
            visual.color = state == 2 ? Color.white : state == 1 ? new Color(1f, .9f, .7f, 1f) : new Color(.7f, .7f, .9f, 1f);
        }
    }
}
