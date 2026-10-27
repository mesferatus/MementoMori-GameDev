using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;
using MementoMori.V65;
using MementoMori.Player;
using MementoMori.Poe;
using MementoMori.World;

public static partial class V65EnvironmentImporter
{
 public static string AuditPhysics(){
  world=GameObject.Find("V65_Environment");plan=Read(world.scene.name);Physics2D.SyncTransforms();var result=new List<object>();
  bool Obstacle(Collider2D c)=>c&&!c.isTrigger&&!c.GetComponentInParent<PlayerController>()&&!c.GetComponentInParent<PoeFollower>()&&!c.GetComponentInParent<DoorController>();
  foreach(var r in plan["regions"]){var p=Point(r["spawn"]);var hits=Physics2D.OverlapCircleAll(p,.27f).Where(Obstacle).Select(c=>c.name).Distinct().ToArray();result.Add(new {type="spawn",id=(string)r["id"],clear=hits.Length==0,hits});}
  foreach(var c in plan["connections"]){var pts=c["points"].Select(Point).ToArray();var hits=new HashSet<string>();for(int i=1;i<pts.Length;i++){Vector2 delta=pts[i]-pts[i-1];foreach(var hit in Physics2D.CircleCastAll(pts[i-1],.27f,delta.normalized,delta.magnitude))if(Obstacle(hit.collider))hits.Add(hit.collider.name);}result.Add(new {type="corridor",id=(string)c["id"],clear=hits.Count==0,hits=hits.ToArray()});}
  foreach(var t in plan["interaction_requirements"]){var p=Approach(t);var hits=Physics2D.OverlapCircleAll(p,.27f).Where(Obstacle).Select(c=>c.name).Distinct().ToArray();result.Add(new {type="approach",id=(string)t["object_id"],clear=hits.Length==0,hits});}
  var report=new {scene=world.scene.path,playing=Application.isPlaying,radius=.27f,doorGatesExcludedFromConnectivity=true,checks=result};var json=Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented);File.WriteAllText("TestResults/EnvironmentAudit/"+world.scene.name+"_physics.json",json);return json;
 }
}
