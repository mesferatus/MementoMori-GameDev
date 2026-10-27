using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MementoMori.V65
{
 public sealed class V65Ambient : MonoBehaviour
 {
  public Sprite[] frames;
  public float fps=8, phase;
  public Light2D localLight;
  public float baseIntensity=.5f;
  public bool floating;
  SpriteRenderer visual; Vector3 origin;
  void Awake(){visual=GetComponent<SpriteRenderer>();origin=transform.localPosition;}
  void Update(){
   if(visual && visual.isVisible && frames!=null && frames.Length>0)visual.sprite=frames[Mathf.FloorToInt((Time.time+phase)*fps)%frames.Length];
   if(localLight) localLight.intensity=baseIntensity*(.94f+.04f*Mathf.Sin(Time.time*3.7f+phase)+.02f*Mathf.Sin(Time.time*7.3f));
   if(floating)transform.localPosition=origin+Vector3.up*(Mathf.Sin(Time.time*.9f+phase)*.035f);
  }
 }
}
