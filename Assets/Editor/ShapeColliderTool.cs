using System.Collections.Generic;
using System.Text;
using MementoMori.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MementoMori.EditorTools
{
    /// <summary>
    /// Solid props block along their drawn outline instead of a rough box, so Melantha cannot step into
    /// the parts of a bed, table or chest that stick out of the old rectangle. Foliage keeps its trunk
    /// footprint on purpose (the canopy is walk-behind and turns transparent).
    /// </summary>
    public static class ShapeColliderTool
    {
        [MenuItem("Memento Mori/Colliders/Contorno real nos objetos da cena aberta")]
        public static void ApplyToOpenScene() => Debug.Log(Apply());

        public static string Apply()
        {
            var report = new StringBuilder();
            var changed = 0;
            var candidates = new List<Collider2D>();
            candidates.AddRange(Object.FindObjectsByType<BoxCollider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            candidates.AddRange(Object.FindObjectsByType<CircleCollider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            candidates.AddRange(Object.FindObjectsByType<CapsuleCollider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            foreach (var old in candidates)
            {
                if (old == null || !old.enabled || !old.gameObject.activeInHierarchy || old.isTrigger || old.usedByComposite) continue;
                var go = old.gameObject;
                if (go.CompareTag("Player") || go.GetComponentInParent<Rigidbody2D>() is { bodyType: RigidbodyType2D.Dynamic }) continue;
                if (go.GetComponentInParent<SeeThroughFoliage>() != null) continue;
                var renderer = go.GetComponent<SpriteRenderer>();
                if (renderer == null || !renderer.enabled || renderer.sprite == null) continue;
                if (renderer.sprite.GetPhysicsShapeCount() == 0) continue;

                var poly = go.GetComponent<PolygonCollider2D>();
                if (poly == null) poly = Undo.AddComponent<PolygonCollider2D>(go);
                else Undo.RecordObject(poly, "Shape collider");
                FillFromSprite(poly, renderer.sprite);
                poly.isTrigger = false;
                poly.sharedMaterial = old.sharedMaterial;
                if (poly.pathCount == 0) { Undo.DestroyObjectImmediate(poly); continue; }
                report.AppendLine($"{go.name}: {old.GetType().Name} -> contorno ({poly.pathCount} contorno(s), {poly.GetTotalPointCount()} pontos)");
                Undo.DestroyObjectImmediate(old);
                EditorUtility.SetDirty(go);
                changed++;
            }
            if (changed > 0) EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            return $"{changed} objeto(s) com contorno real\n{report}";
        }

        // Free-standing props of the open maps whose whole drawing is solid (urns, braziers, gargoyles,
        // fountains, crystals, lecterns...). Wall facades, side faces, floor dials and the illusory
        // niches keep the map's own collision boundary.
        private static readonly HashSet<string> SolidPropSprites = new()
        {
            "e2a114ed1fb84210698949f4b321268363e337d6ca5970a9e07e4c0c8ecac5f0", // urns
            "04d9c95a44ce9f6bb44047bb5d7e778877e981d54fb9093922ccf5ae35da881d", // candle altar
            "b722930a3331182469d1ef287b74da04a82a93217fca1fe9b325686d3b4bfe55", // brazier pedestal
            "522d5ff87cbf1c613b267d7b4aba7568b74d2b08efce1c2db27210e74c95d686", // gargoyle
            "0342635b974e5681c5e6943146f012bb5e48addd31bf83d8bd6903d7d2e552c6", // rubble
            "6f68ae2371acfbe57c5834a576e196fdeb13e3da045acc98d020f8ca571e4a34", // pedestal
            "ec94658f0b354c26d6bbae142ca2ff5d232757886f0ae002edf09d522fdaee56", // flower urn
            "02eb329b03baff8a76fbf5f08a2ade6d51498bd2d57a43d5845f745fccc10912", // fragment pedestal
            "7be27e0ddf2572113766a70266a2e79e36445f2adfb283a7e3fa27a127c5639e", // crescent on the pedestal
            "99ab66670c3020c726a805772c7a58ea84fe3488770c62623ded22321b6b1550", // fallen column
            "b54b58c8877c4fe9909bc49715663eb24e16a56d605ad8ec377ad5f96b5d7277", // standing mirror
            "bd386470be742e72d5f3a64760aab35ebb9b3774ed556f203842da9302e37daf", // fountain
            "0e27cfae19455bc98c93214a62d34c9253f439d8396a67390a2306b7c7508ea8", // books
            "78eec36054e02443464c254ca83cdf0ea8971084ff9466c246f14a8ff5e2a3ba", // crystal cluster
            "25481a525aee542b4e64cd67088ba14c1d7be613c77953f935ba38b215489217", // lectern
            "344a5a4f2fd29a7d0891fb6f4da1f9b474f224976665e63279692873d122fbd4", // crystal shrine
            "c4dc834791b92123e309d44c18abf9ab14a9e904f2781113eaa96ed6e3d23c93", // bench
            "03076b1a8f015c99c4958012baaca0b579650d33393893fd515c328411f6ada8", // hooded statue
            "c315e54439c3db8e26c2224ecf2a7fab7249b334dc27babbd5c39a0ab5107a03", // bookshelf
            "518e0216cec6012d25591c2931b41a1801307477963cabdb7e106b9c65517e9e", // altar table
            "bf48a06cd59adf33b4756f5f0ebaa48c39be653ddb4051db3d3f441d2476b17b", // stone well
            "44221bb70de1a4259c83797fc29c8906e54de80716f169f05660786d7f275b92", // weeping angel
            "ec142e2077ddbe5c7dbcc145ceea4cc63aa2f200a170bbf84a038baaf9b9718f", // scales
            "8ff8553a6c8584c91c67e35085fb85727d263e12f0e0d95d7ed25ec061d29bee", // sarcophagus
        };

        public const string OutlineChild = "ContornoSolido";

        [MenuItem("Memento Mori/Colliders/Contorno real nos adereços dos mapas abertos")]
        public static void ApplyPropsToOpenScene() => Debug.Log(ApplySolidProps());

        /// <summary>Adds a solid outline child to every listed prop; the existing footprint stays.</summary>
        public static string ApplySolidProps(IEnumerable<string> skipObjects = null)
        {
            var skip = skipObjects != null ? new HashSet<string>(skipObjects) : new HashSet<string>();
            var report = new StringBuilder();
            var count = 0;
            foreach (var renderer in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            {
                if (!renderer.enabled || renderer.sprite == null || !IsSolidCandidate(renderer)) continue;
                var existing = renderer.transform.Find(OutlineChild);
                if (skip.Contains(renderer.name))
                {
                    if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);
                    continue;
                }
                var child = existing != null ? existing.gameObject : new GameObject(OutlineChild);
                if (existing == null)
                {
                    Undo.RegisterCreatedObjectUndo(child, "Prop outline");
                    child.transform.SetParent(renderer.transform, false);
                    child.layer = renderer.gameObject.layer;
                }
                var poly = child.GetComponent<PolygonCollider2D>();
                if (poly == null) poly = child.AddComponent<PolygonCollider2D>();
                FillFromSprite(poly, renderer.sprite, renderer.flipX, renderer.flipY);
                if (!IsWallArt(renderer)) ClampToGroundDepth(poly, renderer.sprite.bounds, renderer.flipY);
                poly.isTrigger = false;
                EditorUtility.SetDirty(child);
                count++;
                report.AppendLine(renderer.name);
            }
            if (count > 0) EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            return $"{count} adereço(s) com contorno sólido\n{report}";
        }

        private static bool IsSolidCandidate(SpriteRenderer renderer)
        {
            var assetPath = AssetDatabase.GetAssetPath(renderer.sprite);
            var name = renderer.name.ToLowerInvariant();
            if (name.Contains("layer_") || name.Contains("piso") || name.Contains("floor")
                || name.Contains("carpet") || name.Contains("tapete") || renderer.sortingOrder <= -20000
                || assetPath.Contains("/Tapete/") || assetPath.Contains("/Chao/") || assetPath.Contains("/Piso/")) return false;
            if (renderer.GetComponentInParent<SeeThroughFoliage>() != null) return false;
            if (SolidPropSprites.Contains(renderer.sprite.name)) return true;
            if (name.Contains("fachada") || name.Contains("facade") || name.Contains("juncao_parapeito")) return true;
            Component state = null;
            foreach (var component in renderer.GetComponents<MonoBehaviour>())
                if (component != null && component.GetType().Name == "V65StateVisual") { state = component; break; }
            if (state != null)
            {
                // Floor dials, sigil rings and the illusory arches are walked on or through:
                // only standing puzzle props (mirrors, garden basins, portals) are solid.
                var data = new SerializedObject(state);
                foreach (var field in new[] { "mirror", "garden", "portal" })
                    if (data.FindProperty(field)?.objectReferenceValue != null) return true;
                return false;
            }
            if (renderer.bounds.size.x > 12f || renderer.bounds.size.y > 16f) return false;
            if (assetPath.Contains("/Pendentes/")) return false;
            foreach (var collider in renderer.GetComponentsInChildren<Collider2D>(true))
                if (!collider.isTrigger && !(collider is UnityEngine.Tilemaps.TilemapCollider2D)
                    && !(collider is CompositeCollider2D)
                    && (collider.transform == renderer.transform || collider.transform.parent == renderer.transform)
                    && (collider.attachedRigidbody == null || collider.attachedRigidbody.bodyType != RigidbodyType2D.Dynamic))
                    return true;
            return false;
        }

        private static bool IsWallArt(SpriteRenderer renderer)
        {
            var name = renderer.name.ToLowerInvariant();
            return name.Contains("fachada") || name.Contains("facade") || name.Contains("juncao_parapeito")
                || name.Contains("layer_paredes") || renderer.sprite.name.StartsWith("Coroamento");
        }

        /// <summary>
        /// Three-quarter view: a standing object's ground depth is about 0.6 of its width. The outline above
        /// that is its body, drawn over Melantha when she walks behind it, so it must not block. Walls and
        /// facades keep their whole outline.
        /// </summary>
        private static void ClampToGroundDepth(PolygonCollider2D poly, Bounds spriteBounds, bool flipY)
        {
            var depth = Mathf.Clamp(spriteBounds.size.x * .6f, .5f, spriteBounds.size.y);
            var top = spriteBounds.min.y + depth;
            for (var i = 0; i < poly.pathCount; i++)
            {
                var path = poly.GetPath(i);
                for (var p = 0; p < path.Length; p++)
                {
                    var y = flipY ? -path[p].y : path[p].y;
                    if (y > top) path[p] = new Vector2(path[p].x, flipY ? -top : top);
                }
                poly.SetPath(i, path);
            }
        }

        /// <summary>Wall art keeps its tiles and transforms; physics follows each tile's sprite outline.</summary>
        public static string ApplyWallOutlines()
        {
            var maps = 0;
            foreach (var map in Object.FindObjectsByType<UnityEngine.Tilemaps.Tilemap>(FindObjectsSortMode.None))
            {
                if (!map.name.Contains("_layer_paredes")) continue;
                Undo.RecordObject(map, "Match wall collision to visible tiles");
                foreach (var cell in map.cellBounds.allPositionsWithin)
                    if (map.HasTile(cell)) map.SetColliderType(cell, UnityEngine.Tilemaps.Tile.ColliderType.Sprite);
                var collider = map.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
                if (collider == null) collider = Undo.AddComponent<UnityEngine.Tilemaps.TilemapCollider2D>(map.gameObject);
                collider.isTrigger = false;
                collider.ProcessTilemapChanges();
                EditorUtility.SetDirty(map);
                maps++;
            }
            if (maps > 0) EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            return maps + " wall tilemaps with sprite collision";
        }

        private static void FillFromSprite(PolygonCollider2D poly, Sprite sprite, bool flipX, bool flipY)
        {
            FillFromSprite(poly, sprite);
            if (!flipX && !flipY) return;
            for (var i = 0; i < poly.pathCount; i++)
            {
                var path = poly.GetPath(i);
                for (var p = 0; p < path.Length; p++)
                    path[p] = new Vector2(flipX ? -path[p].x : path[p].x, flipY ? -path[p].y : path[p].y);
                poly.SetPath(i, path);
            }
        }

        private static void FillFromSprite(PolygonCollider2D poly, Sprite sprite)
        {
            var count = sprite.GetPhysicsShapeCount();
            if (count == 0) return;
            poly.pathCount = count;
            var points = new List<Vector2>();
            for (var i = 0; i < count; i++)
            {
                points.Clear();
                sprite.GetPhysicsShape(i, points);
                poly.SetPath(i, points);
            }
            poly.offset = Vector2.zero;
        }
    }
}
