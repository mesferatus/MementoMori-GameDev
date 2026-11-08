using UnityEngine;
using MementoMori.World;
namespace MementoMori.V65
{
 public sealed class V65DoorEffect:MonoBehaviour
 {
  public DoorController door;
  ParticleSystem particles;
  void Awake(){particles=GetComponent<ParticleSystem>();}
  void Update(){if(door&&door.State==DoorState.Open&&particles&&particles.isEmitting)particles.Stop(false,ParticleSystemStopBehavior.StopEmitting);}
 }
}
