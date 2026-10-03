using MementoMori.Core;
using MementoMori.Interaction;
using UnityEngine;

namespace MementoMori.World
{
    public sealed class GalleryCycleChoice : MonoBehaviour, IInteractable
    {
        private const string ProgressKey = "moon.gallery.cycle";
        [SerializeField] private int phaseIndex;
        [SerializeField] private GameObject hiddenWall;

        public string InteractionVerb => phaseIndex switch
        {
            0 => "Observar Crescente",
            1 => "Observar Cheia",
            _ => "Observar Minguante"
        };
        public int InteractionPriority => 8;
        public bool CanInteract(InteractionContext context) =>
            GameState.Instance != null && GameState.Instance.GetPuzzleProgress("moon.illusory_corridor") >= 3
            && GameState.Instance.GetPuzzleProgress(ProgressKey) < 3;

        public void Configure(int index, GameObject wall)
        {
            phaseIndex = index;
            hiddenWall = wall;
        }

        private void Start()
        {
            if (GameState.Instance != null && GameState.Instance.GetPuzzleProgress(ProgressKey) >= 3)
                OpenWall();
        }

        public void Interact(InteractionContext context)
        {
            var state = GameState.Instance;
            if (state == null || !CanInteract(context)) return;
            int progress = state.GetPuzzleProgress(ProgressKey);
            if (progress >= 3) return;
            if (phaseIndex != progress)
            {
                state.SetPuzzleProgress(ProgressKey, 0);
                state.IncrementCounter("gallery.errors");
                return;
            }
            progress = state.SetPuzzleProgress(ProgressKey, progress + 1);
            if (progress == 3)
            {
                GrimoireCatalog.Discover("P10");
                GrimoireCatalog.Discover("R07");
                OpenWall();
                state.SaveCheckpoint();
            }
        }

        private void OpenWall()
        {
            if (hiddenWall == null) return;
            var collider = hiddenWall.GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;
            var renderer = hiddenWall.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.enabled = false;
        }
    }
}
