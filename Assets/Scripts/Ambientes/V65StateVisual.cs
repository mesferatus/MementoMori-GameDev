using System;
using System.Collections.Generic;
using UnityEngine;
using MementoMori.Core;
using MementoMori.Puzzles;
using MementoMori.World;

namespace MementoMori.V65
{
 // Observes the existing puzzle model. Never awards progress or simulates an answer.
 public sealed class V65StateVisual : MonoBehaviour
 {
  public Sprite normal, interaction, error, complete;
  public string puzzleId, storyFlag;
  public MirrorSymbol mirror;
  public GardenPetalPuzzle garden;
  public SigilRingPuzzle sigil;
  public Portal portal;
  public ParticleSystem response;
  public int RealState {get;private set;}
  SpriteRenderer visual; MaterialPropertyBlock block; int errors, registryErrors; float errorUntil;
  void Awake(){visual=GetComponent<SpriteRenderer>();block=new MaterialPropertyBlock();}
  void OnEnable(){if(sigil)sigil.RingEvaluated+=Ring;}
  void OnDisable(){if(sigil)sigil.RingEvaluated-=Ring;}
  void Ring(SigilRing ring,bool accepted){if(!accepted)errorUntil=Time.time+1.2f;if(response)response.Play();}
  void Update(){
   if(block==null)block=new MaterialPropertyBlock();
   if(!visual)visual=GetComponent<SpriteRenderer>();
   if(!visual)return;var state=GameState.Instance;bool done=false,active=false;
   if(state!=null){if(!string.IsNullOrEmpty(puzzleId)&&state.Puzzles.IsKnown(puzzleId)){done=state.Puzzles.GetSolved(puzzleId);active=state.GetPuzzleProgress(puzzleId+".step")>0;int count=state.GetCounter(puzzleId+".errors");if(count>registryErrors){registryErrors=count;errorUntil=Time.time+1.2f;}}
    if(Enum.TryParse(storyFlag,out StoryFlag flag))done|=state.HasFlag(flag);}
   if(mirror)active=mirror.IsActive;
   if(garden){done|=garden.Solved;active=garden.CanCollect(FindAnyObjectByType<MementoMori.Player.PlayerController>()?.transform);if(garden.Errors>errors){errors=garden.Errors;errorUntil=Time.time+1.2f;}}
   if(sigil)done|=sigil.Solved;
   if(portal)active=portal.CanInteract(default);
   int next=done?3:Time.time<errorUntil?2:active?1:0;
   if(next!=RealState&&response)response.Play();RealState=next;
   var sprite=next==3?complete:next==2?error:next==1?interaction:normal;
   if(sprite && visual.sprite!=sprite){visual.sprite=sprite;RefreshSolidContour(sprite);}
   visual.GetPropertyBlock(block);block.SetFloat("_State",next==3?1:next==1?.5f:0);visual.SetPropertyBlock(block);
  }
  void RefreshSolidContour(Sprite sprite){
   var child=transform.Find("ContornoSolido");if(child==null)return;
   var solid=child.GetComponent<PolygonCollider2D>();if(solid==null||sprite.GetPhysicsShapeCount()==0)return;
   solid.pathCount=sprite.GetPhysicsShapeCount();var points=new List<Vector2>();
   for(int i=0;i<solid.pathCount;i++){
    points.Clear();sprite.GetPhysicsShape(i,points);
    for(int p=0;p<points.Count;p++)points[p]=new Vector2(visual.flipX?-points[p].x:points[p].x,visual.flipY?-points[p].y:points[p].y);
    solid.SetPath(i,points);
   }
   solid.offset=Vector2.zero;
  }
 }
}
