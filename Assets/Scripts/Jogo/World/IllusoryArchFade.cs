using UnityEngine;

namespace MementoMori.World
{
    /// <summary>
    /// Corredor Ilusório: a painted arch Melantha can walk into turns translucent while she is inside it,
    /// so the player reads it as false instead of seeing her vanish into a wall.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class IllusoryArchFade : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] float insideAlpha = .35f;
        [SerializeField] float fadeSpeed = 4f;

        SpriteRenderer sprite;
        Transform player;
        float alpha = 1f;

        void Awake() => sprite = GetComponent<SpriteRenderer>();

        void Update()
        {
            if (player == null)
            {
                var tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged == null) return;
                player = tagged.transform;
            }
            var b = sprite.bounds;
            var p = player.position;
            var inside = p.x > b.min.x && p.x < b.max.x && p.y > b.min.y - .2f && p.y < b.max.y;
            alpha = Mathf.MoveTowards(alpha, inside ? insideAlpha : 1f, fadeSpeed * Time.deltaTime);
            var c = sprite.color;
            if (!Mathf.Approximately(c.a, alpha)) { c.a = alpha; sprite.color = c; }
        }
    }
}
