using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace MementoMori.World
{
    /// <summary>
    /// Each "_layer_paredes" tilemap is one 2-unit row of wall art and was sorted by the bottom of that row.
    /// Melantha walking beside a low wall or pillar, with her feet in the upper half of the row, was then drawn
    /// behind it. Sorting the row by the top of its solid body keeps her in front while she is beside or south
    /// of the wall and still behind it once she has walked past its top edge.
    /// The same applies to the cut-out "_layer_paredes_slice" pieces.
        /// Also tints the Moon Domain walls toward the moonlit palette so the mist reads over them like the rest.
    /// </summary>
    public static class WallRowDepth
    {
        private static readonly System.Collections.Generic.HashSet<string> SigilDaisPieces = new()
        {
            "M07_layer_paredes_slice (35)", "M07_layer_paredes_slice (36)", "M07_layer_paredes_slice (37)",
            "M07_layer_paredes_slice (38)", "M07_layer_paredes_slice (39)", "M07_layer_paredes_slice (40)"
        };
        private static readonly Color MoonWallTint = new(.64f, .68f, .86f, 1f);
        // The joint art (Coroamento_Lua) is bluer than the brick tiles: a warmer tint lands it on the walls' grey.
        private static readonly string[] WalkOnDais = { "L06_004" };
        private static readonly Color MoonJointTint = new(.74f, .75f, .8f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded -= OnLoaded;
            SceneManager.sceneLoaded += OnLoaded;
            Apply(SceneManager.GetActiveScene());
        }

        private static void OnLoaded(Scene scene, LoadSceneMode mode) => Apply(scene);

        public static void Apply(Scene scene)
        {
            if (scene.name != "Labirinto" && scene.name != "DominioLua") return;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var pinned in root.GetComponentsInChildren<Renderer>(true))
            {
                // Wall slices, facades and one stair map were pinned to the top order (32767): drawn over the
                // mist (30000), the gargoyles and Melantha from every side. Put them back on the depth scale.
                if (pinned.sortingOrder < 32000 || pinned is ParticleSystemRenderer) continue;
                if (pinned.name.Contains("_layer_escadas")) { pinned.sortingOrder = -20500; continue; }
                if (scene.name == "DominioLua" && SigilDaisPieces.Contains(pinned.name))
                {
                    // The Sigil dais steps: depth-sorted so the gargoyles and Melantha are not covered.
                    var body = pinned.GetComponentInChildren<Collider2D>();
                    var ground = body != null && !body.isTrigger ? body.bounds.max.y : pinned.bounds.min.y;
                    pinned.sortingOrder = Mathf.Clamp(5000 - Mathf.RoundToInt(ground * 100f), -32000, 29000);
                }
                // The wall joints stay on top as authored, just under the mist so it veils them like the rest.
                else pinned.sortingOrder = 29000;
            }
            // Low daises the player is meant to climb onto: draw them as floor so she walks over them.
            foreach (var name in WalkOnDais)
                foreach (var root in scene.GetRootGameObjects())
                foreach (var dais in root.GetComponentsInChildren<SpriteRenderer>(true))
                    if (dais.name == name && dais.gameObject.scene == scene) dais.sortingOrder = -20300;
            // A joint uses the wall's ground edge, so an actor beside its south face stays visible.
            foreach (var root in scene.GetRootGameObjects())
            foreach (var joint in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (!joint.name.Contains("_layer_paredes")) continue;
                var solids = joint.GetComponentsInChildren<Collider2D>(true)
                    .Where(c => c.enabled && !c.isTrigger && !(c is TilemapCollider2D)
                        && !(c is CompositeCollider2D)).ToArray();
                var ground = solids.Length > 0 ? solids.Max(c => c.bounds.max.y) : joint.bounds.max.y;
                joint.sortingOrder = Mathf.Clamp(5000 - Mathf.RoundToInt(ground * 100f), -32000, 29000);
            }
            // Every joint gets the same tint, pinned or not.
            if (scene.name == "DominioLua")
                foreach (var root in scene.GetRootGameObjects())
                foreach (var sprite in root.GetComponentsInChildren<SpriteRenderer>(true))
                    if (sprite.name.Contains("_layer_paredes"))
                        sprite.color = MoonJointTint * new Color(1f, 1f, 1f, sprite.color.a);
            // Labirinto wall and module rows (altars, portals, low walls) had no collision of their own, only the
            // outer boundary: Melantha walked into the altar modules. Each row gets its art's outline as a solid
            // shape and sorts by its foot. Verified: every room and all interactables stay reachable.
            var shaped = new HashSet<TilemapRenderer>();
            if (scene.name == "Labirinto")
                foreach (var root in scene.GetRootGameObjects())
                foreach (var renderer in root.GetComponentsInChildren<TilemapRenderer>(true))
                {
                    if (!renderer.name.Contains("_layer_paredes")) continue;
                    var map = renderer.GetComponent<Tilemap>();
                    var existing = renderer.GetComponent<TilemapCollider2D>();
                    if (map == null || existing != null && existing.enabled || map.GetUsedTilesCount() == 0) continue;
                    if (existing == null) existing = renderer.gameObject.AddComponent<TilemapCollider2D>();
                    existing.enabled = true;
                    // Collision only: the authored draw order stays, so the wall pieces keep joining as drawn.
                    shaped.Add(renderer);
                }
            foreach (var root in scene.GetRootGameObjects())
            foreach (var renderer in root.GetComponentsInChildren<TilemapRenderer>(true))
            {
                if (!renderer.name.Contains("_layer_paredes") || shaped.Contains(renderer)) continue;
                var solid = renderer.GetComponent<TilemapCollider2D>();
                if (solid == null || !solid.enabled) continue;
                var composite = renderer.GetComponent<CompositeCollider2D>();
                var bounds = composite != null && composite.enabled ? composite.bounds : solid.bounds;
                if (bounds.size.y <= 0f) continue;
                renderer.sortingOrder = Mathf.Clamp(5000 - Mathf.RoundToInt(bounds.max.y * 100f), -32000, 32000);
                if (scene.name == "DominioLua")
                {
                    var map = renderer.GetComponent<Tilemap>();
                    if (map != null) map.color = MoonWallTint;
                }
            }
        }
    }
}
