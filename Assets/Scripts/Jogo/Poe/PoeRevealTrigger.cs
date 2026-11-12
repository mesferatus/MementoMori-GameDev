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
        [Tooltip("Where Poe appears before approaching Melantha (C_POE). Optional.")]
        [SerializeField] private Transform revealSpawn;
        private bool used;
        private GameState observedState;

        private void Awake()
        {
            if (revealDialogue == null) revealDialogue = Resources.Load<DialogueData>("Dialogue/DLG_L_POE_REVEAL");
            // Poe starts inactive; resolve only the companion authored in this scene.
            if (poe == null)
                foreach (var candidate in FindObjectsByType<PoeFollower>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    if (candidate.gameObject.scene == gameObject.scene)
                    {
                        poe = candidate;
                        break;
                    }
        }

        private void Start()
        {
            observedState = GameState.Instance;
            if (observedState != null) observedState.OnFlagChanged += OnFlagChanged;
            if (GameState.Instance == null || !GameState.Instance.HasFlag(StoryFlag.PoeRevealed)) return;
            used = true;
            if (poe == null) return;
            poe.gameObject.SetActive(true);
            poe.BeginFollowing();
            // A loaded save may place Melantha far from Poe's authored spot: rejoin her.
            StartCoroutine(RejoinPlayerAfterLoad());
        }

        private void OnDestroy()
        {
            if (observedState != null) observedState.OnFlagChanged -= OnFlagChanged;
        }

        private void OnFlagChanged(StoryFlag flag, bool value)
        {
            if (flag != StoryFlag.EchoTrial03Complete || !value || !isActiveAndEnabled) return;
            // Sleeping bodies need not emit OnTriggerStay when the final answer changes state.
            // Recheck actual overlap, without granting the reveal outside its authored zone.
            var zone = GetComponent<Collider2D>();
            if (zone == null || !zone.enabled) return;
            foreach (var body in FindObjectsByType<Collider2D>(FindObjectsSortMode.None))
                if (body.enabled && body.gameObject.scene == gameObject.scene && body.CompareTag("Player")
                    && zone.Distance(body).isOverlapped)
                {
                    TryReveal(body);
                    break;
                }
        }

        private System.Collections.IEnumerator RejoinPlayerAfterLoad()
        {
            yield return null;
            yield return null;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (poe == null || player == null) yield break;
            if (Vector2.Distance(poe.transform.position, player.transform.position) > 6f)
                poe.transform.position = player.transform.position + Vector3.left * .65f;
        }

        public void Configure(PoeFollower follower, DialogueData dialogue, bool isOneShot)
        {
            poe = follower;
            revealDialogue = dialogue;
            oneShot = isOneShot;
        }

        private void OnTriggerEnter2D(Collider2D other) => TryReveal(other);

        // E01 completes while Melantha may already stand in the exit: re-check while inside.
        private void OnTriggerStay2D(Collider2D other) => TryReveal(other);

        private void TryReveal(Collider2D other)
        {
            if (!other.CompareTag("Player") || used && oneShot)
                return;
            if (GameState.Instance == null || !GameState.Instance.HasFlag(StoryFlag.EchoTrial03Complete)
                || GameState.Instance.HasFlag(StoryFlag.PoeRevealed))
                return;

            used = true;
            if (poe != null)
            {
                if (revealSpawn != null && !poe.gameObject.activeSelf)
                    poe.transform.position = revealSpawn.position;
                RuntimeAudio.PlayOneShot("14_poe_appear");
                poe.Reveal();
                poe.BeginFollowing();
            }
            GameState.Instance?.SetPoeRevealed();
            // LabyrinthNarrativeController queues the V6.1 C_POE scene after the reveal.
        }
    }
}
