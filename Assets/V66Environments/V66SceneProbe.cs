using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MementoMori.Player;
using MementoMori.Poe;
using MementoMori.V65;

// Diagnostic only: attached in Play Mode, never serialized into a game scene.
public sealed class V66SceneProbe : MonoBehaviour
{
 [Serializable] public class Check {public string region;public bool playerMoved,poeMoved,poeStayedOnFloor;public int solidContacts;public float playerDistance,poeDistance,finalSeparation;public string capture;}
 [Serializable] public class Report {public string scene;public bool playMode,narrativeSuspended=true;public int startFrame,endFrame;public float seconds,fps;public List<Check> checks=new();}
 public bool Complete {get;private set;}
 public string Status {get;private set;}
 public void Begin(){StartCoroutine(Run());}
 IEnumerator Run(){
  Application.runInBackground=true;var world=FindAnyObjectByType<V65World>();var player=FindAnyObjectByType<PlayerController>();var body=player.GetComponent<Rigidbody2D>();var poe=FindAnyObjectByType<PoeFollower>(FindObjectsInactive.Include);var camera=Camera.main;
  string dir=Path.GetFullPath(Path.Combine(Application.persistentDataPath,"Validation/Consolidation"));Directory.CreateDirectory(dir);
  var report=new Report{scene=world.gameObject.scene.name,playMode=Application.isPlaying,startFrame=Time.frameCount};float start=Time.realtimeSinceStartup;
  var disabled=new List<Behaviour>();foreach(var b in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)){string n=b.GetType().Name;if(b.enabled&&(n.Contains("NarrativeController")||n=="DialogueTrigger"||n=="PoeRevealTrigger"||n=="PoeEventTrigger"||n=="V6AndrealphusAppearance")){b.enabled=false;disabled.Add(b);}}
  var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.enabled).ToArray();foreach(var c in canvases)c.enabled=false;
  MementoMori.Core.InputGate.Instance?.ClearAll();
  bool Clear(Vector2 p,float radius){if(!world.navigation.HasTile(world.navigation.WorldToCell(p)))return false;return !Physics2D.OverlapCircleAll(p,radius).Any(c=>!c.isTrigger&&!c.GetComponentInParent<PlayerController>()&&!c.GetComponentInParent<PoeFollower>());}
  poe.gameObject.SetActive(true);poe.ConfigureNavigationFloor(world.navigation);
  for(int i=0;i<world.regionIds.Length;i++){
   Status="Region "+world.regionIds[i];var origin=world.regionSpawns[i];body.position=origin;player.transform.position=origin;Physics2D.SyncTransforms();
   Vector2 direction=Vector2.zero;float distance=0;
   foreach(var d in new[]{Vector2.up,Vector2.right,Vector2.left,Vector2.down}){float length=0;for(int k=1;k<=40;k++){if(!Clear(origin+d*k*.1f,player.GetComponent<CircleCollider2D>().radius))break;length=k*.1f;}if(length>distance){distance=length;direction=d;}}
   poe.transform.position=origin;poe.BeginFollowing();yield return null;
   var check=new Check{region=world.regionIds[i],poeStayedOnFloor=true};Vector2 before=body.position,poeBefore=poe.transform.position;float end=Time.time+Mathf.Min(.85f,distance/5);
   player.SetAutomationMoveInput(direction);
   while(Time.time<end){if(!world.navigation.HasTile(world.navigation.WorldToCell(poe.transform.position)))check.poeStayedOnFloor=false;if(!Clear(poe.transform.position,.16f))check.solidContacts++;yield return null;}
   player.ClearAutomationMoveInput();yield return new WaitForSeconds(1.5f);
   check.playerDistance=Vector2.Distance(before,body.position);check.poeDistance=Vector2.Distance(poeBefore,poe.transform.position);check.playerMoved=check.playerDistance>.3f;check.poeMoved=check.poeDistance>.2f;check.finalSeparation=Vector2.Distance(poe.transform.position,body.position);
   yield return new WaitForEndOfFrame();check.capture=world.regionIds[i]+"_gameplay.png";ScreenCapture.CaptureScreenshot(Path.Combine(dir,check.capture));report.checks.Add(check);
  }
  player.ClearAutomationMoveInput();foreach(var b in disabled)if(b)b.enabled=true;foreach(var c in canvases)if(c)c.enabled=true;
  report.endFrame=Time.frameCount;report.seconds=Time.realtimeSinceStartup-start;report.fps=(report.endFrame-report.startFrame)/report.seconds;File.WriteAllText(Path.Combine(dir,report.scene+"_runtime.json"),JsonUtility.ToJson(report,true));Status="Complete";Complete=true;
 }
}
