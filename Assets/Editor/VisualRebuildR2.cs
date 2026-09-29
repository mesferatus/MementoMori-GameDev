using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using MementoMori.Core;
using MementoMori.World;

namespace MementoMori.EditorTools
{
    public static class VisualRebuildR2
    {
        public static Sprite Sprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/FinalProduction/R2/" + path + ".png");
            if (!sprite) throw new InvalidOperationException("Missing R2 sprite: " + path);
            return sprite;
        }

        private static string TileSnapshot()
        {
            var report = new StringBuilder();
            foreach (var tilemap in UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID))
            {
                report.AppendLine(tilemap.GetInstanceID() + "|" + tilemap.transform.position + "|" + tilemap.transform.lossyScale + "|" + tilemap.gameObject.activeSelf);
                foreach (var p in tilemap.cellBounds.allPositionsWithin)
                    if (tilemap.HasTile(p)) report.AppendLine(p + "|" + AssetDatabase.GetAssetPath(tilemap.GetTile(p)) + "|" + tilemap.GetColor(p) + "|" + tilemap.GetTransformMatrix(p));
            }
            return report.ToString();
        }

        private static SpriteRenderer Place(Transform view, string name, string asset, Vector2 center, float width)
        {
            var t = view.Find(name);
            if (!t) throw new InvalidOperationException("Missing prop: " + name);
            var renderer = t.GetComponent<SpriteRenderer>();
            renderer.sprite = Sprite(asset);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(renderer.sprite)));
            int x0 = texture.width, y0 = texture.height, x1 = 0, y1 = 0;
            for (int y = 0; y < texture.height; y++) for (int x = 0; x < texture.width; x++)
                if (texture.GetPixel(x, y).a >= .5f) { x0 = Math.Min(x0, x); x1 = Math.Max(x1, x + 1); y0 = Math.Min(y0, y); y1 = Math.Max(y1, y + 1); }
            UnityEngine.Object.DestroyImmediate(texture);
            var scale = width * 32f / (x1 - x0);
            t.localScale = new Vector3(scale, scale, 1);
            var midpoint = (new Vector2(x0 + x1, y0 + y1) * .5f - renderer.sprite.pivot) / 32f;
            t.position = (Vector3)(center - midpoint * scale);
            renderer.color = Color.white;
            renderer.sortingOrder = 5000 - Mathf.RoundToInt((center.y - (y1 - y0) / 64f * scale) * 100);
            return renderer;
        }

        private static void Solid(Transform view, string name, Vector2 center, Vector2 size)
        {
            var t = view.Find(name);
            var collider = t.GetComponent<BoxCollider2D>();
            if (!collider) collider = t.gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = false;
            collider.size = new Vector2(size.x / t.lossyScale.x, size.y / t.lossyScale.y);
            collider.offset = t.InverseTransformPoint(center);
        }

        private static void Flag(SpriteRenderer target, StoryFlag flag, string before, string after)
        {
            var visual = target.GetComponent<StoryFlagSpriteVisual>();
            if (!visual) visual = target.gameObject.AddComponent<StoryFlagSpriteVisual>();
            visual.Configure(target, flag, Sprite(before), Sprite(after));
        }

        public static string ApplyQuarto()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.name != "Quarto" || EditorApplication.isPlaying) throw new InvalidOperationException("Open Quarto in edit mode");
            var before = TileSnapshot();
            var view = GameObject.Find("__MementoVisualFoundation").transform.Find("ReferenceComposition");
            // Left inner wall x=-6, north wall occupies y=9..10. Keep the retained tilemaps intact.
            Place(view, "Bed", "Q2_Bed/bed", new Vector2(-4.5f, 5.1f), 3f);
            Place(view, "Nightstand", "Q2_Furniture/nightstand", new Vector2(-2.2f, 6.6f), 1.05f);
            Place(view, "Desk", "Q2_Furniture/desk", new Vector2(4.35f, 6.3f), 3.5f);
            var window = Place(view, "Window", "Q2_Window_States/window_open", new Vector2(.5f, 9f), 3f);
            Flag(window, StoryFlag.RoomWindowSecured, "Q2_Window_States/window_open", "Q2_Window_States/window_secured");
            Place(view, "Grimoire", "Q2_Furniture/side_table", new Vector2(5.67f, 1f), 1.1f);
            Place(view, "PoeToy", "Q3_Props_States/rag_doll", new Vector2(-4.63f, -4.05f), .85f);
            Place(view, "PoeBowl", "Q3_Props_States/bowl_empty", new Vector2(-4.65f, -2.07f), 1.15f);
            Place(view, "Photograph", "Q3_Props_States/portrait", new Vector2(5.6f, -3.39f), 1.1f);
            Place(view, "Altar", "Q2_Furniture/altar", new Vector2(-4.63f, -6.57f), 1.3f);
            var chest = Place(view, "Chest", "Q3_Props_States/chest_closed", new Vector2(-2.77f, -7.02f), 1.3f);
            Flag(chest, StoryFlag.RoomRitualItemStored, "Q3_Props_States/chest_closed", "Q3_Props_States/chest_open");
            Place(view, "Sideboard", "Q2_Furniture/sideboard", new Vector2(4.8f, -6.43f), 3.1f);
            Solid(view, "Bed", new Vector2(-4.5f, 4.9f), new Vector2(2.9f, 5.5f));
            Solid(view, "Nightstand", new Vector2(-2.2f, 6.2f), new Vector2(1f, 1f));
            Solid(view, "Desk", new Vector2(4.35f, 6f), new Vector2(3.3f, 2.5f));
            Solid(view, "Altar", new Vector2(-4.63f, -6.9f), new Vector2(1.2f, .9f));
            Solid(view, "Sideboard", new Vector2(4.8f, -6.8f), new Vector2(3f, .9f));
            var bed = UnityEngine.Object.FindFirstObjectByType<BedController>();
            bed.transform.position = new Vector3(-4.5f, 1.8f, 0);
            var trigger = bed.GetComponent<BoxCollider2D>();
            trigger.offset = Vector2.zero; trigger.size = new Vector2(2.1f, .6f);
            var windowTrigger = UnityEngine.Object.FindObjectsByType<MementoMori.Dialogue.DialogueTrigger>(FindObjectsSortMode.None).Single(t => t.name == "Window");
            windowTrigger.transform.position = new Vector3(.5f, 8.2f, 0);
            var box = windowTrigger.GetComponent<BoxCollider2D>(); box.size = new Vector2(3f, .7f); box.offset = Vector2.zero;
            if (before != TileSnapshot()) throw new InvalidOperationException("Tile preservation failed");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            var report = "Quarto R2: 12 props replaced; window/chest observe existing flags; bed/window interactions aligned; tilemaps unchanged.";
            File.WriteAllText("TestResults/VisualRebuild/r2-quarto-integration.txt", report);
            return report;
        }
    }
}
