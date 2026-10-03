using System;
using System.Collections.Generic;
using System.Linq;
using MementoMori.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MementoMori.EditorTools
{
    public static class FinalMapIntegration
    {
        private const string Root = "Assets/Art/FinalProduction/MapasFinais/";

        private readonly struct DomainRoom
        {
            public readonly string Key;
            public readonly Vector2Int Center;
            public readonly bool Open;
            public DomainRoom(string key, int x, int y, bool open = false)
            { Key = key; Center = new Vector2Int(x, y); Open = open; }
            public Vector3 Entry => new Vector3(Center.x, Center.y - 8, 0);
        }

        public static void BuildLabyrinth()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Labirinto.unity");
            var root = GameObject.Find("V3MapArt") ?? throw new InvalidOperationException("V3MapArt missing");
            var floor = root.transform.Find("Floor")?.GetComponent<Tilemap>();
            var walls = root.transform.Find("Walls")?.GetComponent<Tilemap>();
            var collision = root.transform.Find("Collision")?.GetComponent<Tilemap>();
            if (!floor || !walls || !collision) throw new InvalidOperationException("Final tilemaps missing");
            var floorTile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/V3Closure/LabFloorFilled.asset");
            var top = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/V3Closure/LabTopFilled.asset");
            var bottom = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/V3Closure/LabBottomFilled.asset");
            var left = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/V3Closure/LabLeftFilled.asset");
            var right = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/V3Closure/LabRightFilled.asset");
            var solid = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/V3Closure/RoomCollision.asset");
            if (!floorTile || !top || !bottom || !left || !right || !solid)
                throw new InvalidOperationException("V3 tiles missing");

            var cells = new HashSet<Vector3Int>();
            foreach (var cell in floor.cellBounds.allPositionsWithin)
                if (cell.y > -20 && floor.HasTile(cell)) cells.Add(cell);
            void Add(int x, int y) => cells.Add(new Vector3Int(x, y));
            void Corridor(int x0, int y0, int x1, int y1)
            {
                if (x0 != x1 && y0 != y1) throw new InvalidOperationException("Corridors must be orthogonal");
                var count = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
                for (var i = 0; i <= count; i++)
                {
                    var x = x0 + Math.Sign(x1 - x0) * i;
                    var y = y0 + Math.Sign(y1 - y0) * i;
                    for (var dx = -1; dx <= 1; dx++) for (var dy = -1; dy <= 1; dy++) Add(x + dx, y + dy);
                }
            }

            // Lower authored network: central route, three linked loops and four meaningful false spurs.
            Corridor(0, -18, 0, -88);
            Corridor(-38, -38, 38, -38);
            Corridor(-38, -50, 38, -50);
            Corridor(-38, -63, 38, -63);
            Corridor(-38, -38, -38, -63);
            Corridor(38, -38, 38, -63);
            Corridor(-12, -18, -12, -38);
            Corridor(-12, -38, 0, -38);
            Corridor(12, -18, 12, -38);
            Corridor(0, -38, 12, -38);
            Corridor(-38, -45, -48, -45);
            Corridor(-38, -56, -48, -56);
            Corridor(38, -45, 48, -45);
            Corridor(38, -56, 48, -56);
            Corridor(0, -72, -20, -72);
            Corridor(0, -73, 20, -73);

            var rooms = new[]
            {
                new DomainRoom("Moon", -25, -30, true),
                new DomainRoom("Devil", 0, -30),
                new DomainRoom("Tower", 25, -30),
                new DomainRoom("Hanged", -25, -55),
                new DomainRoom("Hermit", 0, -55),
                new DomainRoom("Death", 25, -55),
                new DomainRoom("Judgement", 0, -80)
            };
            foreach (var room in rooms)
                for (var x = -6; x <= 6; x++) for (var y = -6; y <= 6; y++) Add(room.Center.x + x, room.Center.y + y);

            floor.ClearAllTiles();
            foreach (var cell in cells) floor.SetTile(cell, floorTile);
            walls.ClearAllTiles();
            collision.ClearAllTiles();
            var border = new HashSet<Vector3Int>();
            foreach (var cell in cells)
                for (var x = -1; x <= 1; x++) for (var y = -1; y <= 1; y++)
                {
                    var next = cell + new Vector3Int(x, y);
                    if (!cells.Contains(next)) border.Add(next);
                }
            foreach (var cell in border)
            {
                var visual = cells.Contains(cell + Vector3Int.down) ? top
                    : cells.Contains(cell + Vector3Int.up) ? bottom
                    : cells.Contains(cell + Vector3Int.right) ? left : right;
                walls.SetTile(cell, visual);
                collision.SetTile(cell, solid);
            }
            collision.GetComponent<TilemapCollider2D>()?.ProcessTilemapChanges();

            var former = root.transform.Find("FinalDomainRooms");
            if (former) UnityEngine.Object.DestroyImmediate(former.gameObject);
            var visualRoot = new GameObject("FinalDomainRooms").transform;
            visualRoot.SetParent(root.transform, false);
            var sheet = AssetDatabase.LoadAllAssetsAtPath(Root + "L2_SealedDomainRooms.png")
                .OfType<Sprite>().ToDictionary(sprite => sprite.name);
            var moon = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "L2_MoonActiveRoom.png");
            if (!moon || sheet.Count < 6) throw new InvalidOperationException("Domain room sprites missing");
            var props = root.transform.Find("Props");
            foreach (var room in rooms)
            {
                var host = new GameObject("Room_" + room.Key).transform;
                host.SetParent(visualRoot, false);
                host.position = new Vector3(room.Center.x, room.Center.y, 0);
                var renderer = host.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = room.Open ? moon : sheet["L2_" + room.Key];
                renderer.sortingLayerName = "Default";
                renderer.sortingOrder = -25;
                Frame(host, -7.4f, 0, 1, 14, "LeftWall");
                Frame(host, 7.4f, 0, 1, 14, "RightWall");
                Frame(host, 0, 7.4f, 14, 1, "TopWall");
                Frame(host, -4.75f, -7.4f, 4.5f, 1, "LowerLeftWall");
                Frame(host, 4.75f, -7.4f, 4.5f, 1, "LowerRightWall");
                if (!room.Open) Frame(host, 0, -7.4f, 4, 1, "LockedGate");
                var portalName = room.Open ? "MoonPortal" : "DomainPortal_" + room.Key;
                var portal = GameObject.Find(portalName);
                if (!portal) throw new InvalidOperationException("Missing portal " + portalName);
                portal.transform.position = room.Entry;
                if (!room.Open)
                {
                    var obsolete = portal.transform.Find("LockedInteraction");
                    if (obsolete) UnityEngine.Object.DestroyImmediate(obsolete.gameObject);
                    var prompt = new GameObject("LockedInteraction");
                    prompt.transform.SetParent(portal.transform, false);
                    var zone = prompt.AddComponent<BoxCollider2D>();
                    zone.isTrigger = true;
                    zone.size = new Vector2(4, 3);
                    prompt.AddComponent<MementoMori.World.LockedDomainPortal>();
                    var oldPortal = portal.GetComponent<MementoMori.World.Portal>();
                    if (oldPortal) oldPortal.enabled = false;
                }
                if (props)
                    foreach (Transform child in props)
                        if (child.name.Contains("DomainPortal_" + room.Key)) child.position = room.Entry;
            }
            var lead = GameObject.Find("PoeFinalLeadPoint");
            if (lead) lead.transform.position = new Vector3(-12, -38, 0);
            var camera = Camera.main;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (camera && player)
                camera.GetComponent<CameraFollow2D>()?.Configure(player.transform, new Vector2(-45, -82), new Vector2(45, 30));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"FINAL_LABYRINTH_BUILT floor={cells.Count} border={border.Count} rooms={rooms.Length}");
        }

        private static void Frame(Transform root, float x, float y, float width, float height, string name)
        {
            var child = new GameObject(name).transform;
            child.SetParent(root, false);
            child.localPosition = new Vector3(x, y, 0);
            var box = child.gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(width, height);
        }
    }
}
