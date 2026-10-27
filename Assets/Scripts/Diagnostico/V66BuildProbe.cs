using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MementoMori.V65;
using MementoMori.Player;

// Explicit opt-in diagnostic for the development executable; inert in normal play.
public sealed class V66BuildProbe:MonoBehaviour
{
 [Serializable] public class Sample {public int frame;public float time;public string file;public int particles;}
 [Serializable] public class Report {public string scene;public bool playerBuild;public int regions,width,height,errors;public float fps;public List<Sample> samples=new();}
 string requested;int errors;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Boot(){
#if DEVELOPMENT_BUILD
  var arg=Environment.GetCommandLineArgs().FirstOrDefault(a=>a.StartsWith("--v66-world="));if(arg==null)return;
  string name=arg.Substring("--v66-world=".Length);if(name!="Labirinto"&&name!="DominioLua")return;
  if(FindAnyObjectByType<V66BuildProbe>())return;var go=new GameObject("V66BuildEvidence");DontDestroyOnLoad(go);go.AddComponent<V66BuildProbe>().requested=name;
#endif
 }
 void OnEnable(){Application.logMessageReceived+=Log;}
 void OnDisable(){Application.logMessageReceived-=Log;}
 void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;}
 IEnumerator Start(){
  Application.runInBackground=true;Screen.SetResolution(1920,1080,FullScreenMode.Windowed);yield return SceneManager.LoadSceneAsync(requested);yield return null;yield return null;
  foreach(var b in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)){string n=b.GetType().Name;if(n.Contains("NarrativeController")||n=="DialogueTrigger"||n=="PoeRevealTrigger"||n.StartsWith("PoeEvent")){b.StopAllCoroutines();b.enabled=false;}}
  foreach(var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))c.enabled=false;
  var world=FindAnyObjectByType<V65World>();var camera=Camera.main;camera.GetComponent<CameraFollow2D>().enabled=false;camera.GetComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>().enabled=false;
  if(requested=="DominioLua"){camera.transform.position=new Vector3(110.4375f,-69.1f,-10);camera.orthographicSize=5;}
  else{int i=Array.IndexOf(world.regionIds,"L06");camera.transform.position=(Vector3)world.regionSpawns[i]+new Vector3(0,3,-10);camera.orthographicSize=5.625f;}
  var player=FindAnyObjectByType<PlayerController>();player.SetAutomationMoveInput(Vector2.zero);
  string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Evidence/Build"));Directory.CreateDirectory(dir);
  yield return new WaitForSecondsRealtime(3);int frame=Time.frameCount;float start=Time.realtimeSinceStartup;yield return new WaitForSecondsRealtime(10);
  var report=new Report{scene=requested,playerBuild=!Application.isEditor,regions=world.sourceRegions,width=Screen.width,height=Screen.height,fps=(Time.frameCount-frame)/(Time.realtimeSinceStartup-start)};
  for(int i=0;i<12;i++){yield return new WaitForEndOfFrame();string file=requested+"_motion_"+i.ToString("D2")+".png";ScreenCapture.CaptureScreenshot(Path.Combine(dir,file));report.samples.Add(new Sample{frame=Time.frameCount,time=Time.time,file=file,particles=FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Sum(p=>p.particleCount)});yield return new WaitForSecondsRealtime(.2f);}
  report.errors=errors;File.WriteAllText(Path.Combine(dir,requested+"_build_runtime.json"),JsonUtility.ToJson(report,true));Application.Quit(errors==0?0:1);
 }
}
