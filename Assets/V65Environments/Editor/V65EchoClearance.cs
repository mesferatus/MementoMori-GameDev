using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;
using MementoMori.V65;
public static partial class V65EnvironmentImporter {
 public static string RepairEchoClearance(){
  Guard();world=GameObject.Find("V65_Environment");if(world.scene.path!="Assets/V65Environments/Scenes/Labirinto.unity")throw new Exception("Lab copy required");plan=Read("Labirinto");
  var nav=world.GetComponent<V65World>().navigation;var walls=GameObject.Find("CollisionBoundary").GetComponent<Tilemap>();
  var floorTex=AssetSprite((string)plan["entries"].First(e=>(string)e["id"]=="L03_layer_piso")["asset"]).texture;var wallTex=AssetSprite((string)plan["entries"].First(e=>(string)e["id"]=="L03_layer_paredes")["asset"]).texture;
  var patches=new[]{new RectInt(3128,2080,88,24),new RectInt(2816,2120,32,80),new RectInt(2528,2440,64,24)};var changed=new List<int[]>();var ft=AssetDatabase.LoadAssetAtPath<Tile>(Root+"/Generated/NavTile.asset");
  foreach(var rect in patches)for(int y=rect.y;y<rect.yMax;y+=8)for(int x=rect.x;x<rect.xMax;x+=8){int lx=x+4-2240,ly=y+4-1530;if(floorTex.GetPixel(lx,floorTex.height-1-ly).a<.9f||wallTex.GetPixel(lx,wallTex.height-1-ly).a>.1f)continue;var cell=new Vector3Int(x/8,-y/8-1,0);if(!nav.HasTile(cell)){nav.SetTile(cell,ft);changed.Add(new[]{x,y});}}
  var occupied=new HashSet<Vector3Int>();foreach(var c in nav.cellBounds.allPositionsWithin)if(nav.HasTile(c))occupied.Add(c);var boundary=new HashSet<Vector3Int>();foreach(var c in occupied)foreach(var d in new[]{Vector3Int.up,Vector3Int.down,Vector3Int.left,Vector3Int.right})if(!occupied.Contains(c+d))boundary.Add(c+d);
  walls.ClearAllTiles();var wt=AssetDatabase.LoadAssetAtPath<Tile>(Root+"/Generated/CollisionTile.asset");walls.SetTiles(boundary.ToArray(),Enumerable.Repeat<TileBase>(wt,boundary.Count).ToArray());walls.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();walls.GetComponent<CompositeCollider2D>().GenerateGeometry();Physics2D.SyncTransforms();EditorSceneManager.SaveScene(world.scene);
  File.WriteAllText("TestResults/EnvironmentAudit/L03_clearance_repair.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{scope="Three local clearance bottlenecks; only opaque floor with transparent wall pixels; no art changed",changedCells=changed},Newtonsoft.Json.Formatting.Indented));return "Recovered floor cells "+changed.Count;
 }
}
