using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MementoMori.V65 {
 public sealed class V65MotionEvidence : MonoBehaviour {
  [Serializable] public class Sample {public int frame;public float time;public Vector3 camera;public string[] flames;public int liveParticles;}
  [Serializable] public class Result {public string scene;public float averageFps,seconds;public int frames;public bool playing;public List<Sample> samples=new();}
  public string label="live";public bool Complete{get;private set;}
  public void Begin(){StartCoroutine(Run());}
  IEnumerator Run(){
   yield return new WaitForSecondsRealtime(3);var p=FindAnyObjectByType<MementoMori.Player.PlayerController>();p.SetAutomationMoveInput(Vector2.zero);
   var result=new Result{scene=gameObject.scene.name,playing=Application.isPlaying};float start=Time.realtimeSinceStartup;int frame=Time.frameCount;string dir=Path.GetFullPath(Path.Combine(Application.persistentDataPath,"Validation/EnvironmentAudit"));
   Directory.CreateDirectory(dir);for(int i=0;i<3;i++){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,label+"_"+i+".png"));result.samples.Add(new Sample{frame=Time.frameCount,time=Time.time,camera=Camera.main.transform.position,flames=FindObjectsByType<V65Ambient>(FindObjectsSortMode.None).Where(a=>a.GetComponent<SpriteRenderer>()&&a.GetComponent<SpriteRenderer>().isVisible).Select(a=>a.GetComponent<SpriteRenderer>().sprite?.name).ToArray(),liveParticles=FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Sum(s=>s.particleCount)});yield return new WaitForSecondsRealtime(1.1f);}
   result.frames=Time.frameCount-frame;result.seconds=Time.realtimeSinceStartup-start;result.averageFps=result.frames/result.seconds;p.ClearAutomationMoveInput();File.WriteAllText(Path.Combine(dir,label+"_motion.json"),JsonUtility.ToJson(result,true));Complete=true;
  }
 }
}
