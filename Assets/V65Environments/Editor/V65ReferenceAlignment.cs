using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;

public static partial class V65EnvironmentImporter {
 public static string AlignApprovedReference(){
  Guard();world=GameObject.Find("V65_Environment");if(!world||!world.scene.path.StartsWith("Assets/V65Environments/Scenes/"))throw new Exception("Isolated integration scene required");plan=Read(world.scene.name);
  int mirrors=0,rows=0;var sprites=world.GetComponentsInChildren<SpriteRenderer>(true).ToDictionary(s=>s.name,s=>s);
  foreach(var e in plan["entries"]){string id=(string)e["id"];var r=e["rect"];
   if(!id.Contains("_layer_")&&sprites.TryGetValue(id,out var sr)){
    bool flip=(bool?)e["flip"]??false;sr.flipX=flip;
    sr.transform.position=Point(F(r[0])+(flip?F(r[2]):0),F(r[1])+F(r[3]));if(flip)mirrors++;
    string kind=(string)e["kind"];if(kind=="tapete")sr.sortingOrder=-21000;else if(kind=="marcas")sr.sortingOrder=-20300;else if(kind=="sigilos")sr.sortingOrder=-20400;else if(kind=="solo")sr.sortingOrder=-22900;
   }
   if((string)e["kind"]=="agua"&&sprites.TryGetValue(id,out var water)){water.sortingOrder=-22800;water.sharedMaterial.SetFloat("_BackgroundSurface",0);}
   if(!id.Contains("_layer_")||(string)e["kind"]=="agua")continue;
   var texture=AssetSprite((string)e["asset"]).texture;
   foreach(var map in world.GetComponentsInChildren<Tilemap>().Where(m=>m.name.StartsWith(id+"_"))){
    if(!int.TryParse(map.name.Substring(id.Length+1),out int py))continue;
    int hh=Math.Min(128,texture.height-py);if(hh<=0)continue;
    map.transform.parent.position=Point(F(r[0]),F(r[1])+py+hh-128);if(hh!=128)rows++;
    string kind=(string)e["kind"];if(kind!="paredes")map.GetComponent<TilemapRenderer>().sortingOrder=kind=="escadas"?-20500:kind=="sigilos"?-20400:kind=="nenufares"?-22600:kind=="margem"?-22700:-23000;
   }
  }
  if(world.scene.name=="DominioLua"){
   var waterEntry=plan["entries"].First(e=>(string)e["id"]=="M02_layer_agua");var material=world.transform.Find("AnimatedNegativeSpace").GetComponent<MeshRenderer>().sharedMaterial;
   material.SetTexture("_MainTex",AssetSprite((string)waterEntry["asset"]).texture);material.SetFloat("_BackgroundSurface",1);UnityEditor.EditorUtility.SetDirty(material);
  }
  UnityEditor.AssetDatabase.SaveAssets();
  EditorSceneManager.SaveScene(world.scene);return "Reference alignment saved: mirrored objects "+mirrors+", partial rows "+rows;
 }
}
