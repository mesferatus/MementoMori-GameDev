using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using MementoMori.Player;
using MementoMori.UI;
using MementoMori.World;
using MementoMori.Dialogue;

namespace MementoMori.EditorTools
{
    // Explicit, repeatable presentation migration. No import callbacks or gameplay reconstruction.
    public static class FinalVisualProduction
    {
        private const string Output = "Assets/Art/FinalProduction";
        private static T[] All<T>() where T : UnityEngine.Object => UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        private static void Guard()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling) throw new InvalidOperationException("Editor must be idle.");
        }
        private static void Save()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
        public static string CreateFonts()
        {
            Guard();
            Directory.CreateDirectory(Output + "/Fonts");
            AssetDatabase.Refresh();
            var report = new StringBuilder();
            foreach (var family in new[] { "Cinzel", "CormorantGaramond", "UnifrakturMaguntia" })
            {
                var path = Output + "/Fonts/" + family + " SDF.asset";
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (!font)
                {
                    var source = Directory.GetFiles("Assets/Art/Fonts/" + family, "*.ttf").Single().Replace('\\', '/');
                    font = TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>(source), 90, 9,
                        UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
                    font.name = family + " SDF";
                    AssetDatabase.CreateAsset(font, path);
                    font.TryAddCharacters(string.Concat(Enumerable.Range(32, 224).Select(c => (char)c)) + "—–…→←◆✦·‘’“”");
                    AssetDatabase.AddObjectToAsset(font.material, font);
                    foreach (var atlas in font.atlasTextures) if (atlas && !AssetDatabase.Contains(atlas)) AssetDatabase.AddObjectToAsset(atlas, font);
                    EditorUtility.SetDirty(font);
                }
                const string portuguese = "áàâãäéêíóôõúüçÁÀÂÃÉÊÍÓÔÕÚÜÇ";
                font.TryAddCharacters(portuguese);
                var missing = new string(portuguese.Where(c => !font.HasCharacter(c)).ToArray());
                report.AppendLine(family + ": Portuguese missing=" + missing);
            }
            AssetDatabase.SaveAssets();
            File.WriteAllText("TestResults/VisualRebuild/font-glyphs.txt", report.ToString());
            return report.ToString();
        }
        private static TMP_FontAsset Font(string family) => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Output + "/Fonts/" + family + " SDF.asset");
        public static void Typography(GameObject root)
        {
            var cinzel = Font("Cinzel");
            var body = Font("CormorantGaramond");
            var gothic = Font("UnifrakturMaguntia");
            if (!cinzel || !body || !gothic) throw new InvalidOperationException("Create and validate fonts first.");
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                var isBody = text.name.Contains("Body") || text.name.Contains("CreditsText") || text.name.Contains("ClosingMessage");
                text.font = isBody ? body : text.name.Contains("Speaker") ? gothic : cinzel;
                text.fontSharedMaterial = text.font.material;
                text.color = new Color(.91f, .85f, .94f, 1);
                text.raycastTarget = false;
                if (isBody) { text.fontSize = 36; text.fontStyle = FontStyles.Bold; text.lineSpacing = 7; }
                else if (text.name.Contains("Speaker")) text.fontSize = 36;
                else if (text.name.Contains("Objective")) { text.fontSize = 27; text.enableAutoSizing = true; text.fontSizeMin = 22; text.fontSizeMax = 27; }
                else if (text.name.Contains("ActionText")) { text.fontSize = 27; text.enableAutoSizing = true; text.fontSizeMin = 22; text.fontSizeMax = 27; }
                EditorUtility.SetDirty(text);
            }
        }
        public static void UpdateUiPrefabs()
        {
            Guard();
            foreach (var path in Directory.GetFiles("Assets/Prefabs/UI", "*.prefab"))
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try { Typography(root); PrefabUtility.SaveAsPrefabAsset(root, path); }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }
        private static Tile Tile(string name, int row, int col)
        {
            var path = Output + "/Tiles/" + name + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (!tile) { tile = ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(tile, path); }
            tile.sprite = AssetDatabase.LoadAllAssetsAtPath(Output + "/Tiles/Q1_RoomTiles32.png").OfType<Sprite>().Single(s => s.name == "Q1_" + row + "_" + col);
            tile.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.None;
            tile.transform = Matrix4x4.identity;
            EditorUtility.SetDirty(tile);
            return tile;
        }
        private static void RoomTiles(Transform foundation)
        {
            var grid = foundation.Find("Grid");
            var ground = grid.Find("Ground Tilemap").GetComponent<Tilemap>();
            var walls = grid.Find("Walls Tilemap").GetComponent<Tilemap>();
            var floor = Enumerable.Range(0, 4).Select(i => Tile("RoomWood" + i, 0, i)).ToArray();
            var top = Tile("RoomStoneFace", 1, 0);
            var bottom = Tile("RoomStoneBottom", 2, 1);
            var left = Tile("RoomStoneLeft", 2, 2);
            var right = Tile("RoomStoneRight", 2, 3);
            var cornerTL = Tile("RoomStoneCornerTL", 3, 0);
            var cornerTR = Tile("RoomStoneCornerTR", 3, 1);
            var cornerBL = Tile("RoomStoneCornerBL", 3, 2);
            var cornerBR = Tile("RoomStoneCornerBR", 3, 3);
            ground.ClearAllTiles(); walls.ClearAllTiles();
            for (var x = -6; x <= 6; x++) for (var y = -8; y <= 8; y++) ground.SetTile(new Vector3Int(x, y), floor[Math.Abs(x * 17 + y * 3) % 4]);
            for (var x = -7; x <= 7; x++) for (var y = -9; y <= 9; y++)
            {
                if (x != -7 && x != 7 && y != -9 && y != 9) continue;
                var p = new Vector3Int(x, y);
                if (y == 9 && (x == 0 || x == 1)) continue; // Two wall cells form the window opening.
                var tile = x == -7 && y == -9 ? cornerBL
                    : x == 7 && y == -9 ? cornerBR
                    : y == 9 ? top : y == -9 ? bottom : x == -7 ? left : right;
                walls.SetTile(p, tile);
                walls.SetTileFlags(p, TileFlags.None); walls.SetTransformMatrix(p, Matrix4x4.identity);
            }
            // These are the two corner cells visible at the camera's upper wall edge.
            walls.SetTile(new Vector3Int(-7, 8), cornerTL);
            walls.SetTile(new Vector3Int(7, 8), cornerTR);
            ground.color = new Color(.78f,.73f,.8f,1); walls.color = Color.white;
            ground.GetComponent<TilemapRenderer>().sortingOrder = -100;
            walls.GetComponent<TilemapRenderer>().sortingOrder = -5;
            ground.RefreshAllTiles(); walls.RefreshAllTiles();
        }
        private static void Place(Transform view, string name, Vector2 center, float opaqueWidth)
        {
            var t = view.Find(name);
            if (!t) throw new InvalidOperationException("Missing retained prop: " + name);
            var renderer = t.GetComponent<SpriteRenderer>();
            var s = renderer.sprite;
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(s.texture)));
            var x0 = texture.width; var x1 = 0; var y0 = texture.height; var y1 = 0;
            for (int y=0;y<texture.height;y++) for(int x=0;x<texture.width;x++) if(texture.GetPixel(x,y).a>.1f) { x0=Math.Min(x0,x);x1=Math.Max(x1,x+1);y0=Math.Min(y0,y);y1=Math.Max(y1,y+1); }
            UnityEngine.Object.DestroyImmediate(texture);
            var scale = opaqueWidth * s.pixelsPerUnit / (x1-x0);
            t.localScale = new Vector3(scale,scale,1);
            var localCenter = new Vector2((x0+x1)*.5f-s.pivot.x,(y0+y1)*.5f-s.pivot.y)/s.pixelsPerUnit;
            t.position = (Vector3)(center-localCenter*scale);
        }
        private static void Solid(Transform view, string name, Vector2 center, Vector2 size)
        {
            var t=view.Find(name); var collider=t.GetComponent<BoxCollider2D>();
            if(!collider) collider=t.gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger=false;collider.enabled=true;
            collider.size=new Vector2(size.x/t.lossyScale.x,size.y/t.lossyScale.y);
            collider.offset=t.InverseTransformPoint(center);
        }
        private static void RoomComposition(Transform foundation)
        {
            var view=foundation.Find("ReferenceComposition");
            // Canonical Quarto composition: the bed is flush with the left inner wall,
            // and the headboard sits below the upper wall as in the reference room.
            Place(view,"Bed",new Vector2(-5.25f,3.9f),3.2f);
            Place(view,"Nightstand",new Vector2(-2.45f,6.25f),1.1f);
            Place(view,"Desk",new Vector2(4.35f,6.1f),3.5f);
            // The window is architectural: wider than one tile and seated in the top wall.
            Place(view,"Window",new Vector2(.5f,8.18f),2f);
            Solid(view,"Bed",new Vector2(-5.25f,3.7f),new Vector2(2.9f,5.9f));
            Solid(view,"Desk",new Vector2(4.35f,6.1f),new Vector2(3.3f,2.7f));
            // Individual furniture has a solid ground footprint; triggers remain on their narrative owners.
            Solid(view,"Nightstand",new Vector2(-2.25f,6.15f),new Vector2(.9f,.85f));
            foreach(var r in view.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if(r.name.Contains("Rug")||r.name.Contains("Circle")||r.name.Contains("Mat"))continue;
                r.sortingLayerName="Default";
                r.sortingOrder=5000-Mathf.RoundToInt(r.bounds.min.y*100f);
            }
            view.Find("Window").GetComponent<SpriteRenderer>().sortingOrder=-4;
        }
        private static void Actor(SpriteRenderer visual, Transform anchor)
        {
            var parentScale = visual.transform.parent ? visual.transform.parent.lossyScale : Vector3.one;
            visual.transform.localScale=new Vector3(1f/parentScale.x,1f/parentScale.y,1f);
            visual.transform.localPosition=Vector3.zero;
            visual.sortingLayerName="Default";
            visual.spriteSortPoint=SpriteSortPoint.Pivot;
            var depth=visual.GetComponent<VisualDepthOrder>();if(!depth)depth=visual.gameObject.AddComponent<VisualDepthOrder>();
            depth.Configure(anchor);
            visual.sortingOrder=5000-Mathf.RoundToInt(anchor.position.y*100f);
        }
        public static void ApplyActorsAndCamera()
        {
            Guard();
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach(var player in All<PlayerController>().Where(p=>p.gameObject.scene==scene))
            {
                var visual=player.transform.Find("RuntimeVisual")?.GetComponent<SpriteRenderer>();if(visual)Actor(visual,player.transform);
            }
            foreach(var poe in All<MementoMori.Poe.PoeFollower>().Where(p=>p.gameObject.scene==scene))
            {
                var visual=poe.transform.Find("V3PoeVisual")?.GetComponent<SpriteRenderer>();if(visual)Actor(visual,poe.transform);
            }
            foreach(var r in All<SpriteRenderer>().Where(r=>r.gameObject.scene==scene&&r.name=="V3AndrealphusVisual"))Actor(r,r.transform.parent);
            foreach(var r in All<SpriteRenderer>().Where(r=>r.gameObject.scene==scene&&r.name=="MelanthaReturned"))
            {r.transform.localScale=Vector3.one;r.sortingOrder=5000-Mathf.RoundToInt(r.transform.position.y*100);}
            var camera=Camera.main;
            if(camera&&scene.name!="MainMenu")
            {
                if(!(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset))throw new InvalidOperationException("Expected URP");
                var pp=camera.GetComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>();
                if(!pp)pp=camera.gameObject.AddComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>();
                pp.assetsPPU=32;pp.refResolutionX=640;pp.refResolutionY=360;
                pp.gridSnapping=UnityEngine.Rendering.Universal.PixelPerfectCamera.GridSnapping.PixelSnapping;
                pp.cropFrame=UnityEngine.Rendering.Universal.PixelPerfectCamera.CropFrame.None;
                camera.orthographicSize=5.625f;camera.allowHDR=false;camera.allowMSAA=false;camera.allowDynamicResolution=false;
                camera.backgroundColor=new Color(.026f,.022f,.035f,1);
                var player=All<PlayerController>().FirstOrDefault(p=>p.gameObject.scene==scene&&p.gameObject.activeInHierarchy);
                if(scene.name=="Quarto"&&player)
                {
                    camera.GetComponent<CameraFollow2D>().Configure(player.transform,new Vector2(.5f,-3.375f),new Vector2(.5f,4.375f));
                    camera.transform.position=new Vector3(.5f,player.transform.position.y,-10);
                }
                else if(scene.name=="FinalBeta") {camera.transform.position=new Vector3(.5f,3,-10);}
            }
            foreach(var root in scene.GetRootGameObjects())Typography(root);
            Save();
        }
        public static void ApplyRoom()
        {
            Guard();
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.name!="Quarto")throw new InvalidOperationException("Open Quarto first");
            var foundation=GameObject.Find("__MementoVisualFoundation").transform;
            // Preserve existing floors and walls per the latest visual scope.
            RoomComposition(foundation);
            var bed=All<BedController>().First(b=>b.gameObject.activeInHierarchy);
            bed.transform.position=new Vector3(-5.25f,1.05f,0);
            var trigger=bed.GetComponent<BoxCollider2D>();trigger.size=new Vector2(2.1f,.6f);trigger.offset=Vector2.zero;
            // Keep the retained window interaction attached to the same architectural opening.
            var window = GameObject.Find("Window");
            if (window)
            {
                window.transform.position = new Vector3(.5f,7.8f,0);
                var windowCollider = window.GetComponent<BoxCollider2D>();
                if (windowCollider) { windowCollider.isTrigger = true; windowCollider.size = new Vector2(3.2f,.7f); windowCollider.offset = Vector2.zero; }
            }
            ApplyActorsAndCamera();
        }

        public static string NormalizeMelanthaWalkClips()
        {
            Guard();
            var report = new StringBuilder();
            foreach (var direction in new[] { "front", "back", "side" })
            {
                var path = "Assets/Art/Animations/Melantha/melantha_walk_" + direction + ".anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (!clip) continue;
                var binding = AnimationUtility.GetObjectReferenceCurveBindings(clip).First(b => b.propertyName == "m_Sprite");
                var source = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                var frames = source.Take(4).ToArray();
                for (var i = 0; i < frames.Length; i++) frames[i].time = i * 0.10f;
                AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.stopTime = 0.40f;
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                EditorUtility.SetDirty(clip);
                report.AppendLine(direction + " frames=" + frames.Length + " rate=10Hz loop=0.40s");
            }
            AssetDatabase.SaveAssets();
            File.WriteAllText("TestResults/VisualRebuild/melantha-walk-audit.txt", report.ToString());
            return report.ToString();
        }
        public static void ApplyReturnRoom()
        {
            Guard();
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="FinalBeta")throw new InvalidOperationException("Open FinalBeta first");
            var foundation=GameObject.Find("QuartoV3Return").transform;
            // FinalBeta follows the same preservation rule as Quarto.
            RoomComposition(foundation);
            ApplyActorsAndCamera();
        }
        public static string Audit()
        {
            var s=UnityEngine.SceneManagement.SceneManager.GetActiveScene();var sb=new StringBuilder();
            sb.AppendLine("Scene: "+s.name);
            foreach(var r in All<SpriteRenderer>().Where(r=>r.gameObject.scene==s&&(r.name=="RuntimeVisual"||r.name=="V3PoeVisual"||r.name=="V3AndrealphusVisual")))
                sb.AppendLine(r.name+" size="+r.bounds.size+" scale="+r.transform.lossyScale+" ppu="+(r.sprite?r.sprite.pixelsPerUnit:0)+" feet="+r.transform.position);
            foreach(var c in All<Collider2D>().Where(c=>c.gameObject.scene==s&&c.enabled&&c.gameObject.activeInHierarchy&&!c.isTrigger))sb.AppendLine("Solid: "+c.name+" bounds="+c.bounds);
            foreach(var t in All<TMP_Text>().Where(t=>t.gameObject.scene==s))if(!t.font)sb.AppendLine("MISSING FONT: "+t.name);
            File.WriteAllText("TestResults/VisualRebuild/"+s.name+"-presentation-audit.txt",sb.ToString());return sb.ToString();
        }
    }
}
