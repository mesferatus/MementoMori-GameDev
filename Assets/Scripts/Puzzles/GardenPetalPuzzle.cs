using UnityEngine;
using MementoMori.Core;
using MementoMori.Poe;
using MementoMori.Audio;
using MementoMori.Dialogue;

namespace MementoMori.Puzzles
{
    public enum MoonPetal { Crescente, Cheia, Minguante }

    public sealed class GardenPetalPuzzle : MonoBehaviour
    {
        [SerializeField] MoonPetal petal;
        [SerializeField] string requiredTarget;
        [SerializeField] Transform player;
        [SerializeField] PoeFollower poe;
        [SerializeField] float crescentDistance = 3.25f;
        [SerializeField] float crescentOpenDelay = 1.5f;
        [SerializeField] Vector2 fullReflectionPosition;
        [SerializeField] float fullAlignmentTolerance = .45f;
        float crescentTimer;
        bool crescentOpened;
        bool crescentHintRequested;
        int nextWaningFlower = 2;
        public MoonPetal Petal => petal;
        public int Attempts { get; private set; }
        public int Errors { get; private set; }
        public bool Solved => GameState.Instance != null && GameState.Instance.HasFlag(FlagFor(petal));
        public void Configure(MoonPetal kind, string target) { petal = kind; requiredTarget = target; }
        public void ConfigureCrescentRule(Transform playerTransform, PoeFollower poeFollower)
        {
            player = playerTransform; poe = poeFollower;
        }
        public void ConfigureFullReflection(Vector2 reflectedPosition) => fullReflectionPosition = reflectedPosition;
        public bool CanCollect(Transform interactor)
        {
            if (Solved) return false;
            if (petal == MoonPetal.Crescente) return crescentOpened;
            if (petal == MoonPetal.Cheia) return interactor != null && Vector2.Distance(interactor.position, fullReflectionPosition) <= fullAlignmentTolerance;
            return false;
        }

        void Update()
        {
            if (petal != MoonPetal.Crescente || Solved || crescentOpened) return;
            player ??= GameObject.FindGameObjectWithTag("Player")?.transform;
            if (poe == null)
            {
                var followers = Object.FindObjectsByType<PoeFollower>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (followers.Length > 0) poe = followers[0];
            }
            if (poe != null && !poe.gameObject.activeInHierarchy && GameState.Instance != null && GameState.Instance.PoeRevealed)
            {
                poe.Reveal();
                poe.BeginFollowing();
            }
            if (player == null) return;
            if (Vector2.Distance(player.position, transform.position) < crescentDistance)
            {
                crescentTimer = 0f;
                return;
            }
            if (!crescentHintRequested && poe != null)
            {
                crescentHintRequested = true;
                poe.HintAt(transform.position);
            }
            if (poe != null && Vector2.Distance(poe.transform.position, transform.position) < 1.15f)
                crescentTimer += Time.deltaTime;
            if (crescentTimer >= crescentOpenDelay)
            {
                crescentOpened = true;
            }
        }

        public bool Place(string target, Transform interactor = null)
        {
            if (Solved) return false;
            if (petal != MoonPetal.Minguante && !CanCollect(interactor)) return false;
            Attempts++;
            if (target != requiredTarget)
            {
                Errors++;
                GameState.Instance?.IncrementCounter("garden." + petal + ".errors");
                RuntimeAudio.PlayOneShot("10_sigil_error", .35f);
                if (Errors >= 4) Object.FindAnyObjectByType<PoeFollower>()?.Inspect();
                return false;
            }
            GameState.Instance?.SetFlag(FlagFor(petal));
            PlayPetalDialogue();
            if (GameState.Instance != null && GameState.Instance.HasFlag(StoryFlag.GardenPetalCrescente) && GameState.Instance.HasFlag(StoryFlag.GardenPetalCheia) && GameState.Instance.HasFlag(StoryFlag.GardenPetalMinguante))
            { GameState.Instance.SetFlag(StoryFlag.GardenComplete); StoryProgression.Instance?.SaveCheckpoint(CheckpointId.Garden); }
            return true;
        }
        public bool ExtinguishWaning(int flowerIndex)
        {
            if (petal != MoonPetal.Minguante || Solved) return false;
            Attempts++;
            if (flowerIndex != nextWaningFlower)
            {
                Errors++;
                GameState.Instance?.IncrementCounter("garden." + petal + ".errors");
                RuntimeAudio.PlayOneShot("10_sigil_error", .35f);
                if (Errors >= 4) Object.FindAnyObjectByType<PoeFollower>()?.Inspect();
                return false;
            }
            nextWaningFlower--;
            if (nextWaningFlower >= 1) return true;
            return Place(requiredTarget);
        }
        private void PlayPetalDialogue()
        {
            var key = petal switch
            {
                MoonPetal.Crescente => "DLG_GARDEN_CRESCENT",
                MoonPetal.Cheia => "DLG_GARDEN_FULL",
                _ => "DLG_GARDEN_WANING"
            };
            DialogueManager.Instance?.StartDialogue(Resources.Load<DialogueData>("Dialogue/" + key));
        }
        static StoryFlag FlagFor(MoonPetal p) => p switch { MoonPetal.Crescente => StoryFlag.GardenPetalCrescente, MoonPetal.Cheia => StoryFlag.GardenPetalCheia, _ => StoryFlag.GardenPetalMinguante };
    }
}
