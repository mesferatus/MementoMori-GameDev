using MementoMori.Interaction;
using MementoMori.Puzzles;
using UnityEngine;
using MementoMori.Audio;
using MementoMori.Core;

namespace MementoMori.World
{
    public sealed class SigilRingInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] SigilRingPuzzle puzzle;
        [SerializeField] SigilRing ring;
        [SerializeField] string[] values;
        int index;
        public SigilRing Ring => ring;
        public string CurrentValue => values != null && values.Length > 0 ? values[index] : string.Empty;
        public string InteractionVerb => Loc.T(ring == SigilRing.Phase ? "Anel de fases"
            : ring == SigilRing.Memory ? "Anel de memória" : "Anel de intenção") + ": " + Loc.T(CurrentValue);
        public int InteractionPriority => 8;
        public bool CanInteract(InteractionContext context) => puzzle != null && !puzzle.Solved
            && (GameState.Instance == null || GameState.Instance.GetPuzzleProgress("sigil." + ring) == 0);
        public void Configure(SigilRingPuzzle target, SigilRing kind, string[] options) { puzzle = target; ring = kind; values = options; }
        public void Interact(InteractionContext context)
        {
            if (!CanInteract(context) || values == null || values.Length == 0) return;
            index = (index + 1) % values.Length;
            RuntimeAudio.PlayOneShot("08_sigil_hover_loop", .25f);
            puzzle.SetRing(ring, values[index]);
        }
    }
}
