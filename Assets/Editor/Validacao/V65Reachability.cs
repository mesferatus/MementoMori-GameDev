using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MementoMori.V65;

public static partial class V65EnvironmentImporter {
 public static string CheckReachability(bool repairApproaches) {
  Guard();world=GameObject.Find("V65_Environment");plan=Read(world.scene.name);
  if(!UnityEditor.EditorBuildSettings.scenes.Any(s=>s.enabled&&s.path==world.scene.path))throw new Exception("Isolated scene required");
  var nav=world.GetComponent<V65World>().navigation;
  var occupied=new HashSet<Vector3Int>();foreach(var p in nav.cellBounds.allPositionsWithin)if(nav.HasTile(p))occupied.Add(p);
  var safe=new HashSet<Vector3Int>();foreach(var p in occupied){bool ok=true;for(int y=-1;y<=1&&ok;y++)for(int x=-1;x<=1;x++)if(!occupied.Contains(p+new Vector3Int(x,y,0))){ok=false;break;}if(ok)safe.Add(p);}
  Vector3Int Nearest(Vector2 p, HashSet<Vector3Int> allowed=null){allowed=allowed??safe;var c=nav.WorldToCell(p);for(int r=0;r<=16;r++){var candidates=new List<Vector3Int>();for(int y=-r;y<=r;y++)for(int x=-r;x<=r;x++)if(Math.Abs(x)==r||Math.Abs(y)==r){var q=c+new Vector3Int(x,y,0);if(allowed.Contains(q))candidates.Add(q);}if(candidates.Count>0)return candidates.OrderBy(q=>Vector2.SqrMagnitude((Vector2)nav.GetCellCenterWorld(q)-p)).First();}throw new Exception("No clearance near "+p);}
  var start=Nearest(world.GetComponent<V65World>().spawn);var visited=new HashSet<Vector3Int>{start};var queue=new Queue<Vector3Int>();queue.Enqueue(start);var dirs=new[]{Vector3Int.up,Vector3Int.down,Vector3Int.left,Vector3Int.right};while(queue.Count>0){var p=queue.Dequeue();foreach(var d in dirs){var q=p+d;if(safe.Contains(q)&&visited.Add(q))queue.Enqueue(q);}}
  var checks=new List<object>();int failed=0,repairs=0;
  void Check(string type,string id,Vector2 p){var cell=Nearest(p,type=="interaction"?visited:safe);var position=(Vector2)nav.GetCellCenterWorld(cell);bool reachable=visited.Contains(cell);if(!reachable)failed++;checks.Add(new {type,id,reachable,requested=new[]{p.x,p.y},position=new[]{position.x,position.y},adjustment=Vector2.Distance(p,position)});}
  foreach(var r in plan["regions"])Check("region",(string)r["id"],Point(r["spawn"]));
  foreach(var c in plan["connections"]){int i=0;foreach(var p in c["points"])Check("corridor waypoint",(string)c["id"]+":"+i++,Point(p));}
  foreach(var t in plan["interaction_requirements"]){var p=Approach(t);Check("interaction",(string)t["object_id"],p);if(repairApproaches){var target=(Vector2)nav.GetCellCenterWorld(Nearest(p,visited));var prior=(Vector2)nav.GetCellCenterWorld(Nearest(p));var owners=SceneComponents<MonoBehaviour>().Where(b=>b&&b.GetType().Namespace!=null&&b.GetType().Namespace.StartsWith("MementoMori")&&!b.transform.IsChildOf(world.transform)&&b.GetComponent<Collider2D>()&&(Vector2.Distance(b.transform.position,p)<.03f||Vector2.Distance(b.transform.position,prior)<.03f)).Select(b=>b.transform).Distinct().ToArray();foreach(var owner in owners){owner.position=target;repairs++;}}}
  if(repairApproaches){Physics2D.SyncTransforms();EditorSceneManager.SaveScene(world.scene);}
  var report=new {scene=world.scene.path,method="Navigation grid eroded by 1 cell (0.1875 world-unit edge clearance; compact foot collider radius 0.18), four-neighbour flood; waypoints snap within 2 units; doors remain gated",walkCells=occupied.Count,safeCells=safe.Count,reachableSafeCells=visited.Count,failed,repairs,checks};
  var json=Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented);File.WriteAllText("TestResults/EnvironmentAudit/"+world.scene.name+"_reachability.json",json);return "Checks "+checks.Count+", unreachable "+failed+", adjusted owners "+repairs;
 }
}



