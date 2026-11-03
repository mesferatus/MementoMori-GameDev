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

        private void Awake()
        {
            revealDialogue ??= Resources.Load<DialogueData>("Dialogue/DLG_L_POE_REVEAL");
        }

        private void Start()
        {
            if (GameState.Instance == null || !GameState.Instance.HasFlag(StoryFlag.PoeRevealed)) return;
            used = true;
            if (poe == null) return;
            poe.gameObject.SetActive(true);
            poe.BeginFollowing();
        }

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
            if (GameState.Instance == null || !GameState.Instance.HasFlag(StoryFlag.EchoTrial03Complete))
                return;

            used = true;
            if (poe != null)
            {
                RuntimeAudio.PlayOneShot("14_poe_appear");
                poe.Reveal();
                poe.BeginFollowing();
            }
            GameState.Instance?.SetPoeRevealed();
            // LabyrinthNarrativeController queues the V6.1 C_POE scene after the reveal.
        }
    }
}
