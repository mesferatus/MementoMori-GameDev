using MementoMori.Interaction;
using MementoMori.Puzzles;
using UnityEngine;
using MementoMori.Audio;

namespace MementoMori.World
{
    public sealed class SigilRingInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] SigilRingPuzzle puzzle;
        [SerializeField] SigilRing ring;
        [SerializeField] string[] values;
        int index;
        public SigilRing Ring => ring;
        public string InteractionVerb => "Girar anel";
        public int InteractionPriority => 8;
        public bool CanInteract(InteractionContext context) => puzzle != null && !puzzle.Solved;
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
