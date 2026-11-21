using UnityEngine;

namespace MementoMori.Interaction
{
    /// <summary>Names the sprite the interaction rim should outline when it is not the nearest prop.</summary>
    public sealed class InteractionVisualTarget : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer visual;
        public SpriteRenderer Visual => visual;
        public void Configure(SpriteRenderer target) => visual = target;
    }
}
