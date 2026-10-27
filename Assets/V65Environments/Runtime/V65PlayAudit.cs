using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MementoMori.Player;
using MementoMori.Core;

namespace MementoMori.V65
{
 public sealed class V65PlayAudit : MonoBehaviour
 {
  [Serializable] public class Check {public string region;public bool spawnClear,inputBlocked,moved;public float displacement;public int frameStart,frameEnd;}
  [Serializable] public class Report {public string scene;public bool playing,narrativeSuspendedForMovementProbe;public int startFrame,endFrame;public float elapsed,averageFps;public int animatedFlames,particles,regions;public List<Check> checks=new();}
  public bool Complete {get;private set;}
  public string ReportPath {get;private set;}
  public void Begin(){StartCoroutine(Run());}
  bool Clear(Vector2 point,PlayerController player){foreach(var hit in Physics2D.OverlapCircleAll(point,player.GetComponent<CircleCollider2D>().radius))if(!hit.isTrigger&&!hit.transform.IsChildOf(player.transform))return false;return true;}
  IEnumerator Run(){
   var world=FindAnyObjectByType<V65World>();var player=FindAnyObjectByType<PlayerController>();var body=player.GetComponent<Rigidbody2D>();var original=player.transform.position; var poe=FindAnyObjectByType<MementoMori.Poe.PoeFollower>(); var poeOriginal=poe?poe.transform.position:Vector3.zero; var poeState=poe?poe.State:MementoMori.Poe.PoeState.Hidden; if(poe)poe.SetStoryState(MementoMori.Poe.PoeState.Waiting);
   var report=new Report{scene=gameObject.scene.name,playing=Application.isPlaying,startFrame=Time.frameCount,regions=world.sourceRegions,narrativeSuspendedForMovementProbe=true};float start=Time.realtimeSinceStartup;
   var suspended=new List<Behaviour>();
   foreach(var b in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)){string n=b.GetType().Name;if(b.enabled&&(n=="DialogueTrigger"||n=="PoeRevealTrigger"||n=="PoeEventTrigger"||n=="LabyrinthNarrativeController"||n=="MoonDomainNarrativeController"||n=="V6AndrealphusAppearance")){b.enabled=false;suspended.Add(b);}}
   InputGate.Instance?.ClearAll();
   report.animatedFlames=world.GetComponentsInChildren<V65Ambient>().Length;report.particles=world.GetComponentsInChildren<ParticleSystem>().Length;
   for(int i=0;i<world.regionSpawns.Length;i++){
    body.position=world.regionSpawns[i];player.transform.position=body.position;Physics2D.SyncTransforms();yield return new WaitForFixedUpdate();
    var check=new Check{region=world.regionIds[i],spawnClear=Clear(body.position,player),inputBlocked=InputGate.Instance!=null&&InputGate.Instance.IsBlocked,frameStart=Time.frameCount};
    Vector2 dir=Vector2.zero;
    foreach(var d in new[]{Vector2.up,Vector2.left,Vector2.right,Vector2.down}){bool clear=true;for(int k=1;k<=10;k++)if(!Clear(body.position+d*k*.1f,player)){clear=false;break;}if(clear){dir=d;break;}}
    Vector2 before=body.position;player.SetAutomationMoveInput(dir);yield return new WaitForSeconds(.25f);player.ClearAutomationMoveInput();yield return new WaitForFixedUpdate();check.displacement=Vector2.Distance(before,body.position);check.moved=check.displacement>.3f;check.frameEnd=Time.frameCount;report.checks.Add(check);
   }
   player.ClearAutomationMoveInput(); if(poe){poe.transform.position=poeOriginal;poe.SetStoryState(poeState);}body.position=original;player.transform.position=original;foreach(var b in suspended)if(b)b.enabled=true;report.endFrame=Time.frameCount;report.elapsed=Time.realtimeSinceStartup-start;report.averageFps=(report.endFrame-report.startFrame)/report.elapsed;
   string dirPath=Path.Combine(Application.persistentDataPath,"Validation/EnvironmentAudit");Directory.CreateDirectory(dirPath);ReportPath=Path.Combine(dirPath,report.scene+"_play_audit.json");File.WriteAllText(ReportPath,JsonUtility.ToJson(report,true));Complete=true;
  }
 }
}


