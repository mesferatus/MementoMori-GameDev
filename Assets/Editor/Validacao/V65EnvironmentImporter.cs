using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;
public static partial class V65EnvironmentImporter {
 const string External="Tools/EnvironmentMaintenance";
 static JObject plan;
 static GameObject world;
 static float F(JToken t)=>t==null?0:(float)t;
 static Vector2 Point(float x,float y)=>new Vector2(x/64,-y/64);
 static Vector2 Point(JToken t)=>Point(F(t[0]),F(t[1]));
 static void Guard(){if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode");if(!File.Exists("Assets/Scenes/Quarto.unity"))throw new Exception("Wrong project");Directory.CreateDirectory("TestResults/EnvironmentAudit");}
 static JObject Read(string name)=>JObject.Parse(File.ReadAllText(External+"/Bundle/"+name+".json"));
 static T[] SceneComponents<T>() where T:Component=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
 static Vector2 Approach(JToken t)=>t["approach"]!=null?Point(t["approach"]):Point(F(t["x"]),F(t["y"]));
}
