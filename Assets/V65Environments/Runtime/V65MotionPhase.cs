using UnityEngine;
namespace MementoMori.V65 {
 public sealed class V65MotionPhase : MonoBehaviour {
  void OnEnable(){
   var r=GetComponent<SpriteRenderer>();if(!r||!r.sharedMaterial)return;var b=new MaterialPropertyBlock();r.GetPropertyBlock(b);
   int hash=17;foreach(char c in name)hash=unchecked(hash*31+c);b.SetFloat("_Seed",Mathf.Abs(hash%1000)*.01f);
   float mode=r.sharedMaterial.GetFloat("_Mode");if(mode>.5f&&mode<2.5f)b.SetFloat("_Strength",Mathf.Clamp((mode<1.5f?.045f:.025f)/Mathf.Max(.02f,Mathf.Abs(transform.lossyScale.x)),.02f,1f));r.SetPropertyBlock(b);
  }
 }
}
