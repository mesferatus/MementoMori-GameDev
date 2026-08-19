using MementoMori.Core;
using MementoMori.Dialogue;
using UnityEngine;
using MementoMori.Audio;

namespace MementoMori.Poe
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class PoeRevealTrigger : MonoBehaviour
    {
        [SerializeField] private PoeFollower poe;
        [SerializeField] private DialogueData revealDialogue;
        [SerializeField] private bool oneShot = true;
        private bool used;

        public void Configure(PoeFollower follower, DialogueData dialogue, bool isOneShot)
        {
            poe = follower;
            revealDialogue = dialogue;
            oneShot = isOneShot;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player") || used && oneShot)
                return;

            used = true;
            if (poe != null)
            {
                RuntimeAudio.PlayOneShot("14_poe_appear");
                poe.Reveal();
                poe.BeginFollowing();
            }
            GameState.Instance?.SetPoeRevealed();
            if (revealDialogue != null)
                DialogueManager.Instance?.StartDialogue(revealDialogue);
        }
    }
}
