using System.Collections.Generic;
using System.Linq;
using MementoMori.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MementoMori.EditorTools
{
    /// <summary>
    /// Reproducible scene dressing passes (menu Memento Mori/Ambiente):
    /// solid footprints for statues, altars and wells; readable corridor bushes against the walls;
    /// see-through behaviour on every wind-animated foliage sprite.
    /// </summary>
    public static class SceneDressingTools
    {
        // Approved sprites that represent solid furniture (hooded statue, candle altar, stone well).
        private static readonly string[] SolidSpriteNames =
        {
            "03076b1a8f015c99c4958012baaca0b579650d33393893fd515c328411f6ada8",
            "04d9c95a44ce9f6bb44047bb5d7e778877e981d54fb9093922ccf5ae35da881d",
            "bf48a06cd59adf33b4756f5f0ebaa48c39be653ddb4051db3d3f441d2476b17b",
        };

        [MenuItem("Memento Mori/Ambiente/Colliders de base (cena aberta)")]
        public static string AddPropFootprints()
        {
            var scene = EditorSceneManager.GetActiveScene();
            Tilemap collision = null, nav = null;
            FindGrids(scene, ref collision, ref nav);
            int added = 0;
            foreach (var sr in AllSprites(scene))
            {
                if (sr.sprite == null || !SolidSpriteNames.Contains(sr.sprite.name)) continue;
                if (sr.GetComponentsInChildren<Collider2D>(true).Any(c => !c.isTrigger)) continue;
                var b = sr.bounds;
                if (WalkableFraction(b, collision, nav) <= .5f) continue;
                var footprint = new GameObject("Footprint", typeof(BoxCollider2D));
                Undo.RegisterCreatedObjectUndo(footprint, "Footprint");
                footprint.transform.SetParent(sr.transform, true);
                float h = Mathf.Min(.55f, b.size.y * .3f);
                footprint.transform.position = new Vector3(b.center.x, b.min.y + h * .5f + .04f, 0f);
                footprint.transform.rotation = Quaternion.identity;
                var box = footprint.GetComponent<BoxCollider2D>();
                var scale = footprint.transform.lossyScale;
                box.size = new Vector2(b.size.x * .72f / Mathf.Abs(scale.x), h / Mathf.Abs(scale.y));
                added++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            return $"{scene.name}: {added} footprints";
        }

        [MenuItem("Memento Mori/Ambiente/Moitas dos corredores (Labirinto)")]
        public static string RedistributeCorridorPlants()
        {
            var scene = EditorSceneManager.GetActiveScene();
            Tilemap collision = null, nav = null;
            FindGrids(scene, ref collision, ref nav);
            if (nav == null) return "no navigation floor";
            var plants = AllSprites(scene).Where(s => s.name.StartsWith("LabCorridorPlant_"))
                .OrderBy(s => int.Parse(s.name.Substring("LabCorridorPlant_".Length))).ToList();
            var random = new System.Random(1105);
            int kept = 0, removed = 0;
            foreach (var plant in plants)
            {
                Undo.RecordObject(plant.transform, "Corridor plant");
                Undo.RecordObject(plant, "Corridor plant");
                // A third of the evenly spaced copies go: the corridor should feel overgrown in patches, not tiled.
                if (random.NextDouble() < .34) { Undo.DestroyObjectImmediate(plant.gameObject); removed++; continue; }
                var origin = plant.bounds.center;
                float left = Extent(origin, Vector2.left, collision, nav), right = Extent(origin, Vector2.right, collision, nav);
                float up = Extent(origin, Vector2.up, collision, nav), down = Extent(origin, Vector2.down, collision, nav);
                bool horizontal = left + right > up + down;
                float s = .29f + (float)random.NextDouble() * .09f;
                plant.transform.localScale = new Vector3(s, s, 1f);
                plant.flipX = random.NextDouble() < .5;
                float along = ((float)random.NextDouble() * 2f - 1f) * 1.1f;
                Vector3 target;
                var size = plant.bounds.size;
                if (horizontal)
                {
                    bool top = random.NextDouble() < .72;
                    float baseY = top ? origin.y + up - .2f : origin.y - down + .08f;
                    float x = Mathf.Clamp(origin.x + along, origin.x - left + size.x * .5f, origin.x + right - size.x * .5f);
                    target = new Vector3(x, baseY, 0f);
                }
                else
                {
                    bool west = random.NextDouble() < .5;
                    float x = west ? origin.x - left + size.x * .45f : origin.x + right - size.x * .45f;
                    target = new Vector3(x, origin.y + along - size.y * .5f, 0f);
                }
                // place by the sprite's bottom-centre
                var offset = plant.transform.position - new Vector3(plant.bounds.center.x, plant.bounds.min.y, 0f);
                plant.transform.position = target + offset;
                plant.sortingOrder = Mathf.Clamp(5000 - Mathf.RoundToInt(plant.bounds.min.y * 100f), -32000, 32000);
                kept++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            return $"corridor plants kept {kept}, removed {removed}";
        }

        [MenuItem("Memento Mori/Ambiente/Vegetacao transparente (cena aberta)")]
        public static string AttachSeeThroughFoliage()
        {
            var scene = EditorSceneManager.GetActiveScene();
            int added = 0;
            foreach (var sr in AllSprites(scene))
            {
                var m = sr.sharedMaterial;
                if (m == null || !m.HasProperty("_Mode") || !Mathf.Approximately(m.GetFloat("_Mode"), 1f)) continue;
                var b = sr.bounds;
                if (b.size.x > 4f || b.size.y > 4f || sr.sortingOrder < 0) continue;
                if (sr.GetComponentsInChildren<Collider2D>(true).Any(c => !c.isTrigger)) continue;
                if (sr.GetComponent<SeeThroughFoliage>() != null) continue;
                Undo.AddComponent<SeeThroughFoliage>(sr.gameObject);
                added++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            return $"{scene.name}: see-through on {added} foliage sprites";
        }

        /// <summary>
        /// Second pass: every wind-animated plant (trees included, whatever their size or trunk collider) fades
        /// while Melantha walks behind it, and every flame turns half transparent in front of her instead of
        /// disappearing behind her body.
        /// </summary>
        [MenuItem("Memento Mori/Ambiente/Transparencia de arvores e chamas (cena aberta)")]
        public static string AttachSeeThroughTreesAndFlames()
        {
            var scene = EditorSceneManager.GetActiveScene();
            int plants = 0, flames = 0;
            foreach (var sr in AllSprites(scene))
            {
                if (sr.GetComponent<SeeThroughFoliage>() != null || sr.sortingOrder < 0) continue;
                var isFlame = sr.name.StartsWith("Flame_") || (sr.sprite != null && sr.sprite.name.StartsWith("flames_"));
                var m = sr.sharedMaterial;
                var isPlant = m != null && m.HasProperty("_Mode") && Mathf.Approximately(m.GetFloat("_Mode"), 1f);
                if (!isFlame && !isPlant) continue;
                var see = Undo.AddComponent<SeeThroughFoliage>(sr.gameObject);
                var so = new SerializedObject(see);
                if (isFlame)
                {
                    so.FindProperty("flame").boolValue = true;
                    so.FindProperty("coveredAlpha").floatValue = .6f;
                    flames++;
                }
                else
                {
                    // Large canopies stay a little more present than small bushes.
                    so.FindProperty("coveredAlpha").floatValue = sr.bounds.size.y > 4f ? .45f : .38f;
                    plants++;
                }
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene);
            return $"{scene.name}: see-through on {plants} more plants, {flames} flames";
        }

        private static IEnumerable<SpriteRenderer> AllSprites(UnityEngine.SceneManagement.Scene scene) =>
            scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<SpriteRenderer>(true)).ToList();

        private static void FindGrids(UnityEngine.SceneManagement.Scene scene, ref Tilemap collision, ref Tilemap nav)
        {
            foreach (var tm in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Tilemap>(true)))
            {
                if (tm.name == "CollisionBoundary") collision = tm;
                if (tm.name == "NavigationFloor") nav = tm;
            }
        }

        private static bool Walkable(Vector3 p, Tilemap collision, Tilemap nav) =>
            (collision == null || !collision.HasTile(collision.WorldToCell(p))) && (nav == null || nav.HasTile(nav.WorldToCell(p)));

        private static float WalkableFraction(Bounds b, Tilemap collision, Tilemap nav)
        {
            int walk = 0, total = 0;
            for (float x = b.min.x + b.size.x * .2f; x <= b.max.x - b.size.x * .2f; x += .125f)
                for (float y = b.min.y + .1f; y <= b.min.y + b.size.y * .4f; y += .125f)
                { total++; if (Walkable(new Vector3(x, y), collision, nav)) walk++; }
            return total == 0 ? 0f : walk / (float)total;
        }

        private static float Extent(Vector3 from, Vector2 dir, Tilemap collision, Tilemap nav)
        {
            float d = 0f;
            while (d < 8f && Walkable(from + (Vector3)(dir * (d + .125f)), collision, nav)) d += .125f;
            return d;
        }
    }
}
