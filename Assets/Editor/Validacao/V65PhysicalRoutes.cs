using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using MementoMori.Player;
using MementoMori.Poe;
using MementoMori.World;
using MementoMori.V65;
public static partial class V65EnvironmentImporter {
 public static string AuditPhysicalRoutes(){
  Guard();world=GameObject.Find("V65_Environment");plan=Read(world.scene.name);Physics2D.SyncTransforms();var floor=world.GetComponent<V65World>().navigation;var player=SceneComponents<PlayerController>().First();float radius=player.GetComponent<CircleCollider2D>().radius;
  var walk=new HashSet<Vector3Int>();foreach(var p in floor.cellBounds.allPositionsWithin)if(floor.HasTile(p))walk.Add(p);var safe=new HashSet<Vector3Int>();foreach(var p in walk){bool ok=true;for(int y=-1;y<=1&&ok;y++)for(int x=-1;x<=1;x++)if(!walk.Contains(p+new Vector3Int(x,y,0))){ok=false;break;}if(ok)safe.Add(p);}
  Vector3Int Nearest(Vector2 point){var p=floor.WorldToCell(point);for(int r=0;r<20;r++)for(int y=-r;y<=r;y++)for(int x=-r;x<=r;x++){var q=p+new Vector3Int(x,y,0);if(safe.Contains(q))return q;}throw new Exception("Missing route endpoint "+point);}
  var start=Nearest(world.GetComponent<V65World>().spawn);var prev=new Dictionary<Vector3Int,Vector3Int>{{start,start}};var queue=new Queue<Vector3Int>();queue.Enqueue(start);var dirs=new[]{Vector3Int.up,Vector3Int.down,Vector3Int.left,Vector3Int.right};while(queue.Count>0){var p=queue.Dequeue();foreach(var d in dirs){var q=p+d;if(safe.Contains(q)&&!prev.ContainsKey(q)){prev[q]=p;queue.Enqueue(q);}}}
  bool Solid(Collider2D c)=>c&&!c.isTrigger&&!c.GetComponentInParent<PlayerController>()&&!c.GetComponentInParent<PoeFollower>()&&!c.GetComponentInParent<DoorController>();
  var targets=new List<(string id,Vector2 p)>();foreach(var r in plan["regions"])targets.Add(((string)r["id"],Point(r["spawn"])));foreach(var c in plan["connections"]){int i=0;foreach(var p in c["points"])targets.Add(((string)c["id"]+":"+i++,Point(p)));}foreach(var t in plan["interaction_requirements"])targets.Add(((string)t["object_id"],Approach(t)));
  var checks=new List<object>();int failed=0,casts=0;var cache=new Dictionary<string,string[]>();
  foreach(var target in targets){var end=Nearest(target.p);bool reachable=prev.ContainsKey(end);var hits=new HashSet<string>();if(reachable){var path=new List<Vector3Int>{end};for(var p=end;p!=start;){p=prev[p];path.Add(p);}path.Reverse();int begin=0;for(int i=1;i<path.Count;i++){if(i+1<path.Count&&path[i]-path[i-1]==path[i+1]-path[i])continue;var a=(Vector2)floor.GetCellCenterWorld(path[begin]);var b=(Vector2)floor.GetCellCenterWorld(path[i]);string key=path[begin]+":"+path[i];if(!cache.TryGetValue(key,out var found)){var delta=b-a;found=Physics2D.CircleCastAll(a,radius,delta.normalized,delta.magnitude).Where(h=>Solid(h.collider)).Select(h=>h.collider.name).Distinct().ToArray();cache[key]=found;casts++;}foreach(var hit in found)hits.Add(hit);begin=i;}}
   bool pass=reachable&&hits.Count==0;if(!pass)failed++;checks.Add(new{id=target.id,reachable,clear=pass,hits=hits.ToArray()});
  }
  string json=Newtonsoft.Json.JsonConvert.SerializeObject(new{scene=world.scene.path,radius,doorGatesExcluded=true,method="Grid paths verified by CircleCastAll against actual Unity colliders; full paths from entry to every region, corridor waypoint and interaction approach",targets=checks.Count,casts,failed,checks},Newtonsoft.Json.Formatting.Indented);File.WriteAllText("TestResults/EnvironmentAudit/"+world.scene.name+"_physical_routes.json",json);return "Physical routes: "+checks.Count+", casts "+casts+", failures "+failed;
 }
}
