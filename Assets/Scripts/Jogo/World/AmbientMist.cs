using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.World
{
    /// <summary>
    /// Visible drifting mist for the Labyrinth and the Moon Domain: a few large, faint cloud banks that slide
    /// slowly across the view and wrap around the camera, so the air always reads as foggy wherever Melantha is.
    /// Purely presentational: no colliders, no map changes.
    /// </summary>
    public sealed class AmbientMist : MonoBehaviour
    {
        private const int Banks = 9;
        private sealed class Bank { public SpriteRenderer renderer; public Vector2 velocity; public float phase, baseAlpha; }
        private readonly Bank[] banks = new Bank[Banks];
        private Camera view;
        private Color tint;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded -= OnLoaded;
            SceneManager.sceneLoaded += OnLoaded;
            Attach(SceneManager.GetActiveScene());
        }

        private static void OnLoaded(Scene scene, LoadSceneMode mode) => Attach(scene);

        private static void Attach(Scene scene)
        {
            if (scene.name != "Labirinto" && scene.name != "DominioLua") return;
            if (FindAnyObjectByType<AmbientMist>() != null) return;
            var go = new GameObject("AmbientMist");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.AddComponent<AmbientMist>().tint = scene.name == "DominioLua"
                ? new Color(.72f, .78f, 1f, 1f)   // cold moonlit haze
                : new Color(.70f, .60f, .86f, 1f); // violet labyrinth mist
        }

        private void Start()
        {
            var sprite = ModuleFog.CloudSprite();
            var random = new System.Random(31);
            for (var i = 0; i < Banks; i++)
            {
                var go = new GameObject("MistBank_" + i);
                go.transform.SetParent(transform, false);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                // Above the y-sorted world (max ~10000), below the module fog (31000) and all UI.
                renderer.sortingOrder = 30000;
                var scale = .35f + (float)random.NextDouble() * .35f; // cloud sprite is 16 units wide
                go.transform.localScale = new Vector3(scale * 1.7f, scale, 1f);
                var speed = .12f + (float)random.NextDouble() * .16f;
                var angle = (float)(random.NextDouble() * .5 - .25);
                banks[i] = new Bank
                {
                    renderer = renderer,
                    velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed * (i % 2 == 0 ? 1f : -1f),
                    phase = (float)random.NextDouble() * 10f,
                    baseAlpha = .11f + (float)random.NextDouble() * .08f
                };
                renderer.color = new Color(tint.r, tint.g, tint.b, 0f);
            }
            PlaceAroundCamera(random);
        }

        private void PlaceAroundCamera(System.Random random)
        {
            view = Camera.main;
            var center = view != null ? (Vector2)view.transform.position : Vector2.zero;
            var half = HalfExtents();
            foreach (var bank in banks)
                bank.renderer.transform.position = new Vector3(
                    center.x + ((float)random.NextDouble() * 2f - 1f) * half.x,
                    center.y + ((float)random.NextDouble() * 2f - 1f) * half.y, 0f);
        }

        private Vector2 HalfExtents()
        {
            if (view == null || !view.orthographic) return new Vector2(14f, 8f);
            var h = view.orthographicSize;
            // Margin so banks fade in and out beyond the screen edge instead of popping.
            return new Vector2(h * view.aspect + 4f, h + 3f);
        }

        private void LateUpdate()
        {
            if (view == null) view = Camera.main;
            if (view == null) return;
            var center = (Vector2)view.transform.position;
            var half = HalfExtents();
            var calm = MementoMori.Core.AccessibilitySettings.Instance != null && MementoMori.Core.AccessibilitySettings.Instance.ReduceFlashes;
            foreach (var bank in banks)
            {
                if (bank == null) continue;
                var t = bank.renderer.transform;
                var p = (Vector2)t.position + bank.velocity * Time.deltaTime;
                // Wrap around the camera rectangle so the mist never runs out while exploring.
                if (p.x < center.x - half.x) p.x += half.x * 2f; else if (p.x > center.x + half.x) p.x -= half.x * 2f;
                if (p.y < center.y - half.y) p.y += half.y * 2f; else if (p.y > center.y + half.y) p.y -= half.y * 2f;
                t.position = new Vector3(p.x, p.y, 0f);
                // Fade near the wrap edges and breathe slowly.
                var edge = Mathf.Min(1f - Mathf.Abs(p.x - center.x) / half.x, 1f - Mathf.Abs(p.y - center.y) / half.y);
                var breathe = calm ? 1f : .75f + .25f * Mathf.Sin(Time.time * .21f + bank.phase);
                var c = bank.renderer.color;
                c.a = bank.baseAlpha * breathe * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(edge / .25f));
                bank.renderer.color = c;
            }
        }
    }
}
