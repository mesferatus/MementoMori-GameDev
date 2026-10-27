using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;

[InitializeOnLoad]
public static class V66Validation
{
 const string Output="TestResults/Consolidation";
 static TestRunnerApi api;
 static V66Validation(){if(SessionState.GetString("V66TestMode","")!="")Register();}
 static void Guard(){if(!Directory.Exists(Path.Combine(Application.dataPath,"V65Environments")))throw new Exception("Wrong project");Directory.CreateDirectory(Output);}
 static void Register(){if(api)return;api=ScriptableObject.CreateInstance<TestRunnerApi>();api.RegisterCallbacks(new Results());}
 public static string Tests(string mode){Guard();if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode");SessionState.SetString("V66TestMode",mode);Register();return api.Execute(new ExecutionSettings(new Filter{testMode=mode=="EditMode"?TestMode.EditMode:TestMode.PlayMode,assemblyNames=new[]{mode=="EditMode"?"MementoMori.EditMode.Tests":"MementoMori.PlayMode.Tests"}}));}
 sealed class Results:ICallbacks {
  public void RunStarted(ITestAdaptor test){}
  public void TestStarted(ITestAdaptor test){}
  public void TestFinished(ITestResultAdaptor test){}
  public void RunFinished(ITestResultAdaptor result){Guard();string mode=SessionState.GetString("V66TestMode","Unknown");TestRunnerApi.SaveResultToFile(result,Path.GetFullPath(Output+"/"+mode+".xml"));File.WriteAllText(Output+"/"+mode+"_summary.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{mode,result.PassCount,result.FailCount,result.SkipCount,result.InconclusiveCount,result.ResultState},Newtonsoft.Json.Formatting.Indented));SessionState.EraseString("V66TestMode");}
 }
 public static string Inventory(){
  Guard();var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();var roots=scene.GetRootGameObjects();var all=roots.SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();var world=UnityEngine.Object.FindAnyObjectByType<MementoMori.V65.V65World>();var camera=Camera.main;
  var shaders=all.SelectMany(t=>t.GetComponents<Renderer>()).SelectMany(r=>r.sharedMaterials).Where(m=>m&&m.shader).Select(m=>m.shader).Distinct().ToArray();
  var report=new{scene=scene.path,project=Application.dataPath,unity=Application.unityVersion,pipeline=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline?.GetType().FullName,regions=world.regionIds,missing=all.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)),colliders=all.SelectMany(t=>t.GetComponents<Collider2D>()).Count(c=>c.enabled&&c.gameObject.activeInHierarchy),triggers=all.SelectMany(t=>t.GetComponents<Collider2D>()).Count(c=>c.enabled&&c.isTrigger&&c.gameObject.activeInHierarchy),waterfalls=all.Count(t=>t.name.StartsWith("V66_Waterfall_")),flames=all.Count(t=>t.name.StartsWith("Flame_")),particles=all.Count(t=>t.GetComponent<ParticleSystem>()&&t.gameObject.activeInHierarchy),shaderErrors=shaders.Where(ShaderUtil.ShaderHasError).Select(s=>s.name).ToArray(),shaders=shaders.Select(s=>s.name).ToArray(),camera=new{camera.orthographic,camera.orthographicSize,camera.allowMSAA,camera.allowHDR},buildScenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray()};
  var json=Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Output+"/"+scene.name+"_inventory.json",json);return json;
 }
 public static string QueueBuild(){Guard();EditorApplication.delayCall+=Build;return "Windows build queued";}
 static void Build(){
  Guard();Directory.CreateDirectory("Builds/Validation");
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/Validation/MementoMori.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
  var s=report.summary;File.WriteAllText(Output+"/build.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{result=s.result.ToString(),s.totalErrors,s.totalWarnings,seconds=s.totalTime.TotalSeconds,s.totalSize,s.outputPath},Newtonsoft.Json.Formatting.Indented));
 }
}
