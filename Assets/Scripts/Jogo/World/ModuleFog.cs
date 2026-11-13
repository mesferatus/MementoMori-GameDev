using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.World
{
    /// <summary>
    /// Labyrinth rooms feel enclosed: while Melantha stands in a module, every other module sinks under a slow
    /// violet mist; in the corridors all modules keep only a light haze. Modules are read from the authored
    /// module layers (Lxx_layer_*), so the map itself is untouched.
    /// </summary>
    public sealed class ModuleFog : MonoBehaviour
    {
        private const float RoomFog = .42f, CorridorFog = .16f, FadeSpeed = 1.6f;
        private static readonly Color Mist = new(.07f, .035f, .12f, 1f);
        private readonly List<(Rect rect, SpriteRenderer fog, float drift)> modules = new();
        private Transform player;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded += (scene, _) => Attach(scene);
            Attach(SceneManager.GetActiveScene());
        }

        private static void Attach(Scene scene)
        {
            if (scene.name != "Labirinto" || FindAnyObjectByType<ModuleFog>() != null) return;
            var go = new GameObject("ModuleFog");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.AddComponent<ModuleFog>();
        }

        private void Start()
        {
            var bounds = new Dictionary<string, Bounds>();
            var pattern = new Regex(@"^(L\d\d)_layer_");
            foreach (var renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                var match = pattern.Match(renderer.name);
                if (!match.Success || renderer.bounds.size.x < 4f) continue;
                var key = match.Groups[1].Value;
                if (bounds.TryGetValue(key, out var b)) { b.Encapsulate(renderer.bounds); bounds[key] = b; }
                else bounds[key] = renderer.bounds;
            }
            var sprite = CloudSprite();
            var i = 0;
            foreach (var pair in bounds)
            {
                var b = pair.Value;
                var go = new GameObject("Fog_" + pair.Key);
                go.transform.SetParent(transform, false);
                go.transform.position = new Vector3(b.center.x, b.center.y, 0f);
                var fog = go.AddComponent<SpriteRenderer>();
                fog.sprite = sprite;
                fog.drawMode = SpriteDrawMode.Simple;
                fog.transform.localScale = new Vector3((b.size.x + 3f) / sprite.bounds.size.x, (b.size.y + 3f) / sprite.bounds.size.y, 1f);
                fog.sortingOrder = 31000;
                fog.color = new Color(Mist.r, Mist.g, Mist.b, 0f);
                modules.Add((new Rect(b.min.x, b.min.y, b.size.x, b.size.y), fog, i++ * 1.7f));
            }
        }

        private void Update()
        {
            if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) return;
            var at = (Vector2)player.position;
            var inside = -1;
            for (var i = 0; i < modules.Count; i++) if (modules[i].rect.Contains(at)) { inside = i; break; }
            for (var i = 0; i < modules.Count; i++)
            {
                var (rect, fog, drift) = modules[i];
                // Authored module bounds can overlap: clear every layer covering the player.
                var target = rect.Contains(at) ? 0f : inside < 0 ? CorridorFog : RoomFog;
                var c = fog.color;
                c.a = Mathf.MoveTowards(c.a, target, FadeSpeed * Time.deltaTime);
                fog.color = c;
                // slow drift so the mist reads as moving air, not a flat tint
                fog.transform.position = new Vector3(rect.center.x + Mathf.Sin(Time.time * .11f + drift) * .35f,
                    rect.center.y + Mathf.Cos(Time.time * .08f + drift) * .25f, 0f);
            }
        }

        /// <summary>Tileable soft cloud texture (value noise, two octaves), generated once.</summary>
        internal static Sprite CloudSprite()
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var lattice = new float[8, 8];
            var rnd = new System.Random(17);
            for (var y = 0; y < 8; y++) for (var x = 0; x < 8; x++) lattice[x, y] = (float)rnd.NextDouble();
            float Noise(float u, float v, int cells)
            {
                var fx = u * cells; var fy = v * cells;
                int x0 = (int)fx, y0 = (int)fy; float tx = fx - x0, ty = fy - y0;
                tx = tx * tx * (3 - 2 * tx); ty = ty * ty * (3 - 2 * ty);
                float L(int x, int y) => lattice[(x % cells) % 8, (y % cells) % 8];
                return Mathf.Lerp(Mathf.Lerp(L(x0, y0), L(x0 + 1, y0), tx), Mathf.Lerp(L(x0, y0 + 1), L(x0 + 1, y0 + 1), tx), ty);
            }
            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    float u = x / (float)size, v = y / (float)size;
                    var n = Noise(u, v, 4) * .65f + Noise(u, v, 8) * .35f;
                    var edge = Mathf.Min(Mathf.Min(u, 1f-u), Mathf.Min(v, 1f-v));
                    var feather = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(edge / .22f));
                    // An elliptical falloff breaks the rectangular silhouette of the quad.
                    var radius = new Vector2((u - .5f) * 2f, (v - .5f) * 2f).magnitude;
                    feather *= Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - radius) / .5f));
                    pixels[y * size + x] = new Color(1f, 1f, 1f, Mathf.Lerp(.12f, .9f, n) * feather);
                }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 8f, 0, SpriteMeshType.FullRect);
        }
    }
}
