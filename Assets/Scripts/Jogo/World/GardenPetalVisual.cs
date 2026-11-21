using MementoMori.Puzzles;
using UnityEngine;

namespace MementoMori.World
{
    public sealed class GardenPetalVisual : MonoBehaviour
    {
        [SerializeField] private GardenPetalPuzzle puzzle;
        [SerializeField] private Transform player;
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private Sprite dormant;
        [SerializeField] private Sprite active;
        [SerializeField] private int waningIndex = -1;

        public void Configure(GardenPetalPuzzle owner, Transform actor, SpriteRenderer renderer, Sprite off, Sprite on, int index = -1)
        {
            puzzle = owner; player = actor; target = renderer; dormant = off; active = on; waningIndex = index;
        }

        private void LateUpdate()
        {
            if (!puzzle || !target) return;
            var lit = puzzle.Solved || puzzle.CanCollect(player);
            if (waningIndex >= 0) lit = !puzzle.Solved && waningIndex <= puzzle.NextWaningFlower;
            if (!active && !dormant)
            {
                // Approved pool art keeps its sprite; state reads as light instead of a sprite swap.
                if (waningIndex >= 0 && !lit)
                {
                    // An extinguished waning light must read clearly as "out".
                    target.color = Color.Lerp(target.color, new Color(.22f, .2f, .3f, .45f), Time.deltaTime * 6f);
                    return;
                }
                var glow = lit ? .85f + .15f * Mathf.Sin(Time.time * 3f) : .55f;
                target.color = Color.Lerp(target.color, new Color(glow, glow, Mathf.Min(1f, glow + .12f), 1f), Time.deltaTime * 8f);
                return;
            }
            var next = lit ? active : dormant;
            if (next && target.sprite != next) target.sprite = next;
        }
    }
}
