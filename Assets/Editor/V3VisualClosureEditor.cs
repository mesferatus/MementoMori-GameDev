using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using MementoMori.Player;
using MementoMori.Dialogue;
using MementoMori.World;

namespace MementoMori.EditorTools
{
    // Explicit operations only: never rebuild a scene on import or on Play.
    public static class V3VisualClosureEditor
    {
        private const string RootPath = "__MementoVisualFoundation/ReferenceComposition";
        private static T[] All<T>() where T : UnityEngine.Object => UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        private static Transform Composition => GameObject.Find(RootPath).transform;

        private static void Place(string name, float x, float y, float width, int alphaX, int alphaY, int alphaWidth, int alphaHeight)
        {
            var t = Composition.Find(name);
            if (t == null) throw new InvalidOperationException("Missing room prop: " + name);
            Undo.RecordObject(t, "V3 room composition");
            var s = t.GetComponent<SpriteRenderer>().sprite;
            var scale = width * s.pixelsPerUnit / alphaWidth;
            t.localScale = new Vector3(scale, scale, 1);
            var cx = alphaX + alphaWidth * .5f - s.pivot.x;
            var cy = s.rect.height - alphaY - alphaHeight * .5f - s.pivot.y;
            t.position = new Vector3(x - cx * scale / s.pixelsPerUnit, y - cy * scale / s.pixelsPerUnit, 0);
        }

