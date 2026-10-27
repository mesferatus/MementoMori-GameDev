using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;
public static partial class V65EnvironmentImporter {
 public static string ApplyMasonry(string only=null){
  Guard();world=GameObject.Find("V65_Environment");if(!world.scene.path.StartsWith("Assets/V65Environments/Scenes/"))throw new Exception("Integration scene required");plan=Read(world.scene.name);
  var records=JArray.Parse(File.ReadAllText("Tools/EnvironmentMaintenance/wall_segments.json"));
  var source=new Texture2D(2,2);source.LoadImage(File.ReadAllBytes("Tools/EnvironmentMaintenance/lateral.png"));
  int count=0;
  foreach(var e in plan["entries"].Where(e=>(string)e["kind"]=="paredes")){
   string rid=(string)e["region"],id=(string)e["id"];if(only!=null&&rid!=only)continue;var record=records.First(r=>(string)r["id"]==rid);if((bool?)record["garden"]==true)continue;
   var original=AssetSprite((string)e["asset"]).texture;var pixels=original.GetPixels32();var baseline=(Color32[])pixels.Clone();int w=original.width,h=original.height;
   void Put(int x,int y,Color32 c){if(x>=0&&y>=0&&x<w&&y<h)pixels[(h-y-1)*w+x]=c;}
   foreach(var side in record["sides"]){int x=(int)side[0],y0=(int)side[1],y1=(int)side[2],width=(int)side[3],end=(int)side[5];bool flip=(bool)side[4];
    for(int y=y0;y<y1;y++)for(int xx=0;xx<width;xx++)Put(x+xx,y,new Color32(0,0,0,0));
    for(int y=y0;y<end;y++)for(int xx=0;xx<32;xx++){int sx=Mathf.Clamp((flip?31-xx:xx)*source.width/32,0,source.width-1);int sy=35+(y-y0)%350*1300/350;Put(x+xx,y,source.GetPixel(sx,source.height-1-Mathf.Clamp(sy,0,source.height-1)));}
   }
   foreach(var b in record["fronts"])for(int y=Math.Max(0,(int)b[1]);y<Math.Min(h,(int)b[3]);y++)for(int x=Math.Max(0,(int)b[0]);x<Math.Min(w,(int)b[2]);x++)pixels[(h-y-1)*w+x]=baseline[(h-y-1)*w+x];
   string path=Root+"/Generated/"+rid+"_Masonry.asset";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(!texture){texture=new Texture2D(w,h,TextureFormat.RGBA32,false);AssetDatabase.CreateAsset(texture,path);}texture.SetPixels32(pixels);texture.Apply();texture.filterMode=FilterMode.Point;EditorUtility.SetDirty(texture);
   foreach(var map in world.GetComponentsInChildren<Tilemap>().Where(m=>m.name.StartsWith(id+"_"))){foreach(var cell in map.cellBounds.allPositionsWithin){var tile=map.GetTile<Tile>(cell);if(!tile||!tile.sprite)continue;var old=tile.sprite;string tilePath=AssetDatabase.GetAssetPath(tile);var replacement=AssetDatabase.LoadAllAssetsAtPath(tilePath).OfType<Sprite>().FirstOrDefault(s=>s.name=="masonry");if(!replacement){replacement=Sprite.Create(texture,old.rect,Vector2.zero,64,0,SpriteMeshType.FullRect);replacement.name="masonry";AssetDatabase.AddObjectToAsset(replacement,tile);}tile.sprite=replacement;EditorUtility.SetDirty(tile);}map.RefreshAllTiles();}count++;
  }
  UnityEngine.Object.DestroyImmediate(source);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(world.scene);return "Masonry applied to "+count+" modules; facade and gates preserved";
 }
}

