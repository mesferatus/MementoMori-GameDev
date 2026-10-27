using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class V66WaterfallInstaller
{
 const string Root="Assets/V66Environments";
 public static string Apply()
 {
  if(!Directory.Exists(Path.Combine(Application.dataPath,"V65Environments"))||EditorApplication.isPlaying)throw new Exception("Wrong project or Play Mode");
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
  if(scene.path!="Assets/V65Environments/Scenes/DominioLua.unity"||scene.isDirty)throw new Exception("Saved lunar integration scene required");
  var shader=Shader.Find("Memento/V66/Waterfall");if(!shader||ShaderUtil.ShaderHasError(shader))throw new Exception("Waterfall shader must compile first");
  Directory.CreateDirectory("TestResults/Recovery");Directory.CreateDirectory(Root+"/Generated");
  string backup="TestResults/Recovery/DominioLua_before_waterfalls.unity";if(!File.Exists(backup))File.Copy(scene.path,backup);
  var world=GameObject.Find("V65_Environment");if(!world)throw new Exception("Existing world missing");
  var plan=JObject.Parse(File.ReadAllText("Tools/EnvironmentMaintenance/Bundle/DominioLua.json"));
  var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Generated/WaterQuad.asset");
  if(!mesh){mesh=new Mesh{name="WaterQuad"};mesh.vertices=new[]{new Vector3(-.5f,0),new Vector3(.5f,0),new Vector3(-.5f,1),new Vector3(.5f,1)};mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.up,Vector2.one};mesh.triangles=new[]{0,2,1,1,2,3};mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Root+"/Generated/WaterQuad.asset");}
  int count=0;
  foreach(var e in plan["effects"].Where(e=>(string)e["type"]=="waterfall")){
   float x=(float)e["x"]/64,y=-(float)e["y"]/64,w=(float)e["width"]/64,h=(float)e["height"]/64;
   // The inherited M02 effect box includes part of the receiving lake. Match the actual painted spillway.
   if((string)e["region"]=="M02"){y=-4322f/64;h=106f/64;}
   string id="V66_Waterfall_"+(string)e["region"]+"_"+count;
   if(world.transform.Find(id)){count++;continue;}
   var root=new GameObject(id);root.transform.SetParent(world.transform);root.transform.position=new Vector3(x,y-h,0);
   int order=Mathf.Clamp(5000+Mathf.RoundToInt((-y+h)*100),-32000,31990);
   Material Material(string suffix,bool foam){var path=Root+"/Generated/"+id+suffix+".mat";var m=new Material(shader);m.SetVector("_Pixels",new Vector4((float)e["width"],foam?24:h*64,0,0));m.SetFloat("_Seed",count*1.73f);m.SetFloat("_Foam",foam?1:0);m.SetFloat("_Speed",100+count*9);AssetDatabase.CreateAsset(m,path);return m;}
   void Surface(string name,Vector3 scale,Vector3 offset,Material mat,int depth){var g=new GameObject(name);g.transform.SetParent(root.transform,false);g.transform.localPosition=offset;g.transform.localScale=scale;g.AddComponent<MeshFilter>().sharedMesh=mesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.sortingOrder=depth;}
   Surface("ContinuousDownwardFlow",new Vector3(w,h,1),Vector3.zero,Material("_Flow",false),order);
   Surface("ImpactFoamAndRipples",new Vector3(w*1.6f,.38f,1),new Vector3(0,-.19f,0),Material("_Foam",true),order+1);
   var spray=new GameObject("ImpactSpray");spray.transform.SetParent(root.transform,false);var ps=spray.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   var main=ps.main;main.startLifetime=new ParticleSystem.MinMaxCurve(.22f,.55f);main.startSize=new ParticleSystem.MinMaxCurve(.018f,.04f);main.startSpeed=0;main.startColor=new Color(.4f,.6f,1,.7f);main.gravityModifier=.3f;main.maxParticles=48;main.simulationSpace=ParticleSystemSimulationSpace.World;
   var emission=ps.emission;emission.rateOverTime=Mathf.Clamp(w*22,6,24);var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(w,.035f,.01f);
   var vel=ps.velocityOverLifetime;vel.enabled=true;vel.x=new ParticleSystem.MinMaxCurve(-.3f,.3f);vel.y=new ParticleSystem.MinMaxCurve(.3f,.8f);vel.z=new ParticleSystem.MinMaxCurve(0,0);
   var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/V65Environments/Generated/Particle.mat");renderer.sortingOrder=order+2;
   // Preserve the original emitter for recovery but replace its sparse rain with a coherent impact spray.
   foreach(var t in world.GetComponentsInChildren<Transform>().Where(t=>t.name=="Waterfall_"+(string)e["region"]&&Vector2.Distance(t.position,new Vector2(x,y))<.02f))t.gameObject.SetActive(false);
   count++;
  }
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
  return "Saved "+count+" waterfalls with continuous flow, impact foam, ripples and spray; existing geometry preserved";
 }
}