        public static void PolishQuarto()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before applying composition.");
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "Quarto") throw new InvalidOperationException("Open Quarto first.");
            Place("Bed", -4, 4.65f, 3.6f, 19,12,57,116);
            Place("Nightstand", -1.6f, 6.1f, 1.25f, 6,10,35,54);
            Place("Window", .5f, 8, 2.6f, 2,7,60,49);
            Place("Desk", 4.25f, 5.7f, 4.5f, 26,12,76,84);
            Place("Grimoire", 5.65f, .9f, 1.1f, 7,6,33,58);
            Place("RitualRug", .5f, -1.6f, 6.3f, 19,11,122,170);
            Place("BowlMat", -4.65f,-2.5f,1.8f,1,1,30,29);
            Place("PoeBowl", -4.65f,-2.4f,1.2f,5,12,22,20);
            Place("PoeToy", -4.65f,-4.4f,1.25f,3,14,25,34);
            Place("Photograph",5.6f,-3.55f,1.45f,4,9,40,55);
            Place("Altar",-4.65f,-6.65f,1.55f,13,4,37,60);
            Place("Chest",-2.8f,-7.25f,1.3f,4,8,23,24);
            Place("Sideboard",4.8f,-6.8f,3.4f,32,14,64,50);
            Place("Door",.5f,-8.5f,3,1,21,94,43);
            // The official desk already contains its books, ink and chair.
            foreach(var n in new[]{"Chair","OpenBook","Ink"}) Composition.Find(n).gameObject.SetActive(false);
            var positions = new[]{new Vector2(.5f,1.7f),new Vector2(.5f,-4.8f),new Vector2(-2,-1.6f),new Vector2(3,-1.6f)};
            var names = new[]{"CandleNorth","CandleSouth","CandleWest","CandleEast"};
            for(var i=0;i<4;i++) Place(names[i],positions[i].x,positions[i].y,.36f,9,6,14,56);
            var bed=All<BedController>().Single(b=>b.gameObject.activeInHierarchy);
            bed.transform.position=new Vector3(-3.6f,.85f,0);
            bed.transform.localScale=Vector3.one;
            var bedTrigger=bed.GetComponent<BoxCollider2D>(); bedTrigger.size=new Vector2(2.8f,.7f); bedTrigger.offset=Vector2.zero;
            foreach(var d in All<DialogueTrigger>().Where(d=>d.gameObject.activeInHierarchy))
            {
                Vector2? p=d.name switch {"Grimoire"=>new Vector2(5.65f,.9f),"Window"=>new Vector2(.5f,7.45f),"Candles"=>positions[0],"Photo"=>new Vector2(5.6f,-3.55f),"PoeBowl"=>new Vector2(-4.65f,-2.4f),"PoeToy"=>new Vector2(-4.65f,-4.4f),"RitualItem"=>new Vector2(-2.8f,-7.25f),_=>null};
                if(!p.HasValue) continue;
                d.transform.position=p.Value; d.transform.localScale=Vector3.one;
                var c=d.GetComponent<BoxCollider2D>();if(!c)c=d.gameObject.AddComponent<BoxCollider2D>();
                c.enabled=true;c.isTrigger=true;c.offset=Vector2.zero;c.size=new Vector2(.6f,.6f);
            }
            var ritual=All<RitualController>().Single(r=>r.gameObject.activeInHierarchy);
            ritual.transform.position=new Vector3(.5f,-1.6f,0);
            ritual.GetComponent<CircleCollider2D>().radius=.65f;
            ConfigureSolid("Bed",new Vector2(3.3f,6.65f),new Vector2(-4,4.6f));
            ConfigureSolid("Desk",new Vector2(4.1f,2.45f),new Vector2(4.25f,6.6f));
            ConfigureSolid("Sideboard",new Vector2(3.1f,1.25f),new Vector2(4.8f,-7.05f));
            ConfigureSolid("Altar",new Vector2(1.25f,1.3f),new Vector2(-4.65f,-7.05f));
            ConfigureRoomTiles();
            SetupPlayer(new Vector2(-1.25f,3.4f));
            var spawn=GameObject.Find("SPAWN_Quarto_Entrada");if(spawn)spawn.transform.position=new Vector3(-1.25f,3.4f,0);
            var camera=Camera.main;camera.orthographicSize=10;camera.transform.position=new Vector3(.5f,.5f,-10);
            camera.backgroundColor=new Color(.025f,.021f,.034f);camera.allowHDR=false;camera.allowMSAA=false;
            camera.GetComponent<CameraFollow2D>().Configure(All<PlayerController>().First().transform,new Vector2(.5f,.5f),new Vector2(.5f,.5f));
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }

        private static void ConfigureSolid(string name,Vector2 size,Vector2 center)
        {
            var t=Composition.Find(name);var c=t.GetComponent<BoxCollider2D>();if(!c)c=t.gameObject.AddComponent<BoxCollider2D>();
            c.enabled=true;c.isTrigger=false;c.size=new Vector2(size.x/t.lossyScale.x,size.y/t.lossyScale.y);
            c.offset=t.InverseTransformPoint(center);
        }

        private static Tile CroppedTile(string name,string texturePath,Rect rect,Vector3 scale)
        {
            const string dir="Assets/Art/Tiles/V3Closure";
            if(!AssetDatabase.IsValidFolder(dir))AssetDatabase.CreateFolder("Assets/Art/Tiles","V3Closure");
            var path=dir+"/"+name+".asset";var tile=AssetDatabase.LoadAssetAtPath<Tile>(path);
            if(tile) return tile;
            tile=ScriptableObject.CreateInstance<Tile>();tile.name=name;
            var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            var sprite=Sprite.Create(tex,rect,new Vector2(.5f,.5f),32,0,SpriteMeshType.FullRect);
            sprite.name=name+"_Cropped";tile.sprite=sprite;tile.colliderType=Tile.ColliderType.None;
            tile.transform=Matrix4x4.Scale(scale);
            AssetDatabase.CreateAsset(tile,path);AssetDatabase.AddObjectToAsset(sprite,tile);EditorUtility.SetDirty(tile);return tile;
        }

        private static void ConfigureRoomTiles()
        {
            var root=GameObject.Find("__MementoVisualFoundation/Grid");
            var ground=root.transform.Find("Ground Tilemap").GetComponent<Tilemap>();
            var walls=root.transform.Find("Walls Tilemap").GetComponent<Tilemap>();
            var floor=CroppedTile("RoomFloor","Assets/Art/Sprites/Quarto/01_Tiles/01_tile_chao_madeira.png",new Rect(3,4,24,24),new Vector3(32f/24,32f/24,1));
            var wall=CroppedTile("RoomStone","Assets/Art/Sprites/Quarto/01_Tiles/02_segmento_parede_gotica.png",new Rect(9,6,15,6),new Vector3(32f/15,32f/15,1));
            ground.ClearAllTiles();walls.ClearAllTiles();
            for(var x=-6;x<=6;x++) for(var y=-8;y<=8;y++) ground.SetTile(new Vector3Int(x,y),floor);
            ground.color=new Color(.72f,.67f,.73f,1);
            // Art and collision use separate Tilemaps: scaling stone never scales physics.
            var collisionObject=root.transform.Find("V3WallCollision")?.gameObject??new GameObject("V3WallCollision",typeof(Tilemap),typeof(TilemapRenderer),typeof(TilemapCollider2D));
            collisionObject.transform.SetParent(root.transform,false);
            collisionObject.GetComponent<TilemapRenderer>().enabled=false;
            var collision=collisionObject.GetComponent<Tilemap>();collision.ClearAllTiles();
            const string cp="Assets/Art/Tiles/V3Closure/RoomCollision.asset";
            var ct=AssetDatabase.LoadAssetAtPath<Tile>(cp);
            if(!ct){ct=ScriptableObject.CreateInstance<Tile>();ct.colliderType=Tile.ColliderType.Grid;AssetDatabase.CreateAsset(ct,cp);}
            var oldCollider=walls.GetComponent<TilemapCollider2D>();if(oldCollider)oldCollider.enabled=false;
            for(var x=-7;x<=7;x++) for(var y=-9;y<=9;y++)
            {
                if(x!=-7&&x!=7&&y!=-9&&y!=9) continue;
                var p=new Vector3Int(x,y);collision.SetTile(p,ct);walls.SetTile(p,wall);
                walls.SetTileFlags(p,TileFlags.None);
                // Side stones are turned, keeping their original aspect ratio.
                var side=x==-7||x==7;
                walls.SetTransformMatrix(p,Matrix4x4.TRS(Vector3.zero,Quaternion.Euler(0,0,side?90:0),new Vector3(32f/15,32f/15*2,1)));
            }
            walls.RefreshAllTiles();collision.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
        }

        public static void SetupPlayer(Vector2? position=null)
        {
            foreach(var player in All<PlayerController>().Where(p=>p.gameObject.activeInHierarchy))
            {
                if(position.HasValue)player.transform.position=position.Value;
                var t=player.transform.Find("RuntimeVisual")??new GameObject("RuntimeVisual").transform;t.SetParent(player.transform,false);
                var sr=t.GetComponent<SpriteRenderer>();if(!sr)sr=t.gameObject.AddComponent<SpriteRenderer>();
                sr.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Characters/Melantha/Idle/melantha_idle_front_01.png");
                sr.enabled=true;sr.color=Color.white;sr.sortingOrder=50;sr.sortingLayerName="Default";
                t.localPosition=new Vector3(0,.1f,0);t.localScale=new Vector3(3.125f,3.125f,1);
                var animator=t.GetComponent<Animator>();if(!animator)animator=t.gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Animations/Melantha/Melantha.controller");
                var so=new SerializedObject(player);so.FindProperty("animator").objectReferenceValue=animator;so.ApplyModifiedPropertiesWithoutUndo();
                var collider=player.GetComponent<CircleCollider2D>();if(collider){collider.radius=.27f;collider.offset=Vector2.zero;}
            }
        }

        public static void RepairGameplayUi()
        {
            var root=GameObject.Find("GameplayUI"); if(!root)return;
            foreach(var c in root.GetComponentsInChildren<Canvas>(true))
            {
                if(c.gameObject==root)continue;
                var rt=(RectTransform)c.transform;
                rt.localScale=Vector3.one;rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;
                rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;rt.anchoredPosition=Vector2.zero;
                var scaler=c.GetComponent<UnityEngine.UI.CanvasScaler>();if(scaler)scaler.enabled=false;
            }
            foreach(var text in root.GetComponentsInChildren<TMPro.TMP_Text>(true))
                text.overflowMode=TMPro.TextOverflowModes.Overflow;
        }

        public static void BindDialoguePresentation()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first.");
            var portraits=new[]{
                CroppedTile("PortraitMelantha","Assets/Art/Characters/Melantha/Idle/melantha_idle_front_01.png",new Rect(20,35,25,26),Vector3.one).sprite,
                CroppedTile("PortraitPoe","Assets/Art/Characters/Poe/Idle/poe_idle_front_01.png",new Rect(9,18,31,28),Vector3.one).sprite,
                CroppedTile("PortraitAndrealphus","Assets/Art/Characters/Andrealphus/Idle/andrealphus_idle_float_01.png",new Rect(30,81,34,36),Vector3.one).sprite
            };
            foreach(var sceneName in new[]{"Quarto","Labirinto","DominioLua","FinalBeta"})
            {
                var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sceneName+".unity");
                RepairGameplayUi();
                foreach(var pause in All<MementoMori.UI.PauseMenuController>())
                {
                    var dimmer=pause.transform.Find("Dimmer");
                    if(dimmer){var settings=new SerializedObject(pause);settings.FindProperty("dimmer").objectReferenceValue=dimmer.gameObject;settings.ApplyModifiedPropertiesWithoutUndo();dimmer.gameObject.SetActive(false);}
                }
                foreach(var dialogue in All<DialogueManager>())
                {
                    var so=new SerializedObject(dialogue);
                    var fields=new[]{"melanthaPortrait","poePortrait","andrealphusPortrait"};
                    for(int i=0;i<fields.Length;i++)so.FindProperty(fields[i]).objectReferenceValue=portraits[i];
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                foreach(var text in All<TMPro.TMP_Text>())
                {
                    if(text.name=="ActionText_TMP") {text.rectTransform.anchoredPosition=new Vector2(245,0);text.rectTransform.sizeDelta=new Vector2(230,54);text.fontSize=30;var bg=(RectTransform)text.transform.parent;bg.sizeDelta=new Vector2(600,270);var image=bg.GetComponent<Image>();if(image){image.type=Image.Type.Simple;image.preserveAspect=true;}}
                    if(text.name=="ObjectiveText_TMP")text.fontSize=30;
                }
                if(sceneName=="Labirinto") foreach(var passage in All<EchoPassageChoice>())
                {
                    Prop(passage.transform,"EchoVoiceArt",FindArt("Assets/Art/Sprites/Labirinto","53_circle_generic_01.png"),passage.transform.position,1.15f,4);
                }
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        }

        private static Sprite FindArt(string prefix,string file)
        {
            var paths=AssetDatabase.FindAssets(System.IO.Path.GetFileNameWithoutExtension(file),new[]{prefix})
                .Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.EndsWith("/"+file,StringComparison.Ordinal)).ToArray();
            if(paths.Length!=1)throw new InvalidOperationException("Expected one asset: "+file);
            return AssetDatabase.LoadAssetAtPath<Sprite>(paths[0]);
        }

        private static Tile MapTile(string name,Sprite sprite,bool solid=false)
        {
            var path="Assets/Art/Tiles/V3Closure/"+name+".asset";
            var t=AssetDatabase.LoadAssetAtPath<Tile>(path);
            if(!t){t=ScriptableObject.CreateInstance<Tile>();AssetDatabase.CreateAsset(t,path);}
            t.sprite=sprite;t.colliderType=solid?Tile.ColliderType.Grid:Tile.ColliderType.None;
            t.transform=Matrix4x4.identity;EditorUtility.SetDirty(t);return t;
        }

        private static SpriteRenderer Prop(Transform root,string name,Sprite sprite,Vector2 position,float width,int order=10)
        {
            var visualName=root.name=="Props"?"Art_"+name:name;
            var t=root.Find(visualName);
            if(!t)t=root.Find(name);
            if(!t){t=new GameObject(visualName).transform;t.SetParent(root,false);}
            t.name=visualName;
            t.position=position;
            var r=t.GetComponent<SpriteRenderer>();if(!r)r=t.gameObject.AddComponent<SpriteRenderer>();
            r.sprite=sprite;r.sortingOrder=order;r.sortingLayerName="Default";r.color=Color.white;r.enabled=true;
            t.localScale=Vector3.one*(width/sprite.bounds.size.x);return r;
        }

        public static void RestoreMapArt()
        {
            var scene=EditorSceneManager.GetActiveScene();var moon=scene.name=="DominioLua";
            if(!moon&&scene.name!="Labirinto")throw new InvalidOperationException("Open Labirinto or DominioLua.");
            var prefix="Assets/Art/Sprites/"+(moon?"DominioDaLua":"Labirinto");
            var ground=All<Tilemap>().First(t=>t.name=="Ground"&&t.GetUsedTilesCount()>0);
            var cells=new System.Collections.Generic.HashSet<Vector3Int>();
            foreach(var p in ground.cellBounds.allPositionsWithin)if(ground.HasTile(p))cells.Add(p);
            // Connect the two lower side Arcana chambers to the central passage.
            if(!moon)for(int x=-10;x<=10;x++)for(int y=-21;y<=-20;y++)cells.Add(new Vector3Int(x,y));
            foreach(var r in All<Renderer>())if(r is TilemapRenderer||r.name.StartsWith("Label_")||r.name=="RuntimeBackground")r.enabled=false;
            foreach(var c in All<TilemapCollider2D>())c.enabled=false;
            foreach(var c in All<CompositeCollider2D>())c.enabled=false;
            var root=GameObject.Find("V3MapArt");if(!root)root=new GameObject("V3MapArt",typeof(Grid));
            Tilemap Layer(string name,int order,bool solid)
            {
                var tr=root.transform.Find(name);var go=tr?tr.gameObject:new GameObject(name,typeof(Tilemap),typeof(TilemapRenderer));go.transform.SetParent(root.transform,false);
                var t=go.GetComponent<Tilemap>();t.ClearAllTiles();var r=go.GetComponent<TilemapRenderer>();r.enabled=true;r.sortingOrder=order;
                if(solid){var c=go.GetComponent<TilemapCollider2D>();if(!c)c=go.AddComponent<TilemapCollider2D>();c.enabled=true;}
                return t;
            }
            var floor=Layer("Floor",-30,false);var walls=Layer("Walls",-20,false);var collision=Layer("Collision",-50,true);collision.GetComponent<TilemapRenderer>().enabled=false;
            var f=MapTile(scene.name+"Floor",FindArt(prefix,moon?"01_floor_moon_clean_01.png":"01_floor_clean_01.png"));
            f=CroppedTile("LabFloorFilled",AssetDatabase.GetAssetPath(FindArt("Assets/Art/Sprites/Labirinto","01_floor_clean_01.png")),new Rect(6,4,24,23),new Vector3(32f/24,32f/23,1));
            var top=MapTile(scene.name+"WallTop",FindArt(prefix,moon?"07_wall_moon_top.png":"13_wall_top.png"));
            var bottom=MapTile(scene.name+"WallBottom",FindArt(prefix,moon?"08_wall_moon_bottom.png":"14_wall_bottom.png"));
            var left=MapTile(scene.name+"WallLeft",FindArt(prefix,moon?"09_wall_moon_left.png":"15_wall_left.png"));
            var right=MapTile(scene.name+"WallRight",FindArt(prefix,moon?"10_wall_moon_right.png":"16_wall_right.png"));
            if(!moon)
            {
                top=CroppedTile("LabTopFilled",AssetDatabase.GetAssetPath(top.sprite),new Rect(1,10,31,12),new Vector3(32f/31,2,1));
                bottom=CroppedTile("LabBottomFilled",AssetDatabase.GetAssetPath(bottom.sprite),new Rect(0,9,32,13),new Vector3(1,2,1));
                left=CroppedTile("LabLeftFilled",AssetDatabase.GetAssetPath(left.sprite),new Rect(12,0,11,32),new Vector3(2,1,1));
                right=CroppedTile("LabRightFilled",AssetDatabase.GetAssetPath(right.sprite),new Rect(9,1,11,30),new Vector3(2,32f/30,1));
            }
            var ct=AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Tiles/V3Closure/RoomCollision.asset");
            foreach(var p in cells)floor.SetTile(p,f);
            var border=new System.Collections.Generic.HashSet<Vector3Int>();
            foreach(var p in cells)for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++){var q=p+new Vector3Int(x,y);if(!cells.Contains(q))border.Add(q);}
            foreach(var p in border){var tile=cells.Contains(p+Vector3Int.down)?top:cells.Contains(p+Vector3Int.up)?bottom:cells.Contains(p+Vector3Int.right)?left:right;walls.SetTile(p,tile);collision.SetTile(p,ct);}
            floor.color=moon?new Color(.64f,.69f,.86f):new Color(.7f,.64f,.78f);
            var deco=root.transform.Find("Props");if(!deco){deco=new GameObject("Props").transform;deco.SetParent(root.transform,false);}
            if(!moon)
            {
                Prop(deco,"EntrySigil",FindArt(prefix,"53_circle_generic_01.png"),new Vector2(0,12),5,-10);
                Prop(deco,"CoreSigil",FindArt(prefix,"55_circle_core_labyrinth.png"),new Vector2(0,-4),7,-10);
                Prop(deco,"EntryGate",FindArt(prefix,"35_locked_gate.png"),new Vector2(0,17),4);
                var well=All<VoiceWellController>().First();var wr=Prop(deco,"VoiceWell",FindArt(prefix,"48_well_stone.png"),well.transform.position,4);
                var stages=new GameObject[4];for(int i=0;i<4;i++){stages[i]=Prop(deco,"WellEnergy"+i,FindArt(prefix,"09_well_energy_0"+(i+1)+".png"),well.transform.position,3.5f,11).gameObject;stages[i].SetActive(false);}well.ConfigureVisualStages(stages);
                var fd=All<FalseDoorController>().First();Prop(deco,"FalseDoor",FindArt(prefix,"35_locked_gate.png"),fd.transform.position,3);
                Prop(deco,"EmptyCircle",FindArt(prefix,"54_circle_generic_02.png"),new Vector2(12,4),4,-10);
                var portals=All<Portal>();
                foreach(var portal in portals.Where(p=>p.name.StartsWith("DomainPortal_")))
                {
                    var file=portal.name switch {"DomainPortal_Moon"=>"04_moon_portal_sealed.png","DomainPortal_Tower"=>"14_portal_seal_tower.png","DomainPortal_Hanged"=>"15_portal_seal_hangedman.png","DomainPortal_Hermit"=>"16_portal_seal_hermit.png","DomainPortal_Death"=>"17_portal_seal_death.png","DomainPortal_Devil"=>"13_portal_seal_devil.png",_=>"18_portal_seal_judgement.png"};
                    var r=Prop(deco,portal.name,FindArt(prefix,file),portal.transform.position,3);
                    if(portal.name=="DomainPortal_Moon")portal.ConfigureVisualStates(r,FindArt(prefix,file),FindArt(prefix,"05_moon_portal_active_01.png"));
                }
                // Preserve the working MoonPortal binding but place it at the canonical lunar access.
                var linked=portals.FirstOrDefault(p=>p.name=="MoonPortal");var lunar=portals.FirstOrDefault(p=>p.name=="DomainPortal_Moon");
                if(linked&&lunar){linked.transform.position=lunar.transform.position;lunar.gameObject.SetActive(false);var r=deco.Find("Art_DomainPortal_Moon").GetComponent<SpriteRenderer>();linked.ConfigureVisualStates(r,FindArt(prefix,"04_moon_portal_sealed.png"),FindArt(prefix,"05_moon_portal_active_01.png"));}
                foreach(var point in new[]{new Vector2(-4,15),new Vector2(4,15),new Vector2(-14,4),new Vector2(-10,4),new Vector2(-4,-7),new Vector2(4,-7),new Vector2(10,5),new Vector2(14,5)})
                    Prop(deco,"Brazier_"+point,FindArt(prefix,"44_brazier_base.png"),point,1.2f);
            }
            else
            {
                foreach(var c in All<FalseDoorController>())c.gameObject.SetActive(false);
                foreach(var c in All<VoiceWellController>())c.gameObject.SetActive(false);
                foreach(var c in All<EchoCorridorPuzzle>())c.gameObject.SetActive(false);
                foreach(var c in All<EchoPassageChoice>())c.gameObject.SetActive(false);
                var puzzles=All<MementoMori.Puzzles.GardenPetalPuzzle>();
                foreach(var puzzle in puzzles)
                {
                    var file=puzzle.Petal switch{MementoMori.Puzzles.MoonPetal.Crescente=>"07_flower_crescent_closed.png",MementoMori.Puzzles.MoonPetal.Cheia=>"08_flower_full_reflect.png",_=>"09_flowers_waning_off.png"};
                    Prop(deco,puzzle.name,FindArt(prefix,file),puzzle.transform.position,1.15f);
                }
                foreach(var node in All<MementoMori.Interaction.WaningFlowerNode>())Prop(deco,node.name,FindArt(prefix,"09_flowers_waning_off.png"),node.transform.position,.5f);
                var owner=All<MementoMori.Puzzles.PuzzleMirror>().First();
                foreach(var mirror in All<MementoMori.Puzzles.MirrorSymbol>())
                {
                    var file=mirror.SymbolId switch{"Present"=>"02_reflection_present.png","Delayed"=>"03_reflection_delayed.png","Ahead"=>"04_reflection_advanced.png","Absent"=>"05_reflection_no_poe.png","Double"=>"06_reflection_two_poes.png","Room"=>"07_reflection_room.png",_=>"08_black_mirror_off.png"};
                    var r=Prop(deco,mirror.name,FindArt(prefix,file),mirror.transform.position,1.2f);
                    mirror.Configure(mirror.SymbolId,owner,r);mirror.ConfigureVisualStates(r.sprite,r.sprite);
                    if(mirror.SymbolId=="Black"){var sequence=mirror.GetComponent<MementoMori.Puzzles.BlackMirrorSequenceController>();if(!sequence)sequence=mirror.gameObject.AddComponent<MementoMori.Puzzles.BlackMirrorSequenceController>();sequence.Configure(r);}
                }
                Prop(deco,"LunarEntrance",FindArt(prefix,"17_lunar_doorframe.png"),new Vector2(0,19),4);
                Prop(deco,"SigilBase",FindArt(prefix,"29_sigil_base_moon.png"),new Vector2(0,-10.5f),7,-10);
                foreach(var ring in All<MementoMori.World.SigilRingInteractable>())
                    Prop(deco,ring.name,FindArt(prefix,ring.name.Contains("Fases")?"33_sigil_symbol_moon.png":ring.name.Contains("Mem")?"34_sigil_symbol_grimoire.png":"35_sigil_symbol_sustain.png"),ring.transform.position,1);
                var fragment=All<FragmentCollectible>().First();Prop(deco,"FragmentPedestal",FindArt(prefix,"40_fragment_pedestal.png"),fragment.transform.position,1.4f);
                Prop(deco,"FragmentEvidence",FindArt(prefix,"39_fragment_collectible.png"),(Vector2)fragment.transform.position+new Vector2(0,.65f),.75f,15);
                var point=deco.Find("PoeReappearPoint");if(!point){point=new GameObject("PoeReappearPoint").transform;point.SetParent(deco,false);}point.position=new Vector3(1,-20,0);
                var fs=new SerializedObject(fragment);fs.FindProperty("poeReappearPoint").objectReferenceValue=point;fs.FindProperty("poe").objectReferenceValue=All<MementoMori.Poe.PoeFollower>().First();fs.ApplyModifiedPropertiesWithoutUndo();
                Prop(deco,"FinalPortal",FindArt(prefix,"18_portal_frame_final.png"),new Vector2(0,-21.5f),3.4f);
                foreach(var pos in new[]{new Vector2(6,5),new Vector2(15,5),new Vector2(6,-.5f),new Vector2(15,-.5f)})Prop(deco,"GardenCrystal"+pos,FindArt(prefix,"44_crystal_cluster_large.png"),pos,1.8f);
                foreach(var pos in new[]{new Vector2(-4,17),new Vector2(4,17),new Vector2(-4,-9),new Vector2(4,-9),new Vector2(-4,-15),new Vector2(4,-15)})Prop(deco,"LunarBrazier"+pos,FindArt(prefix,"41_brazier_moon_base.png"),pos,1.3f);
                var npc=GameObject.Find("AndrealphusAlcove");if(!npc){npc=new GameObject("AndrealphusAlcove");npc.transform.position=new Vector3(-3,-7.5f,0);npc.AddComponent<DialogueTrigger>();}
                foreach(var d in All<DialogueTrigger>().Where(d=>d.name.StartsWith("GalleryDoor_")))
                    Prop(deco,d.name,FindArt(prefix,d.name.Contains("Cheia")?"13_symbol_full_moon.png":d.name.Contains("Minguante")?"14_symbol_waning_moon.png":d.name.Contains("Crescente")?"12_symbol_crescent_moon.png":"11_symbol_new_moon.png"),d.transform.position,1.15f);
            }
            SetupPlayer();SetupCompanions();RepairGameplayUi();
            foreach(var r in All<SpriteRenderer>())if(!r.sprite || !AssetDatabase.GetAssetPath(r.sprite).StartsWith("Assets/Art/"))r.enabled=false;
            foreach(var r in All<MeshRenderer>())r.enabled=false;
            var cam=Camera.main;cam.orthographicSize=6.2f;cam.backgroundColor=new Color(.025f,.023f,.035f);cam.allowHDR=false;cam.allowMSAA=false;
            var player=All<PlayerController>().First(p=>p.gameObject.activeInHierarchy);cam.transform.position=player.transform.position+new Vector3(0,0,-10);
            cam.GetComponent<CameraFollow2D>().Configure(player.transform,new Vector2(ground.cellBounds.xMin+3,ground.cellBounds.yMin+4),new Vector2(ground.cellBounds.xMax-3,ground.cellBounds.yMax-4));
            collision.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }

        public static void SetupCompanions()
        {
            foreach(var p in All<MementoMori.Poe.PoeFollower>())
            {
                var t=p.transform.Find("V3PoeVisual");if(!t){t=new GameObject("V3PoeVisual").transform;t.SetParent(p.transform,false);}
                var r=Prop(p.transform,"V3PoeVisual",FindArt("Assets/Art/Characters/Poe","poe_idle_front_01.png"),p.transform.position,1);
                r.sortingOrder=51;t.localPosition=Vector3.zero;
                var a=t.GetComponent<Animator>();if(!a)a=t.gameObject.AddComponent<Animator>();a.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Animations/Poe/Poe.controller");
            }
            var npc=All<DialogueTrigger>().FirstOrDefault(d=>d.name=="AndrealphusAlcove");
            if(npc)
            {
                var r=Prop(npc.transform,"V3AndrealphusVisual",FindArt("Assets/Art/Characters/Andrealphus","andrealphus_idle_float_01.png"),npc.transform.position,2);
                r.sortingOrder=50;var a=r.GetComponent<Animator>();if(!a)a=r.gameObject.AddComponent<Animator>();a.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Animations/Andrealphus/Andrealphus.controller");
                if(!r.GetComponent<MementoMori.Narrative.AndrealphusAnimation>())r.gameObject.AddComponent<MementoMori.Narrative.AndrealphusAnimation>();
            }
        }

        public static void FinishRoomAndEpilogue()
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Quarto.unity");
            RepairGameplayUi();SetupPlayer();
            var composition=Composition;
            var host=composition.gameObject.GetComponent<RoomRitualVisual>();if(!host)host=composition.gameObject.AddComponent<RoomRitualVisual>();
            host.Configure(composition.Find("RitualRug").GetComponent<SpriteRenderer>(),new[]{"CandleNorth","CandleSouth","CandleWest","CandleEast"}.Select(n=>composition.Find(n).GetComponent<SpriteRenderer>()).ToArray(),FindArt("Assets/Art/Sprites/Quarto","06_candle_unlit.png"),FindArt("Assets/Art/Sprites/Quarto","04_vela_individual.png"),false);
            EditorSceneManager.SaveScene(scene);
            if(!AssetDatabase.IsValidFolder("Assets/Prefabs/Environment"))AssetDatabase.CreateFolder("Assets/Prefabs","Environment");
            var prefab=PrefabUtility.SaveAsPrefabAsset(GameObject.Find("__MementoVisualFoundation"),"Assets/Prefabs/Environment/QuartoV3Composition.prefab");
            var dialoguePrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/DialogueUI.prefab");
            scene=EditorSceneManager.OpenScene("Assets/Scenes/FinalBeta.unity");
            EditorSceneManager.SaveScene(scene,"Temp/V3Closure/FinalBeta-before.unity",true);
            foreach(var r in All<Renderer>())r.enabled=false;
            var room=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);room.name="QuartoV3Return";
            var view=room.transform.Find("ReferenceComposition");var visual=view.GetComponent<RoomRitualVisual>();
            visual.Configure(view.Find("RitualRug").GetComponent<SpriteRenderer>(),new[]{"CandleNorth","CandleSouth","CandleWest","CandleEast"}.Select(n=>view.Find(n).GetComponent<SpriteRenderer>()).ToArray(),FindArt("Assets/Art/Sprites/Quarto","06_candle_unlit.png"),FindArt("Assets/Art/Sprites/Quarto","04_vela_individual.png"),true);
            var melantha=Prop(view,"MelanthaReturned",FindArt("Assets/Art/Characters/Melantha","melantha_idle_front_01.png"),new Vector2(-1.25f,3.4f),1,50);
            melantha.transform.localScale=new Vector3(3.125f,3.125f,1);
            var anim=melantha.gameObject.AddComponent<Animator>();anim.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Animations/Melantha/Melantha.controller");
            Prop(view,"PhysicalFragment",FindArt("Assets/Art/Sprites/DominioDaLua","39_fragment_collectible.png"),new Vector2(-1.1f,2.4f),.5f,51);
            var camera=Camera.main;camera.transform.position=new Vector3(.5f,.5f,-10);camera.orthographicSize=10;camera.backgroundColor=new Color(.025f,.021f,.034f);
            var follow=camera.GetComponent<CameraFollow2D>();if(follow)follow.enabled=false;
            if(dialoguePrefab&&!All<DialogueManager>().Any()){var ui=(GameObject)PrefabUtility.InstantiatePrefab(dialoguePrefab,scene);ui.transform.localScale=Vector3.one;}
            var controller=All<MementoMori.UI.FinalBetaController>().First();var so=new SerializedObject(controller);
            var card=GameObject.Find("FinalBetaUI");so.FindProperty("endCardRoot").objectReferenceValue=card;
            so.FindProperty("circleActive").objectReferenceValue=null;so.FindProperty("circleOff").objectReferenceValue=null;so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
    }
}
