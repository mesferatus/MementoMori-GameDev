using MementoMori.Core;
using UnityEngine;
namespace MementoMori.Regions
{
    /// <summary>Maps existing authored puzzle outcomes into the configurable registry.</summary>
    public sealed class V6ProgressionBridge : MonoBehaviour
    {
        private void Start()
        {
            var state=GameState.Instance;
            if(gameObject.scene.name!="DominioLua" || state==null || !state.PoeRevealed)return;
            var player=FindAnyObjectByType<MementoMori.Player.PlayerController>();
            var poe=FindAnyObjectByType<MementoMori.Poe.PoeFollower>(FindObjectsInactive.Include);
            var floor=GameObject.Find("LuizaOriginalGeometry/Grid/Ground_Original_3384");
            if(player==null || poe==null)return;
            poe.gameObject.SetActive(true);
            poe.Configure(player.transform,3f,1.2f);
            if(floor!=null)poe.ConfigureNavigationFloor(floor.GetComponent<UnityEngine.Tilemaps.Tilemap>());
            poe.transform.position=player.transform.position+Vector3.left*.65f;
            poe.BeginFollowing();
        }
        private void Update()
        {
            var state=GameState.Instance;
            if(state==null) return;
            Sync(state,StoryFlag.EchoTrial03Complete,"lab.echoes");
            Sync(state,StoryFlag.GardenComplete,"moon.garden");
            Sync(state,StoryFlag.MirrorPuzzleComplete,"moon.mirrors");
            Sync(state,StoryFlag.SigilPuzzleComplete,"moon.sigil");
        }
        private static void Sync(GameState state,StoryFlag flag,string id)
        {
            if(state.Puzzles.IsKnown(id) && state.HasFlag(flag) && !state.Puzzles.GetSolved(id)) state.Puzzles.MarkSolved(id);
        }
    }
}

