using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor.SceneManagement;
using MementoMori.V65;

public static partial class V65EnvironmentImporter
{
 public static string DressLabyrinth(){
  Guard();world=GameObject.Find("V65_Environment");if(world.scene.name!="Labirinto")throw new Exception("Labyrinth only");if(world.transform.Find("CorridorVegetation"))return "Already dressed";
  plan=Read("Labirinto");var nav=world.GetComponent<V65World>().navigation;var parent=Child("CorridorVegetation",world.transform);int count=0;
  var sources=plan["assets"].ToDictionary(a=>(string)a["id"],a=>(string)(a["source_label"]??a["source"]));
  var options=plan["entries"].Where(e=>(string)e["motion"]=="anchored_wind"&&(sources[(string)e["asset"]].Contains("flores")||sources[(string)e["asset"]].Contains("arbusto"))).ToArray();
  if(options.Length==0)throw new Exception("No approved corridor vegetation assets");
  foreach(var link in plan["connections"]){var pts=link["points"].Select(p=>new Vector2((float)p[0],(float)p[1])).ToArray();for(int i=1;i<pts.Length;i++){
   var delta=pts[i]-pts[i-1];for(float along=180;along<delta.magnitude-140;along+=280){var pos=pts[i-1]+delta.normalized*along;pos+=Mathf.Abs(delta.x)>Mathf.Abs(delta.y)?new Vector2(0,58):new Vector2(54,0);float width=44,height=58;
    bool inRoom=plan["regions"].Any(r=>pos.x>(float)r["origin"][0]-64&&pos.x<(float)r["origin"][0]+(float)r["size"][0]+64&&pos.y>(float)r["origin"][1]-64&&pos.y<(float)r["origin"][1]+(float)r["size"][1]+64);if(inRoom)continue;
    bool clear=true;for(float y=pos.y-height;y<=pos.y;y+=8)for(float x=pos.x-width/2;x<=pos.x+width/2;x+=8)if(!nav.HasTile(nav.WorldToCell(Point(x,y))))clear=false;if(!clear)continue;
    var e=(JObject)options[count%options.Length].DeepClone();e["id"]="LabCorridorPlant_"+count;e["rect"]=new JArray(pos.x-width/2,pos.y-height,width,height);e["anchor"]=new JArray(pos.x,pos.y);e["states"]=new JArray();BuildObject(e,parent.transform);count++;
   }
  }}
  EditorSceneManager.SaveScene(world.scene);return "Approved corridor plants added: "+count;
 }
}
