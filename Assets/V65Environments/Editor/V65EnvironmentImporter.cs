using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using MementoMori.V65;
using MementoMori.Player;
using MementoMori.Poe;
using MementoMori.World;
using MementoMori.Puzzles;
using MementoMori.Regions;
using Object=UnityEngine.Object;

public static partial class V65EnvironmentImporter
{
 const string Root="Assets/V65Environments";
 const string External="Tools/EnvironmentMaintenance";
 static JObject plan;
 static GameObject world;
 static Dictionary<string,SpriteRenderer> visuals=new();
 static Dictionary<string,Material> materials=new();
 static Dictionary<string,Sprite[]> flameCache=new();
 static float F(JToken t)=>t==null?0:(float)t;
 static Vector2 Point(float x,float y)=>new Vector2(x/64,-y/64);
 static Vector2 Point(JToken t)=>Point(F(t[0]),F(t[1]));
 static void Guard(){Directory.CreateDirectory("TestResults/EnvironmentAudit");if(!Directory.Exists(Path.Combine(Application.dataPath,"V65Environments")))throw new Exception("Wrong project");if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode");}
 static JObject Read(string worldName)=>JObject.Parse(File.ReadAllText(External+"/Bundle/"+worldName+".json"));
 public static string PrepareAssets(){
  Guard();Directory.CreateDirectory(Root+"/Textures");Directory.CreateDirectory(Root+"/Generated");
  int n=0;foreach(var name in new[]{"Labirinto","DominioLua"}){
   var p=Read(name);foreach(var a in p["assets"]){var dst=Root+"/Textures/"+(string)a["id"]+".png";if(!File.Exists(dst)){File.Copy((string)a["source"],dst);n++;}}
   foreach(var c in p["corridor_chunks"]){var dst=Root+"/Textures/"+Path.GetFileName((string)c["source"]);if(!File.Exists(dst)){File.Copy((string)c["source"],dst);n++;}}
  }
  AssetDatabase.Refresh();return "Copied "+n+" approved textures";
 }
 public static string ConfigureTextures(int start,int count){
  Guard();var paths=Directory.GetFiles(Root+"/Textures","*.png").OrderBy(s=>s).ToArray();int n=0;
  for(int i=start;i<Math.Min(paths.Length,start+count);i++){
   var imp=(TextureImporter)AssetImporter.GetAtPath(paths[i]);if(imp==null)continue;
   imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.spritePixelsPerUnit=64;
   imp.spritePivot=Vector2.zero;var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.BottomLeft;settings.spriteMeshType=SpriteMeshType.FullRect;imp.SetTextureSettings(settings);
   imp.filterMode=FilterMode.Point;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;imp.isReadable=true;imp.npotScale=TextureImporterNPOTScale.None;imp.maxTextureSize=8192;imp.SaveAndReimport();n++;
  }return n+" configured / total "+paths.Length;
 }
 static Material Mat(string key,float mode,float seed=0){
  if(materials.TryGetValue(key,out var cached))return cached;
  var path=Root+"/Generated/"+key+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(!mat){mat=new Material(Shader.Find("Memento/V65/Environment"));AssetDatabase.CreateAsset(mat,path);}mat.SetFloat("_Mode",mode);mat.SetFloat("_Seed",seed);mat.SetFloat("_Strength",mode==1?.08f:.045f);materials[key]=mat;return mat;
 }
 static Material Lit(){var path=Root+"/Generated/Lit.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));AssetDatabase.CreateAsset(m,path);}return m;}
 static GameObject Child(string name,Transform parent){var g=new GameObject(name);g.transform.SetParent(parent,false);return g;}
 static Sprite AssetSprite(string id)=>AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Textures/"+id+".png");
 static SpriteRenderer Render(string name,Sprite sprite,float x,float y,float w,float h,Transform parent,int order,Material mat){
  var go=Child(name,parent);go.transform.position=Point(x,y+h);var r=go.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=mat;r.sortingOrder=order;
  if(sprite)go.transform.localScale=new Vector3(w/sprite.rect.width,h/sprite.rect.height,1);return r;
 }
 static int Depth(float y)=>Mathf.Clamp(5000+Mathf.RoundToInt(y*100/64),-32000,32000);
 static Tilemap Map(string name,Transform parent,float cell,Vector2 pos,int order){var grid=Child(name+"_Grid",parent);grid.transform.position=pos;grid.AddComponent<Grid>().cellSize=new Vector3(cell,cell,1);var g=Child(name,grid.transform);var map=g.AddComponent<Tilemap>();var r=g.AddComponent<TilemapRenderer>();r.sortingOrder=order;r.sharedMaterial=Lit();return map;}
 static void Layer(JToken e,Transform parent){
  string id=(string)e["id"],kind=(string)e["kind"],asset=(string)e["asset"];
  var sprite=AssetSprite(asset);var tex=sprite.texture;var r=e["rect"];float x=F(r[0]),y=F(r[1]),w=F(r[2]),h=F(r[3]);
  if(kind=="agua"){
   bool abyss=plan["regions"].Any(q=>(string)q["id"]==(string)e["region"]&&(bool)q["void"]);
   Render(id,sprite,x,y,w,h,parent,-22800,Mat(abyss?"EnforcadoPurpleVoid":"LunarWater",abyss?4:3));return;
  }
  const int size=128;var colors=tex.GetPixels32();var gates=e["gate_cutouts"] as JArray;
  // Tilemap rows permit foot-based ordering of front walls while keeping modules editable.
  for(int py=0;py<tex.height;py+=size){
   int hh=Math.Min(size,tex.height-py);bool wall=kind=="paredes";int order=wall?Depth(y+py+hh-20):kind=="escadas"?-20500:kind=="sigilos"?-20400:kind=="nenufares"?-22600:kind=="margem"?-22700:-23000;
   var map=Map(id+"_"+py,parent,size/64f,Point(x,y+py+hh-size),order);
   for(int px=0;px<tex.width;px+=size){int ww=Math.Min(size,tex.width-px);bool visible=false;
    for(int yy=py;yy<py+hh&&!visible;yy+=4)for(int xx=px;xx<px+ww;xx+=4){if(colors[(tex.height-1-yy)*tex.width+xx].a>0){visible=true;break;}}
    if(!visible)continue;
    string path=Root+"/Generated/"+id+"_"+px+"_"+py+".asset";var tile=AssetDatabase.LoadAssetAtPath<Tile>(path);
    if(!tile){tile=ScriptableObject.CreateInstance<Tile>();tile.colliderType=Tile.ColliderType.None;AssetDatabase.CreateAsset(tile,path);
     var sp=Sprite.Create(tex,new Rect(px,tex.height-py-hh,ww,hh),Vector2.zero,64,0,SpriteMeshType.FullRect);sp.name=id+"_slice";AssetDatabase.AddObjectToAsset(sp,tile);tile.sprite=sp;EditorUtility.SetDirty(tile);}
    map.tileAnchor=Vector3.zero;map.SetTile(new Vector3Int(px/size,-1,0),tile);
   }
   // Gates are clipped in texture coordinates, so this material applies only to the original texture slices.
   if(gates!=null&&gates.Count>0){var mat=Mat(id+"_gates",0);for(int i=0;i<Math.Min(2,gates.Count);i++){var g=gates[i];mat.SetVector(i==0?"_Gate":"_Gate2",new Vector4(F(g[0])/w,F(g[1])/h,F(g[2])/w,F(g[3])/h));}map.GetComponent<TilemapRenderer>().sharedMaterial=mat;}
  }
 }
 public static string BuildVisuals(string name){
  Guard();plan=Read(name);var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
  if(scene.isDirty)throw new Exception("Preserve unsaved scene");
  Directory.CreateDirectory(Root+"/Scenes");string integrationPath=Root+"/Scenes/"+name+".unity";
  if(!File.Exists(integrationPath)){if(!AssetDatabase.CopyAsset("Assets/Scenes/"+name+".unity",integrationPath))throw new Exception("Cannot create isolated integration copy");}
  scene=EditorSceneManager.OpenScene(integrationPath);
  if(scene.path!=integrationPath)throw new Exception("Only isolated integration scenes may be changed");
  if(scene.isDirty)throw new Exception("Unsaved scene before integration");
  if(GameObject.Find("V65_Environment"))throw new Exception("Already imported; revise existing root instead");
  // A serialized copy permits recovery without touching the original repository.
  var backup=External+"/SceneBackups/"+name+"_before_V65.unity";Directory.CreateDirectory(Path.GetDirectoryName(backup));if(!File.Exists(backup))File.Copy(scene.path,backup);
  var actors=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Where(t=>t.GetComponent<PlayerController>()||t.GetComponent<PoeFollower>()||t.name=="AndrealphusAlcove").ToArray();
  foreach(var r in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Renderer>(true)))if(!actors.Any(a=>r.transform.IsChildOf(a)))r.enabled=false;
  foreach(var l in Object.FindObjectsByType<Light2D>(FindObjectsInactive.Include,FindObjectsSortMode.None))l.enabled=false;
  foreach(var c in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Collider2D>(true)))if(!c.isTrigger&&!actors.Any(a=>c.transform.IsChildOf(a)))c.enabled=false;
  world=new GameObject("V65_Environment");var contract=world.AddComponent<V65World>();contract.sourceRegions=plan["regions"].Count();contract.sourceFingerprint=(string)plan["source_manifest_sha256"];
  contract.regionIds=plan["regions"].Select(r=>(string)r["id"]).ToArray();contract.regionSpawns=plan["regions"].Select(r=>Point(r["spawn"])).ToArray();contract.spawn=Point(plan["regions"].First(r=>(string)r["id"]==(name=="Labirinto"?"L01":"M01"))["spawn"]);
  var sky=Child("AnimatedNegativeSpace",world.transform);var filter=sky.AddComponent<MeshFilter>();var mesh=new Mesh();float width=F(plan["canvas"][0])/64,height=F(plan["canvas"][1])/64;mesh.vertices=new[]{new Vector3(-2,2),new Vector3(width+2,2),new Vector3(-2,-height-2),new Vector3(width+2,-height-2)};mesh.uv=new[]{Vector2.up,Vector2.one,Vector2.zero,Vector2.right};mesh.triangles=new[]{0,1,2,2,1,3};mesh.colors=Enumerable.Repeat(Color.white,4).ToArray();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Root+"/Generated/"+name+"_background.asset");filter.sharedMesh=mesh;var mr=sky.AddComponent<MeshRenderer>();mr.sharedMaterial=Mat(name+"_negative_space",name=="Labirinto"?4:3);mr.sortingOrder=-30000;
  var corridors=Child("Corridors",world.transform);
  foreach(var c in plan["corridor_chunks"]){var s=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Textures/"+Path.GetFileName((string)c["source"]));var r=c["rect"];Render(Path.GetFileNameWithoutExtension((string)c["source"]),s,F(r[0]),F(r[1]),F(r[2]),F(r[3]),corridors.transform,-22000,Lit());}
  foreach(var reg in plan["regions"]){string id=(string)reg["id"];var parent=Child(id+"_"+(string)reg["name"],world.transform);
   foreach(var e in plan["entries"].Where(e=>(string)e["region"]==id)){
    if(((string)e["id"]).Contains("_layer_")){Layer(e,parent.transform);continue;}
    BuildObject(e,parent.transform);
   }
   AmbientRegion(reg,parent.transform,name);
  }
  foreach(var e in plan["entries"].Where(e=>(string)e["region"]=="external"))BuildObject(e,corridors.transform);
  foreach(var e in plan["effects"]){string type=(string)e["type"];if(type=="flame")Flame(e);if(type=="waterfall"){var g=Child("Waterfall_"+(string)e["region"],world.transform);g.transform.position=Point(F(e["x"]),F(e["y"]));Particles(g.transform,new Color(.25f,.4f,.8f,.45f),8,new Vector2(.1f,F(e["height"])/64),-1.4f);}}
  var global=Child("EnvironmentAmbient",world.transform).AddComponent<Light2D>();global.lightType=Light2D.LightType.Global;global.intensity=.9f;global.color=new Color(.88f,.87f,1);
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);return "Visual layers saved: "+name;
 }
 static void BuildObject(JToken e,Transform parent){
  string id=(string)e["id"],motion=(string)e["motion"],kind=(string)e["kind"];var r=e["rect"];float mode=motion=="anchored_wind"?1:motion=="pinned_cloth"||motion=="pendant"?2:motion=="glow"||(e["states"]?.Count()>0)?5:0;
  int order=kind=="marcas"?-20300:kind=="sigilos"?-20400:kind=="tapete"?-21000:kind=="solo"?-22900:Depth(F(e["anchor"][1]));
  var sr=Render(id,AssetSprite((string)e["asset"]),F(r[0]),F(r[1]),F(r[2]),F(r[3]),parent,order,mode==0?Lit():Mat("SharedMotion_"+mode,mode));var block=new MaterialPropertyBlock();block.SetFloat("_Seed",Mathf.Abs(id.GetHashCode()%1000)*.01f);sr.SetPropertyBlock(block);sr.flipX=(bool?)e["flip"]??false;if(sr.flipX)sr.transform.position+=Vector3.right*(F(r[2])/64f);visuals[id]=sr;
  if(motion=="float"){var a=sr.gameObject.AddComponent<V65Ambient>();a.floating=true;a.phase=F(r[0]);}
  if(e["states"]?.Count()>0){var v=sr.gameObject.AddComponent<V65StateVisual>();v.normal=sr.sprite;foreach(var st in e["states"]){var s=AssetSprite((string)st["asset"]);switch((string)st["name"]){case "normal":case "fechada":v.normal=s;break;case "interacao":case "aberta":v.interaction=s;break;case "erro":v.error=s;break;case "concluido":case "coletada":v.complete=s;break;}}}
 }
 static void Flame(JToken e){
  string id=(string)e["asset"];if(!flameCache.TryGetValue(id,out var frames)){
   var tex=AssetSprite(id).texture;var list=new List<Sprite>();string path=Root+"/Generated/Flames_"+id+".asset";var saved=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
   if(saved.Length>0)frames=saved;else {for(int y=tex.height-128;y>=0;y-=128)for(int x=0;x<tex.width;x+=128){var s=Sprite.Create(tex,new Rect(x,y,128,128),Vector2.zero,64,0,SpriteMeshType.FullRect);s.name="frame_"+list.Count.ToString("D2");if(list.Count==0)AssetDatabase.CreateAsset(s,path);else AssetDatabase.AddObjectToAsset(s,path);list.Add(s);}frames=list.ToArray();}flameCache[id]=frames;
  }
  float size=F(e["size"]);var sr=Render("Flame_"+(string)e["owner"],frames[0],F(e["x"])-size/2,F(e["y"])-size,size,size,world.transform,Depth(F(e["y"]))+2,Mat("FlameUnlit",0));var a=sr.gameObject.AddComponent<V65Ambient>();a.frames=frames;a.phase=F(e["phase"]);
  // Lights are limited to the first two flames of each room; every flame retains its own frame animation.
  string region=(string)e["region"];int count=world.GetComponentsInChildren<Light2D>().Count(l=>l.name.StartsWith(region+"_FlameLight"));
  if(count<2){var lg=Child(region+"_FlameLight"+count,world.transform);lg.transform.position=Point(F(e["x"]),F(e["y"]));var light=lg.AddComponent<Light2D>();light.lightType=Light2D.LightType.Point;light.pointLightOuterRadius=2.4f;light.pointLightInnerRadius=.25f;light.intensity=.45f;light.color=(string)e["color"]=="azul"?new Color(.3f,.45f,1):new Color(1,.55f,.28f);a.localLight=light;a.baseIntensity=.45f;Particles(lg.transform,light.color,1,new Vector2(.15f,.1f),.35f);}
 }
 static ParticleSystem Particles(Transform parent,Color color,float rate,Vector2 area,float velocity){
  var go=Child("EnvironmentalParticles",parent);var p=go.AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=p.main;main.startLifetime=5;main.startSpeed=0;main.startSize=new ParticleSystem.MinMaxCurve(.025f,.07f);main.startColor=color;main.maxParticles=100;main.simulationSpace=ParticleSystemSimulationSpace.World;main.loop=true;main.playOnAwake=true;
  var emission=p.emission;emission.rateOverTime=rate;var shape=p.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(area.x,area.y,.01f);var vel=p.velocityOverLifetime;vel.enabled=true;vel.x=.04f;vel.y=velocity;vel.z=0;var noise=p.noise;noise.enabled=true;noise.strength=.1f;noise.frequency=.2f;
  var life=p.colorOverLifetime;life.enabled=true;var grad=new Gradient();grad.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.6f,.25f),new GradientAlphaKey(0,1)});life.color=grad;
  var pr=p.GetComponent<ParticleSystemRenderer>();pr.sharedMaterial=Mat("Particle",6);pr.sortingOrder=28000;return p;
 }
 static void AmbientRegion(JToken reg,Transform parent,string name){var go=Child("Atmosphere",parent);var origin=Point(reg["origin"]);var size=Point(reg["size"]);go.transform.position=origin+size*.5f;Particles(go.transform,name=="Labirinto"?new Color(.5f,.3f,.7f,.4f):new Color(.65f,.7f,1,.5f),name=="Labirinto"?1.2f:2,new Vector2(Mathf.Abs(size.x)*.6f,Mathf.Abs(size.y)*.45f),-.06f);}
 public static string BuildPhysics(){
  Guard();world=GameObject.Find("V65_Environment");if(!world)throw new Exception("Import visuals first");var name=world.scene.name;plan=Read(name);if(world.GetComponent<V65World>().navigation)return "Physics exists";
  var nav=JObject.Parse(File.ReadAllText((string)plan["navigation_source"]));var rows=nav["grid"].Select(r=>(string)r).ToArray();int h=rows.Length,w=rows[0].Length;float cell=F(nav["cell_size"])/64;
  var floor=Map("NavigationFloor",world.transform,cell,Vector2.zero,-29000);floor.GetComponent<TilemapRenderer>().enabled=false;
  var walls=Map("CollisionBoundary",world.transform,cell,Vector2.zero,-29000);walls.GetComponent<TilemapRenderer>().enabled=false;
  var ft=AssetDatabase.LoadAssetAtPath<Tile>(Root+"/Generated/NavTile.asset");if(!ft){ft=ScriptableObject.CreateInstance<Tile>();ft.colliderType=Tile.ColliderType.None;AssetDatabase.CreateAsset(ft,Root+"/Generated/NavTile.asset");}
  var wt=AssetDatabase.LoadAssetAtPath<Tile>(Root+"/Generated/CollisionTile.asset");if(!wt){wt=ScriptableObject.CreateInstance<Tile>();wt.colliderType=Tile.ColliderType.Grid;AssetDatabase.CreateAsset(wt,Root+"/Generated/CollisionTile.asset");}
  var walk=new List<Vector3Int>();var boundary=new List<Vector3Int>();bool Walk(int x,int y)=>x>=0&&y>=0&&x<w&&y<h&&rows[y][x]=='1';
  for(int y=0;y<h;y++)for(int x=0;x<w;x++){if(Walk(x,y))walk.Add(new Vector3Int(x,-y-1,0));else if(Walk(x-1,y)||Walk(x+1,y)||Walk(x,y-1)||Walk(x,y+1))boundary.Add(new Vector3Int(x,-y-1,0));}
  floor.SetTiles(walk.ToArray(),Enumerable.Repeat<TileBase>(ft,walk.Count).ToArray());walls.SetTiles(boundary.ToArray(),Enumerable.Repeat<TileBase>(wt,boundary.Count).ToArray());
  var body=walls.gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Static;var cc=walls.gameObject.AddComponent<CompositeCollider2D>();cc.geometryType=CompositeCollider2D.GeometryType.Polygons;var tc=walls.gameObject.AddComponent<TilemapCollider2D>();tc.compositeOperation=Collider2D.CompositeOperation.Merge;tc.ProcessTilemapChanges();cc.GenerateGeometry();world.GetComponent<V65World>().navigation=floor;
  AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(world.scene);return "Walk cells "+walk.Count+", collider cells "+boundary.Count+", paths "+cc.pathCount;
 }
}


