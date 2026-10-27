using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
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
 static T[] SceneComponents<T>() where T:Component => UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
 static void Property(Object target,string name,Vector2 value){var so=new SerializedObject(target);var p=so.FindProperty(name);if(p!=null&&p.propertyType==SerializedPropertyType.Vector2){p.vector2Value=value;so.ApplyModifiedPropertiesWithoutUndo();}}
 static Vector2 RegionPoint(string id,float x=768,float y=800){var r=plan["regions"].First(r=>(string)r["id"]==id);return Point(F(r["origin"][0])+x,F(r["origin"][1])+y);}
 static JToken Requirement(string id)=>plan["interaction_requirements"].FirstOrDefault(t=>(string)t["id"]==id);
 static Vector2 Approach(JToken t)=>t["approach"]!=null?Point(t["approach"]):Point(F(t["x"]),F(t["y"]));
 static void Place(Component owner,JToken target){if(target==null)return;owner.transform.position=Approach(target);foreach(var c in owner.GetComponents<Collider2D>()){c.enabled=true;c.isTrigger=true;if(c is BoxCollider2D b){b.offset=Vector2.zero;b.size=new Vector2(.7f,.7f);}if(c is CircleCollider2D circle){circle.offset=Vector2.zero;circle.radius=.45f;}}}
 static string Group(string id)=>id.StartsWith("moon.garden")?"moon.garden":id.StartsWith("moon.mirrors")?"moon.mirrors":id.StartsWith("moon.illusion")?"moon.illusory_corridor":id=="moon.phase_cycle"?"moon.cycles":id=="moon.fragmented_sigil"?"moon.sigil":id.StartsWith("lab.echoes")?"lab.echoes":id=="lab.moon_portal"?"lab.moon_activation":id;
 public static string BindGameplay(){
  Guard();world=GameObject.Find("V65_Environment");plan=Read(world.scene.name);var contract=world.GetComponent<V65World>();
  if(world.transform.Find("BindingsCompleted"))return "Bindings already applied";
  bool moon=world.scene.name=="DominioLua";
  // Explicit old room centres come from the saved, authored scenes; no gameplay rules change.
  string[] ids=moon?new[]{"M01","M02","M03","M04","M05","M06","M07","M08"}:new[]{"L01","L02","L03","L04","L05","L06","L07","L08","L09","L10","L11","L12","L13","L14"};
  Vector2[] old=moon?new[]{new Vector2(3,-5),new Vector2(57,-62),new Vector2(2,-30),new Vector2(38,-23),new Vector2(0,-73),new Vector2(0,-89),new Vector2(-53,-88),new Vector2(-53,-99)}:new[]{new Vector2(0,26),new Vector2(24,8),new Vector2(20,22),new Vector2(-24,23),new Vector2(-24,4.4f),new Vector2(20,-2.5f),new Vector2(0,-13),new Vector2(-20,-29),new Vector2(0,-29),new Vector2(20,-29),new Vector2(-20,-40),new Vector2(0,-40),new Vector2(20,-40),new Vector2(0,-50)};
  Vector2 Convert(Vector2 p){int i=Enumerable.Range(0,old.Length).OrderBy(i=>(old[i]-p).sqrMagnitude).First();return RegionPoint(ids[i])+(p-old[i])*.55f;}
  var player=SceneComponents<PlayerController>().First(p=>p.gameObject.activeInHierarchy);var poe=SceneComponents<PoeFollower>().FirstOrDefault();
  var candidates=SceneComponents<MonoBehaviour>().Where(b=>b&&b.GetType().Namespace!=null&&b.GetType().Namespace.StartsWith("MementoMori")&&!b.GetType().Namespace.Contains("UI")&&!b.transform.IsChildOf(world.transform)&&!b.transform.IsChildOf(player.transform)&&(poe==null||!b.transform.IsChildOf(poe.transform))&&(b.GetComponent<Collider2D>()||b.GetType().Name=="PoeEventPoint")).Select(b=>b.transform).Distinct().Select(t=>new {t,pos=Convert(t.position)}).ToArray();
  Vector2 Safe(Vector2 requested){var floor=contract.navigation;var start=floor.WorldToCell(requested);for(int r=0;r<56;r++)for(int yy=-r;yy<=r;yy++)for(int xx=-r;xx<=r;xx++){if(Math.Abs(xx)!=r&&Math.Abs(yy)!=r)continue;var cell=start+new Vector3Int(xx,yy,0);bool clear=true;for(int dy=-2;dy<=2&&clear;dy++)for(int dx=-2;dx<=2;dx++)if(!floor.HasTile(cell+new Vector3Int(dx,dy,0))){clear=false;break;}if(clear)return floor.GetCellCenterWorld(cell);}return requested;}
  foreach(var item in candidates.OrderBy(i=>i.t.GetComponentsInParent<Transform>().Length))item.t.position=Safe(item.pos);
  var requirements=plan["interaction_requirements"].ToArray();
  var mappings=new List<object>();
  foreach(var challenge in SceneComponents<V6Challenge>()){
   var so=new SerializedObject(challenge);string id=so.FindProperty("definition.id").stringValue;var t=requirements.FirstOrDefault(r=>Group((string)r["id"])==id);if(t!=null){Place(challenge,t);Property(challenge,"unused",Vector2.zero);mappings.Add(new {owner=challenge.name,region=(string)t["region"],requirement=(string)t["id"]});}
  }
  foreach(var c in SceneComponents<FalseDoorController>())Place(c,Requirement("lab.false_door"));
  foreach(var c in SceneComponents<VoiceWellController>())Place(c,Requirement("lab.voice_well"));
  foreach(var c in SceneComponents<EchoPassageChoice>()){int index=new SerializedObject(c).FindProperty("passage").intValue;Place(c,Requirement("lab.echoes.node."+index));}
  foreach(var c in SceneComponents<EchoCorridorPuzzle>())if(!moon)Property(c,"returnPosition",RegionPoint("L03",768,1050));
  foreach(var mirror in SceneComponents<MirrorSymbol>()){var t=Requirement("moon.mirrors."+mirror.SymbolId.ToLowerInvariant());Place(mirror,t);if(t!=null)mappings.Add(new {owner=mirror.name,region="M03",requirement=(string)t["id"]});}
  if(moon){
   var garden=SceneComponents<GardenPetalPuzzle>();
   foreach(var g in garden){var key=g.Petal==MoonPetal.Crescente?"crescent":g.Petal==MoonPetal.Cheia?"full":"waning";var req=Requirement("moon.garden."+key);Place(g,req);if(g.Petal==MoonPetal.Cheia)g.ConfigureFullReflection(Approach(req));}
   foreach(var g in SceneComponents<MementoMori.World.GardenPetalInteractable>()){
    string n=g.name;string key=n.Contains("Crescente")?"crescent":n.Contains("Cheia")?"full":"waning";var ts=requirements.Where(t=>(string)t["id"]=="moon.garden."+key).ToArray();int idx=n.Contains("Decoy")?Math.Min(1,ts.Length-1):n.Contains("Reflexo")?Math.Min(2,ts.Length-1):0;Place(g,ts[idx]);}
   var waning=requirements.Where(t=>(string)t["id"]=="moon.garden.waning").ToArray();foreach(var n in SceneComponents<MementoMori.Interaction.WaningFlowerNode>()){int i=new SerializedObject(n).FindProperty("flowerIndex").intValue;Place(n,waning[Mathf.Clamp(i,0,waning.Length-1)]);}
   var sigil=Requirement("moon.fragmented_sigil");var rings=SceneComponents<MementoMori.World.SigilRingInteractable>();for(int i=0;i<rings.Length;i++){Place(rings[i],sigil);rings[i].transform.position+=(Vector3)(Vector2.right*((i-1)*.9f));}
   foreach(var f in SceneComponents<FragmentCollectible>())f.transform.position=Safe(RegionPoint("M07",768,1100));
   foreach(var door in SceneComponents<DoorController>()){
    door.transform.position=door.name=="MirrorGate"?Point(3168,3570):Point(1900,8480);
    var blocker=door.GetComponent<BoxCollider2D>()??door.gameObject.AddComponent<BoxCollider2D>();blocker.offset=Vector2.zero;blocker.size=new Vector2(3.25f,.22f);blocker.isTrigger=false;blocker.enabled=true;door.Configure(blocker);
    var pulse=Particles(door.transform,new Color(.45f,.25f,1,.8f),12,new Vector2(3.1f,.08f),.05f);var react=pulse.gameObject.AddComponent<V65DoorEffect>();react.door=door;
   }
  }
  foreach(var portal in SceneComponents<Portal>()){
   string region=moon?"M08":portal.name.Contains("Moon")?"L08":portal.name.Contains("Devil")?"L09":portal.name.Contains("Tower")?"L10":portal.name.Contains("Hanged")?"L11":portal.name.Contains("Hermit")?"L12":portal.name.Contains("Death")?"L13":"L14";
   portal.transform.position=Safe(RegionPoint(region,768,950));foreach(var c in portal.GetComponents<Collider2D>()){c.enabled=true;c.isTrigger=true;}
   var visual=world.transform.Find(region+"_"+(string)plan["regions"].First(r=>(string)r["id"]==region)["name"]);
   if(visual){var sr=visual.GetComponentsInChildren<SpriteRenderer>().OrderBy(s=>Vector2.Distance(s.bounds.center,portal.transform.position)).FirstOrDefault(s=>s.name.Contains("_00"));if(sr){var v=sr.GetComponent<V65StateVisual>()??sr.gameObject.AddComponent<V65StateVisual>();v.portal=portal;v.normal=sr.sprite;sr.sharedMaterial=Mat(sr.name+"_portal",5);}}
  }
  foreach(var e in plan["entries"].Where(e=>e["states"]?.Count()>0)){
   string id=(string)e["id"],group=(string)e["source_puzzle_group"];var target=world.GetComponentsInChildren<V65StateVisual>().FirstOrDefault(v=>v.name==id);if(!target)continue;target.puzzleId=Group(group);
   if(group.StartsWith("moon.mirrors")){string symbol=group.Split('.').Last();target.mirror=SceneComponents<MirrorSymbol>().FirstOrDefault(m=>m.SymbolId.Equals(symbol,StringComparison.OrdinalIgnoreCase));}
   if(group.StartsWith("moon.garden")){var petal=group.EndsWith("crescent")?MoonPetal.Crescente:group.EndsWith("full")?MoonPetal.Cheia:MoonPetal.Minguante;target.garden=SceneComponents<GardenPetalPuzzle>().FirstOrDefault(g=>g.Petal==petal);}
   if(group=="moon.fragmented_sigil")target.sigil=SceneComponents<SigilRingPuzzle>().FirstOrDefault();
   target.response=Particles(target.transform,new Color(.6f,.35f,1,.8f),0,new Vector2(.5f,.5f),.2f);var emission=target.response.emission;emission.SetBursts(new[]{new ParticleSystem.Burst(0,12)});var main=target.response.main;main.loop=false;main.playOnAwake=false;
  }
  player.transform.position=contract.spawn;var body=player.GetComponent<Rigidbody2D>();if(body)body.position=contract.spawn;
  if(poe){poe.ConfigureNavigationFloor(contract.navigation);poe.transform.position=contract.spawn+Vector2.left*.65f;}
  var camera=Camera.main;if(camera){camera.GetComponent<CameraFollow2D>()?.Configure(player.transform,new Vector2(0,-F(plan["canvas"][1])/64),new Vector2(F(plan["canvas"][0])/64,0));camera.transform.position=new Vector3(contract.spawn.x,contract.spawn.y,-10);camera.orthographicSize=5.625f;var pp=camera.GetComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>();if(pp){pp.assetsPPU=32;pp.refResolutionX=640;pp.refResolutionY=360;}}
  CompactActor(player);
  Child("BindingsCompleted",world.transform);File.WriteAllText("TestResults/EnvironmentAudit/"+world.scene.name+"_bindings.json",Newtonsoft.Json.JsonConvert.SerializeObject(mappings,Newtonsoft.Json.Formatting.Indented));EditorSceneManager.SaveScene(world.scene);AssetDatabase.SaveAssets();return "Existing gameplay owners relocated; bindings "+mappings.Count;
 }
 static void CompactActor(PlayerController player){
  string path=Root+"/Generated/MelanthaCompact.controller";var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
  if(!controller){
   controller=AnimatorController.CreateAnimatorControllerAtPath(path);foreach(var p in new[]{"Speed","MoveX","MoveY","LastMoveX","LastMoveY"})controller.AddParameter(p,AnimatorControllerParameterType.Float);
   AnimationClip Clip(string direction,bool moving){string file=Root+"/Generated/compact_"+(moving?"walk_":"idle_")+direction+".anim";var clip=new AnimationClip(){frameRate=8,name=Path.GetFileNameWithoutExtension(file)};var list=new List<ObjectReferenceKeyframe>();int count=moving?6:1;for(int i=0;i<count;i++){string source="Assets/Art/FinalProduction/R3/Melantha/"+(moving?"walk_"+direction+"_"+i:"idle_"+direction)+".png";var sp=AssetDatabase.LoadAssetAtPath<Sprite>(source);if(!sp)throw new Exception("Missing compact frame "+source);list.Add(new ObjectReferenceKeyframe{time=i/8f,value=sp});}list.Add(new ObjectReferenceKeyframe{time=count/8f,value=list[0].value});AnimationUtility.SetObjectReferenceCurve(clip,new EditorCurveBinding{path="",type=typeof(SpriteRenderer),propertyName="m_Sprite"},list.ToArray());var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,settings);AssetDatabase.CreateAsset(clip,file);return clip;}
   BlendTree Direction(bool walking){var tree=new BlendTree{name=walking?"Walking":"Idle",blendType=BlendTreeType.SimpleDirectional2D,blendParameter=walking?"MoveX":"LastMoveX",blendParameterY=walking?"MoveY":"LastMoveY"};AssetDatabase.AddObjectToAsset(tree,controller);tree.AddChild(Clip("front",walking),Vector2.down);tree.AddChild(Clip("back",walking),Vector2.up);var side=Clip("side",walking);tree.AddChild(side,Vector2.right);tree.AddChild(side,Vector2.left);return tree;}
   var root=new BlendTree{name="Locomotion",blendType=BlendTreeType.Simple1D,blendParameter="Speed",useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(root,controller);root.AddChild(Direction(false),0);root.AddChild(Direction(true),.1f);var state=controller.layers[0].stateMachine.AddState("CompactLocomotion");state.motion=root;controller.layers[0].stateMachine.defaultState=state;
  }
  var animator=player.GetComponentsInChildren<Animator>(true).First();animator.runtimeAnimatorController=controller;animator.transform.localScale=Vector3.one;animator.transform.localPosition=Vector3.zero;var sr=animator.GetComponent<SpriteRenderer>();sr.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/FinalProduction/R3/Melantha/idle_front.png");sr.enabled=true;
 }
}
