using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MementoMori.EditorTools
{
    // Operates only on the active official Moon scene. The cell data was exported
    // from the exact approved commit; no historical art assets are imported.
    public static class MoonLuizaIntegration
    {
        const string ScenePath = "Assets/Scenes/DominioLua.unity";
        const string DataPath = "Assets/Editor/LuizaGeometry_d61d74f.json";
        const string TilePath = "Assets/Art/FinalProduction/R3/MoonTiles/tile_";
        const string WallPath = "Assets/Art/Tiles/V3Closure/DominioLuaWall";

        [Serializable] sealed class Geometry { public string commit; public Map[] maps; }
        [Serializable] sealed class Map { public string name; public int[] cells; }

        static void EnsureScene()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != ScenePath)
                throw new InvalidOperationException("Abra Assets/Scenes/DominioLua.unity antes da integracao.");
        }

        static TileBase Tile(string path)
        {
            var tile = AssetDatabase.LoadAssetAtPath<TileBase>(path);
            if (tile == null) throw new FileNotFoundException("Tile aprovado ausente", path);
            return tile;
        }

        static GameObject Child(Transform parent, string name)
        {
            var found = parent.Find(name);
            if (found != null) return found.gameObject;
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Integrar geometria da Luiza");
            go.transform.SetParent(parent, false);
            return go;
        }

        static Tilemap Layer(Transform grid, string name, int order)
        {
            var go = Child(grid, name);
            var map = go.GetComponent<Tilemap>();
            var renderer = go.GetComponent<TilemapRenderer>();
            if (map == null || renderer == null)
                throw new InvalidOperationException("Crie Tilemap e TilemapRenderer com o Unity MCP: " + name);
            renderer.sortingOrder = order;
            map.ClearAllTiles();
            return map;
        }

        static HashSet<Vector2Int> Points(Map map)
        {
            var set = new HashSet<Vector2Int>();
            if (map == null || map.cells == null) return set;
            for (var i = 0; i + 2 < map.cells.Length; i += 3)
                set.Add(new Vector2Int(map.cells[i], map.cells[i + 1]));
            return set;
        }

        static void Set(Tilemap map, Vector2Int p, TileBase tile) =>
            map.SetTile(new Vector3Int(p.x, p.y, 0), tile);

        [MenuItem("Memento Mori/Mapas Lua/01 Importar planta da Luiza")]
        public static void ImportGeometry()
        {
            EnsureScene();
            var json = File.ReadAllText(DataPath);
            var data = JsonUtility.FromJson<Geometry>(json);
            if (data == null || data.commit != "d61d74f9bcb553d524d94a72350f7f938729710a")
                throw new InvalidDataException("A fonte geometrica nao corresponde ao commit aprovado.");
            var source = new Dictionary<string, Map>();
            foreach (var map in data.maps) source[map.name] = map;
            if (Points(source["Tilemap (1)"]).Count != 3384 || Points(source["Tilemap"]).Count != 1711)
                throw new InvalidDataException("Contagem de celulas da Luiza diverge da evidencia aprovada.");

            var root = GameObject.Find("LuizaOriginalGeometry") ?? new GameObject("LuizaOriginalGeometry");
            if (root.transform.parent != null) root.transform.SetParent(null);
            root.transform.position = Vector3.zero;
            var gridGo = Child(root.transform, "Grid");
            var grid = gridGo.GetComponent<Grid>();
            if (grid == null) throw new InvalidOperationException("Crie Grid com o Unity MCP.");
            grid.cellSize = Vector3.one;
            grid.cellGap = Vector3.zero;

            var ground = Layer(grid.transform, "Ground_Original_3384", -20);
            var walls = Layer(grid.transform, "Walls_Original_1711", -10);
            var details = Layer(grid.transform, "Details_Original", -9);
            var collision = Layer(grid.transform, "Collision_Original", -8);
            var floorTiles = new[] {
                Tile(TilePath + "0_0.asset"), Tile(TilePath + "0_1.asset"),
                Tile(TilePath + "0_2.asset"), Tile(TilePath + "0_3.asset")
            };
            var wallTop = Tile(WallPath + "Top.asset");
            var wallLeft = Tile(WallPath + "Left.asset");
            var wallRight = Tile(WallPath + "Right.asset");
            var wallBottom = Tile(WallPath + "Bottom.asset");
            var collisionTile = Tile("Assets/Art/Tiles/V3Closure/RoomCollision.asset");
            var floor = Points(source["Tilemap (1)"]);
            var solid = Points(source["Tilemap"]);
            foreach (var p in floor)
            {
                var v = (Mathf.Abs(p.x * 17 + p.y * 31) % 19 == 0) ? 1 : 0;
                Set(ground, p, floorTiles[v]);
            }
            foreach (var p in solid)
            {
                var t = !solid.Contains(p + Vector2Int.up) ? wallTop :
                    !solid.Contains(p + Vector2Int.left) ? wallLeft :
                    !solid.Contains(p + Vector2Int.right) ? wallRight : wallBottom;
                Set(walls, p, t);
                Set(collision, p, collisionTile);
            }
            foreach (var name in new[] { "Tilemap (3)", "Tilemap (4)", "Tilemap (5)",
                         "Tilemap (6)", "Tilemap (7)", "Tilemap (8)" })
            {
                var pts = Points(source[name]);
                foreach (var p in pts)
                    if (!solid.Contains(p)) Set(details, p, floorTiles[2 + (Mathf.Abs(p.x + p.y) % 2)]);
            }

            // The main and southern floor components are disconnected at y=-80.
            // A two-cell aperture matches the existing three-cell corridor below.
            foreach (var x in new[] { -5, -4 })
            {
                var p = new Vector2Int(x, -80);
                Set(ground, p, floorTiles[0]);
                Set(walls, p, null);
                Set(collision, p, null);
            }
            if (collision.GetComponent<TilemapCollider2D>() == null)
                Debug.LogWarning("Adicione TilemapCollider2D em Collision_Original usando o Unity MCP.");

            // Preserve functional GameObjects; retire only old compact tile surfaces.
            var old = GameObject.Find("V3MapArt");
            if (old != null)
                foreach (var map in old.GetComponentsInChildren<Tilemap>(true))
                {
                    if (map.TryGetComponent<TilemapRenderer>(out var renderer)) renderer.enabled = false;
                    foreach (var collider in map.GetComponents<Collider2D>()) collider.enabled = false;
                }
            var environment = GameObject.Find("SceneRoot/Environment/Grid");
            if (environment != null)
                foreach (var map in environment.GetComponentsInChildren<Tilemap>(true))
                {
                    if (map.TryGetComponent<TilemapRenderer>(out var renderer)) renderer.enabled = false;
                    foreach (var collider in map.GetComponents<Collider2D>()) collider.enabled = false;
                }
            var background = GameObject.Find("RuntimeBackground");
            if (background != null)
            {
                if (background.TryGetComponent<SpriteRenderer>(out var sprite)) sprite.enabled = false;
                if (background.TryGetComponent<Collider2D>(out var collider)) collider.enabled = false;
            }
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("LUIZA_D61D74F_GEOMETRY_IMPORTED: 3384 floor, 1711 wall cells; 2-cell M5-M6 aperture; original gameplay objects retained.");
        }

        static void Shift(string path, Vector2 delta)
        {
            var go = GameObject.Find(path);
            if (go != null) go.transform.position += (Vector3)delta;
            else Debug.LogWarning("Objeto para realocacao nao encontrado: " + path);
        }

        static void Move(string path, float x, float y)
        {
            var go = GameObject.Find(path);
            if (go != null)
            {
                var z = go.transform.position.z;
                go.transform.position = new Vector3(x, y, z);
            }
            else Debug.LogWarning("Objeto para realocacao nao encontrado: " + path);
        }

        static void ShiftChildrenByPrefix(string parentPath, string prefix, Vector2 delta)
        {
            var parent = GameObject.Find(parentPath);
            if (parent == null) return;
            foreach (Transform child in parent.transform)
                if (child.name.StartsWith(prefix, StringComparison.Ordinal))
                    child.position += (Vector3)delta;
        }

        static void SetVector2(UnityEngine.Object component, string name, Vector2 value)
        {
            if (component == null) return;
            var serialized = new SerializedObject(component);
            var property = serialized.FindProperty(name);
            if (property == null) throw new InvalidOperationException(component.name + "." + name + " ausente");
            property.vector2Value = value;
            serialized.ApplyModifiedProperties();
        }

        [MenuItem("Memento Mori/Mapas Lua/02 Realocar gameplay e arte existente")]
        public static void RepositionExisting()
        {
            EnsureScene();
            if (GameObject.Find("LuizaGameplayPlaced") != null)
                throw new InvalidOperationException("Realocacao ja aplicada nesta cena.");
            if (GameObject.Find("LuizaOriginalGeometry") == null)
                throw new InvalidOperationException("Importe primeiro a planta original.");

            const string puzzleObjects = "SceneRoot/Gameplay/PuzzleObjects";
            ShiftChildrenByPrefix(puzzleObjects, "Petala", new Vector2(35, -60));
            ShiftChildrenByPrefix(puzzleObjects, "LabyrinthMirror_", new Vector2(20, -22.4f));
            ShiftChildrenByPrefix(puzzleObjects, "EcoPassagem_", new Vector2(14, -49));
            ShiftChildrenByPrefix(puzzleObjects, "SigilRing_", new Vector2(0, -61.8f));
            // The three indexed passages retain the original puzzle solution.
            // Passage 4 stays serialized as auxiliary but is not a visible choice.
            var auxiliary = GameObject.Find(puzzleObjects + "/EcoPassagem_4");
            if (auxiliary != null)
            {
                if (auxiliary.TryGetComponent<SpriteRenderer>(out var render)) render.enabled = false;
                if (auxiliary.TryGetComponent<BoxCollider2D>(out var collider)) collider.enabled = false;
            }
            var echo = GameObject.Find("EchoCorridorPuzzle")?.GetComponent<MementoMori.World.EchoCorridorPuzzle>();
            SetVector2(echo, "returnPosition", new Vector2(28, -27));
            var full = GameObject.Find(puzzleObjects + "/Petala_Cheia")?.GetComponent<MementoMori.Puzzles.GardenPetalPuzzle>();
            SetVector2(full, "fullReflectionPosition", new Vector2(53.9f, -51.6f));

            Move("SceneRoot/Gameplay/Checkpoints/Checkpoint_MoonDomainEntry", 3, -6);
            Move("SceneRoot/Gameplay/Checkpoints/Checkpoint_GardenExit", 45, -52);
            Move("SceneRoot/Gameplay/Checkpoints/Checkpoint_MirrorsExit", 3.5f, -41.5f);
            Move("SceneRoot/Gameplay/Checkpoints/Checkpoint_BeforeSigil", -4.5f, -83);
            Move("SceneRoot/Gameplay/Checkpoints/Checkpoint_FinalPortal", -53, -96);
            Move("SceneRoot/Characters/Player", 3, -5);
            Move("MoonDomainEntry", 3, -5);
            Move("SceneRoot/_World/Fragment", -53, -88);
            Move("SceneRoot/Gameplay/Portals/MoonPortalArt", -53, -99);
            Move("SceneRoot/Gameplay/Doors/MirrorGate", 3.5f, -42.5f);
            Move("SceneRoot/Gameplay/Doors/LunarSeal", -30, -100);
            Move("AndrealphusAlcove", -4, -88);
            ShiftChildrenByPrefix("SceneRoot/Gameplay", "", Vector2.zero); // retains hierarchy references

            foreach (var name in new[] { "FlorMinguante_1", "FlorMinguante_2", "FlorMinguante_3" })
                Shift(name, new Vector2(35, -60));
            foreach (var name in new[] { "GalleryDoor_Nova", "GalleryDoor_Crescente", "GalleryDoor_Cheia",
                         "GalleryDoor_Minguante", "GalleryDoor_Gibosa", "GalleryDoor_Quarto",
                         "GalleryDoor_Eco", "GalleryDoor_SemSímbolo" })
                Shift(name, new Vector2(-22, -65.4f));

            const string props = "V3MapArt/Props";
            ShiftChildrenByPrefix(props, "Art_Petala", new Vector2(35, -60));
            ShiftChildrenByPrefix(props, "Art_Flor", new Vector2(35, -60));
            ShiftChildrenByPrefix(props, "Art_Garden", new Vector2(35, -60));
            ShiftChildrenByPrefix(props, "Art_LabyrinthMirror_", new Vector2(20, -22.4f));
            ShiftChildrenByPrefix(props, "Art_GalleryDoor_", new Vector2(-22, -65.4f));
            ShiftChildrenByPrefix(props, "Art_Sigil", new Vector2(0, -61.8f));
            ShiftChildrenByPrefix(props, "Art_Fragment", new Vector2(-53, -51));
            Move(props + "/Art_FinalPortal", -53, -99);
            Move(props + "/Art_PoeReappearPoint", -50, -97);
            Move(props + "/Art_LunarEntrance", 3, -4);

            foreach (var name in new[] { "MoonGardenArt", "MoonBridge", "MoonTree", "MoonBush", "MoonFlowers", "MoonCrystal" })
                Shift(name, new Vector2(35, -60));
            Move("MoonWater", 57, -52);
            Shift("MoonMirrorArt", new Vector2(20, -22.4f));
            Shift("MoonReflection", new Vector2(20, -22.4f));
            Shift("MoonWaterReflection", new Vector2(20, -22.4f));
            Shift("MoonSigilArt", new Vector2(0, -61.8f));
            Move("MoonMist", 57, -52);
            Move("MoonFogAtmosphere", 57, -52);
            Move("MoonLightAtmosphere", 57, -52);
            Move("ReflectedSky", 57, -52);

            var composition = GameObject.Find("V3MapArt/R4Composition");
            if (composition != null)
                foreach (Transform child in composition.transform)
                {
                    var name = child.name;
                    var delta = name.StartsWith("LunarEntry") || name.StartsWith("LunarLanding") ? new Vector2(3, -37) :
                        name.StartsWith("Garden") ? new Vector2(35, -60) :
                        name.StartsWith("Mirror") || name.StartsWith("BlackMirror") ? new Vector2(20, -22.4f) :
                        name.StartsWith("Cycles") ? new Vector2(-22, -65.4f) :
                        name.StartsWith("Sigil") ? new Vector2(0, -61.8f) :
                        name.StartsWith("Fragment") ? new Vector2(-53, -51) :
                        name.StartsWith("FinalPortal") ? new Vector2(-53, -56) : Vector2.zero;
                    child.position += (Vector3)delta;
                }
            var oldDecoration = GameObject.Find("V3MapArt/R3Decoration");
            if (oldDecoration != null) oldDecoration.SetActive(false); // no puzzle components in this decorative group

            new GameObject("LuizaGameplayPlaced");
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("LUIZA_GAMEPLAY_REPOSITIONED: mirrors=7, echo choices=3 visible + auxiliary, sigil=3, fragment and portal separate; serialized components retained.");
        }

        [MenuItem("Memento Mori/Mapas Lua/03 Montar Lago Lunar")]
        public static void BuildLake()
        {
            EnsureScene();
            var grid = GameObject.Find("LuizaOriginalGeometry/Grid");
            if (grid == null) throw new InvalidOperationException("Importe primeiro a planta original.");
            var water = Layer(grid.transform, "Water_LunarLake", -18);
            var rim = Layer(grid.transform, "LakeRim", -17);
            var blocker = Layer(grid.transform, "LakeCollision", -16);
            var tile = Tile(TilePath + "0_0.asset");
            var edge = Tile(WallPath + "Top.asset");
            var collisionTile = Tile("Assets/Art/Tiles/V3Closure/RoomCollision.asset");
            water.color = new Color(0.13f, 0.54f, 0.98f, 0.92f);
            rim.color = new Color(0.55f, 0.82f, 1f, 1f);
            var waterCount = 0;
            for (var x = 46; x <= 68; x++)
                for (var y = -62; y <= -42; y++)
                {
                    var dx = (x - 57f) / 9f;
                    var dy = (y + 52f) / 6f;
                    var dist = dx * dx + dy * dy;
                    var p = new Vector2Int(x, y);
                    if (dist <= 1f)
                    {
                        Set(water, p, tile);
                        Set(blocker, p, collisionTile);
                        waterCount++;
                    }
                    else if (dist <= 1.29f) Set(rim, p, edge);
                }
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("LUNAR_LAKE_BUILT: " + waterCount + " water tile cells at Luiza eastern garden (center 57,-52), rim and collision cells separate.");
        }

        static Sprite AtlasSprite(int index)
        {
            const string path = "Assets/Art/FinalProduction/Sheets/D2_LunarGarden_Runtime.png";
            var spriteName = "D2_LunarGarden_" + (index / 4) + "_" + (index % 4);
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                if (asset is Sprite sprite && sprite.name == spriteName)
                    return sprite;
            throw new FileNotFoundException("Sprite do atlas aprovado ausente: " + spriteName);
        }

        static GameObject Decorate(GameObject template, Transform parent, string name,
            int atlasIndex, Vector2 position, int order)
        {
            var existing = parent.Find(name);
            var go = existing != null ? existing.gameObject : UnityEngine.Object.Instantiate(template, parent);
            go.name = name;
            go.transform.position = new Vector3(position.x, position.y, 0);
            go.transform.localScale = Vector3.one;
            if (!go.TryGetComponent<SpriteRenderer>(out var renderer))
                throw new InvalidOperationException("Template sem SpriteRenderer: " + template.name);
            renderer.sprite = AtlasSprite(atlasIndex);
            renderer.color = Color.white;
            renderer.sortingOrder = order;
            foreach (var collider in go.GetComponents<Collider2D>())
                collider.enabled = atlasIndex < 4;
            return go;
        }

        [MenuItem("Memento Mori/Mapas Lua/04 Decorar jardim aprovado")]
        public static void DecorateGarden()
        {
            EnsureScene();
            var composition = GameObject.Find("V3MapArt/R4Composition");
            if (composition == null) throw new InvalidOperationException("Composicao R4 ausente.");
            var trees = composition.transform.Find("GardenTreeWest")?.gameObject;
            var shrubs = composition.transform.Find("GardenPlant15,5")?.gameObject;
            if (trees == null || shrubs == null) throw new InvalidOperationException("Templates aprovados de vegetacao ausentes.");
            // Keep the existing trees and their original GUID-backed SpriteRenderers.
            trees.transform.position = new Vector3(49, -43, 0);
            var eastTree = composition.transform.Find("GardenTreeEast");
            if (eastTree != null) eastTree.position = new Vector3(66, -49, 0);

            var treePositions = new[] {
                new Vector2(64, -43), new Vector2(46, -59), new Vector2(49, -64),
                new Vector2(64, -64), new Vector2(51, -45), new Vector2(67, -56)
            };
            for (var i = 0; i < treePositions.Length; i++)
                Decorate(trees, composition.transform, "LuizaGardenTree_" + (i + 1), i % 4,
                    treePositions[i], 18);

            var shrubPositions = new[] {
                new Vector2(47, -46), new Vector2(53, -43), new Vector2(60, -43),
                new Vector2(67, -46), new Vector2(69, -53), new Vector2(67, -61),
                new Vector2(61, -65), new Vector2(54, -65), new Vector2(48, -62),
                new Vector2(45, -54), new Vector2(50, -60), new Vector2(64, -60)
            };
            for (var i = 0; i < shrubPositions.Length; i++)
                Decorate(shrubs, composition.transform, "LuizaGardenFlowers_" + (i + 1),
                    4 + i % 4, shrubPositions[i], 17);

            var crystal = GameObject.Find("V3MapArt/Props/Art_GardenCrystal(6.00, 5.00)");
            if (crystal != null)
            {
                var spots = new[] { new Vector2(47, -50), new Vector2(51, -63),
                    new Vector2(64, -62), new Vector2(68, -55), new Vector2(63, -44),
                    new Vector2(49, -45) };
                for (var i = 0; i < spots.Length; i++)
                    Decorate(crystal, composition.transform, "LuizaGardenCrystal_" + (i + 1),
                        8 + i % 4, spots[i], 16);
            }
            // Architectural accents frame the water without changing floor cells or blockers.
            foreach (var (name, sprite, position) in new[] {
                ("NorthArchWest", 14, new Vector2(50, -43)),
                ("NorthArchEast", 14, new Vector2(64, -43)),
                ("MoonUrnWest", 12, new Vector2(45, -50)),
                ("MoonUrnEast", 12, new Vector2(69, -50)),
                ("LunarFountainWest", 15, new Vector2(47, -60)),
                ("LunarFountainEast", 15, new Vector2(67, -60)),
                ("CrescentMonumentWest", 13, new Vector2(52, -65)),
                ("CrescentMonumentEast", 13, new Vector2(63, -65))
            })
                Decorate(trees, composition.transform, "LuizaGarden_" + name,
                    sprite, position, 15);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("LUNAR_GARDEN_DECORATED: 6 atlas trees, 12 luminous shrubs, 6 crystals, 2 arches, 2 urns, 2 fountains and 2 monuments on the original eastern floor.");
        }

    }
}
