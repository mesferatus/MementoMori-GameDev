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
            var next = lit ? active : dormant;
            if (next && target.sprite != next) target.sprite = next;
        }
    }
}
