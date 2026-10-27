if(!UnityEngine.Application.isPlaying)throw new System.Exception("Runtime probe requires Play Mode");

var world=UnityEngine.Object.FindAnyObjectByType<MementoMori.V65.V65World>();
var player=UnityEngine.Object.FindAnyObjectByType<MementoMori.Player.PlayerController>();
var poe=UnityEngine.Object.FindAnyObjectByType<MementoMori.Poe.PoeFollower>(UnityEngine.FindObjectsInactive.Include);
var body=player.GetComponent<UnityEngine.Rigidbody2D>();float radius=player.GetComponent<UnityEngine.CircleCollider2D>().radius;
foreach(var b in UnityEngine.Object.FindObjectsByType<UnityEngine.MonoBehaviour>(UnityEngine.FindObjectsSortMode.None)){var n=b.GetType().Name;if(n.Contains("NarrativeController")||n=="DialogueTrigger"||n.StartsWith("PoeEvent")||n=="PoeRevealTrigger"||n=="PoeEnvironmentalReaction"){b.StopAllCoroutines();b.enabled=false;}}
foreach(var c in UnityEngine.Object.FindObjectsByType<UnityEngine.Canvas>(UnityEngine.FindObjectsSortMode.None))c.enabled=false;
UnityEngine.Application.runInBackground=true;MementoMori.Core.InputGate.Instance?.ClearAll();poe.gameObject.SetActive(true);poe.ConfigureNavigationFloor(world.navigation);
var records=new System.Collections.Generic.List<object>();int index=0,phase=0,waypoint=0;float deadline=0;var path=new System.Collections.Generic.List<UnityEngine.Vector2>();UnityEngine.Vector2 original=default,catStart=default;int collisions=0;bool onFloor=true;float start=UnityEngine.Time.realtimeSinceStartup;int firstFrame=UnityEngine.Time.frameCount;string dir=System.IO.Path.Combine(UnityEngine.Application.persistentDataPath,"Validation/ConsolidationRoutes");System.IO.Directory.CreateDirectory(dir);
System.Func<UnityEngine.Collider2D,bool> solid=c=>c&&!c.isTrigger&&!c.GetComponentInParent<MementoMori.Player.PlayerController>()&&!c.GetComponentInParent<MementoMori.Poe.PoeFollower>();
System.Func<UnityEngine.Vector2,bool> clear=p=>world.navigation.HasTile(world.navigation.WorldToCell(p))&&!UnityEngine.Physics2D.OverlapCircleAll(p,radius+.005f).Any(solid);
UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{
try{
 if(!UnityEditor.EditorApplication.isPlaying){UnityEditor.EditorApplication.update-=tick;return;}
 if(index>=world.regionIds.Length){player.ClearAutomationMoveInput();UnityEditor.EditorApplication.update-=tick;System.IO.File.WriteAllText(dir+"/"+world.gameObject.scene.name+"_route_runtime.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{playing=true,method="Actual player input along local physics-cleared grid path of at least 3.5 units, companion following; narrative disabled only for probe",radius,seconds=UnityEngine.Time.realtimeSinceStartup-start,frames=UnityEngine.Time.frameCount-firstFrame,checks=records},Newtonsoft.Json.Formatting.Indented));UnityEditor.SessionState.SetString("ConsolidationRouteProbe","Complete");return;}
 if(phase==0){
  UnityEditor.SessionState.SetString("ConsolidationRouteProbe",world.regionIds[index]);original=world.regionSpawns[index];body.position=original;player.transform.position=original;poe.transform.position=original;poe.StopAllCoroutines();poe.ConfigureNavigationFloor(world.navigation);poe.BeginFollowing();catStart=original;UnityEngine.Physics2D.SyncTransforms();collisions=0;onFloor=true;
  var floor=world.navigation;var cell=floor.WorldToCell(original);var prev=new System.Collections.Generic.Dictionary<UnityEngine.Vector3Int,UnityEngine.Vector3Int>();var q=new System.Collections.Generic.Queue<UnityEngine.Vector3Int>();prev[cell]=cell;q.Enqueue(cell);var end=cell;
  while(q.Count>0&&prev.Count<3000){var a=q.Dequeue();var ap=(UnityEngine.Vector2)floor.GetCellCenterWorld(a);if(UnityEngine.Vector2.Distance(ap,original)>=3.5f){end=a;break;}foreach(var d in new[]{UnityEngine.Vector3Int.up,UnityEngine.Vector3Int.right,UnityEngine.Vector3Int.down,UnityEngine.Vector3Int.left}){var b=a+d;if(prev.ContainsKey(b)||!floor.HasTile(b))continue;var bp=(UnityEngine.Vector2)floor.GetCellCenterWorld(b);if(!clear(bp))continue;var delta=bp-ap;if(UnityEngine.Physics2D.CircleCastAll(ap,radius,delta.normalized,delta.magnitude).Any(h=>solid(h.collider)))continue;prev[b]=a;q.Enqueue(b);}}
  path.Clear();for(var a=end;a!=cell;a=prev[a])path.Add(floor.GetCellCenterWorld(a));path.Reverse();waypoint=0;deadline=UnityEngine.Time.time+15;phase=1;return;
 }
 onFloor &= world.navigation.HasTile(world.navigation.WorldToCell(poe.transform.position));
 if(UnityEngine.Physics2D.OverlapCircleAll(poe.transform.position,.16f).Any(solid))collisions++;
 if(phase==1){
  while(waypoint<path.Count&&UnityEngine.Vector2.Distance(body.position,path[waypoint])<.10f)waypoint++;
  if(waypoint>=path.Count||UnityEngine.Time.time>deadline){player.ClearAutomationMoveInput();deadline=UnityEngine.Time.time+1.7f;phase=2;}
  else player.SetAutomationMoveInput((path[waypoint]-body.position).normalized);
 }else if(UnityEngine.Time.time>=deadline){
  var pd=UnityEngine.Vector2.Distance(original,body.position);var cd=UnityEngine.Vector2.Distance(catStart,poe.transform.position);
  records.Add(new{region=world.regionIds[index],pathFound=path.Count>0,reachedEnd=waypoint>=path.Count,playerDistance=pd,poeDistance=cd,poeStayedOnFloor=onFloor,solidContacts=collisions,separation=UnityEngine.Vector2.Distance(body.position,poe.transform.position),passed=path.Count>0&&waypoint>=path.Count&&pd>3&&cd>1&&onFloor&&collisions==0});
  UnityEngine.ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath(dir+"/"+world.regionIds[index]+"_gameplay.png"));index++;phase=0;
 }
}catch(System.Exception e){player.ClearAutomationMoveInput();UnityEditor.EditorApplication.update-=tick;System.IO.File.WriteAllText(dir+"/probe_error.txt",e.ToString());UnityEditor.SessionState.SetString("ConsolidationRouteProbe","Error");}
};
UnityEditor.EditorApplication.update+=tick;return "Physical runtime route probe queued";


