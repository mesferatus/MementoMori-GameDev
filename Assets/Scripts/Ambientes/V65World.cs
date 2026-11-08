using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using MementoMori.Player;
using MementoMori.Poe;

namespace MementoMori.V65
{
 [DefaultExecutionOrder(1000)]
 public sealed class V65World : MonoBehaviour
 {
  public Tilemap navigation;
  public Vector2 spawn;
  public Vector2[] regionSpawns;
  public string[] regionIds;
  public string sourceFingerprint;
  public int sourceRegions;
  IEnumerator Start(){
   // Rebind after legacy Start methods that refer to the old geometry.
   yield return null;
   var player=FindAnyObjectByType<PlayerController>();
   var poe=FindAnyObjectByType<PoeFollower>(FindObjectsInactive.Include);
   if(poe)poe.ConfigureNavigationFloor(navigation);
   if(player&&!navigation.HasTile(navigation.WorldToCell(player.transform.position)))player.transform.position=spawn;
  }
 }
}
