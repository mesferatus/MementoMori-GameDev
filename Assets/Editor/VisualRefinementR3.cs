using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MementoMori.EditorTools
{
    public static class VisualRefinementR3
    {
        public static string EnlargeMap()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || (scene.name != "Labirinto" && scene.name != "DominioLua"))
                throw new InvalidOperationException("Open a map in edit mode.");
            var root = GameObject.Find("V3MapArt");
            if (!root || root.transform.Find("R3ExpansionApplied")) throw new InvalidOperationException("Missing map or R3 already applied.");
            var floor = root.transform.Find("Floor").GetComponent<Tilemap>();
            var walls = root.transform.Find("Walls").GetComponent<Tilemap>();
            var collision = root.transform.Find("Collision").GetComponent<Tilemap>();
            var oldCellList = new List<Vector3Int>();
            foreach (var position in floor.cellBounds.allPositionsWithin)
                if (floor.HasTile(position)) oldCellList.Add(position);
            var oldCells = oldCellList.ToArray();
            TileBase collisionTile = null;
            foreach (var position in collision.cellBounds.allPositionsWithin)
                if (collision.HasTile(position)) { collisionTile = collision.GetTile(position); break; }
            if (!collisionTile) throw new InvalidOperationException("Missing retained collision tile.");
            var family = scene.name == "DominioLua" ? "Moon" : "Labyrinth";
            TileBase Tile(int row, int col) => AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Art/FinalProduction/R3/" + family + "Tiles/tile_" + row + "_" + col + ".asset");
            if (!Tile(0, 0) || !Tile(1, 0)) throw new InvalidOperationException("Import R3 tiles first.");

            // Snapshot world positions before changing parents; never scale actors or UI.
            var transforms = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                .Where(t => !(t is RectTransform) && !t.GetComponentInParent<Canvas>(true)).ToArray();
            var positions = transforms.ToDictionary(t => t, t => t.position);
            int Depth(Transform t) { int d = 0; while (t.parent) { d++; t = t.parent; } return d; }
            foreach (var t in transforms.OrderBy(Depth))
            {
                var p = positions[t]; t.position = new Vector3(p.x * 2, p.y * 2, p.z);
            }
            // Props have different pivots; scale their visual centers, not their sprite offsets.
            foreach (var renderer in root.transform.Find("Props").GetComponentsInChildren<SpriteRenderer>(true))
                renderer.transform.position += renderer.bounds.center - renderer.transform.position;
            // Serialized positions are spatial dependencies, unlike vectors used for facing or speed.
            foreach (var component in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MonoBehaviour>(true)))
            {
                if (!component || component.GetComponentInParent<Canvas>(true)) continue;
                var serialized = new SerializedObject(component);
                foreach (var name in new[] { "returnPosition", "safePosition", "fullReflectionPosition", "minBounds", "maxBounds" })
                {
                    var property = serialized.FindProperty(name);
                    if (property != null && property.propertyType == SerializedPropertyType.Vector2) property.vector2Value *= 2;
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            foreach (var door in UnityEngine.Object.FindObjectsByType<MementoMori.World.DoorController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var serialized = new SerializedObject(door);
                var blocker = serialized.FindProperty("blockingCollider")?.objectReferenceValue as BoxCollider2D;
                if (blocker) { blocker.size *= 2; blocker.offset *= 2; }
            }
            var cells = new HashSet<Vector3Int>();
            foreach (var p in oldCells) for (int x = 0; x < 2; x++) for (int y = 0; y < 2; y++) cells.Add(new Vector3Int(p.x * 2 + x, p.y * 2 + y, p.z));
            floor.ClearAllTiles(); walls.ClearAllTiles(); collision.ClearAllTiles();
            foreach (var p in cells) floor.SetTile(p, Tile(0, (Math.Abs(p.x * 17 + p.y * 31) % 19 == 0) ? 1 : 0));
            var border = new HashSet<Vector3Int>();
            foreach (var p in cells) for (int x = -1; x <= 1; x++) for (int y = -1; y <= 1; y++)
            { var q = p + new Vector3Int(x, y, 0); if (!cells.Contains(q)) border.Add(q); }
            foreach (var p in border)
            {
                var tile = cells.Contains(p + Vector3Int.down) ? Tile(1, 0) : cells.Contains(p + Vector3Int.up) ? Tile(2, 1) : cells.Contains(p + Vector3Int.right) ? Tile(2, 2) : Tile(2, 3);
                walls.SetTile(p, tile); collision.SetTile(p, collisionTile);
            }
            floor.color = family == "Moon" ? new Color(.68f, .71f, .83f) : new Color(.78f, .72f, .82f);
            walls.color = Color.white;
            collision.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
            var marker = new GameObject("R3ExpansionApplied"); marker.transform.SetParent(root.transform, false);
            var report = scene.name + ": floor cells " + oldCells.Length + " -> " + cells.Count + "; width/height doubled, area x4; transforms and spatial fields migrated; new R3 tiles applied.";
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            File.WriteAllText("TestResults/VisualRebuild/r3-" + scene.name + "-space.txt", report);
            return report;
        }
    }
}
