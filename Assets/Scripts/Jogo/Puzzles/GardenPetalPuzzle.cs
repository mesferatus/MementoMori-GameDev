using UnityEngine;
using MementoMori.Core;
using MementoMori.Poe;
using MementoMori.Audio;
using MementoMori.Dialogue;
using MementoMori.World;

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
        [SerializeField] float fullAlignmentTolerance = .6f;
        float crescentTimer;
        bool crescentOpened;
        bool crescentHintRequested;
        bool crescentApproached;
        int nextWaningFlower = 2;
        public MoonPetal Petal => petal;
        public Vector2 FullReflectionPosition => fullReflectionPosition;
        public int NextWaningFlower => nextWaningFlower;
        public int Attempts { get; private set; }
        public int Errors { get; private set; }
        public bool Solved => GameState.Instance != null && GameState.Instance.HasFlag(FlagFor(petal));
        public void Configure(MoonPetal kind, string target) { petal = kind; requiredTarget = target; }
        public void ConfigureCrescentRule(Transform playerTransform, PoeFollower poeFollower)
        {
            player = playerTransform; poe = poeFollower;
        }
        public void ConfigureFullReflection(Vector2 reflectedPosition) => fullReflectionPosition = reflectedPosition;
        public static bool IsSwimming(GameObject interactor)
        {
            var swimmer = interactor == null ? null : interactor.GetComponentInParent<MementoMori.Player.PlayerController>();
            return swimmer != null && swimmer.IsSwimming;
        }
        public bool CanCollect(Transform interactor)
        {
            if (Solved) return false;
            if (petal == MoonPetal.Crescente) return crescentOpened;
            if (petal == MoonPetal.Cheia) return interactor != null && Vector2.Distance(interactor.position, fullReflectionPosition) <= fullAlignmentTolerance;
            return false;
        }

        void Update()
        {
            if (player == null) { var tagged = GameObject.FindGameObjectWithTag("Player"); if (tagged != null) player = tagged.transform; }
            if (player != null && Vector2.Distance(player.position, transform.position) < 3.5f)
            {
                GrimoireCatalog.Discover("A09");
                GrimoireCatalog.Discover(petal switch { MoonPetal.Crescente => "P08", MoonPetal.Cheia => "P09", _ => "P10" });
            }
            if (petal != MoonPetal.Crescente || Solved || crescentOpened) return;
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
            var away = Vector2.Distance(player.position, transform.position);
            if (away < crescentDistance)
            {
                // "Quanto mais perto eu fico, menos ela responde": reaching for it first is part of the lesson.
                crescentApproached = true;
                crescentTimer = 0f;
                return;
            }
            if (!crescentApproached || away > 9f) return;
            if (!crescentHintRequested && poe != null)
            {
                crescentHintRequested = true;
                poe.HintAt(transform.position);
            }
            // P01_D005 "Era só manter distância": Poe waits on the shore (M02_WATER), so the
            // crescent opens once Melantha keeps away from it; Poe only points the way.
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
                nextWaningFlower = 2;
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
        static StoryFlag FlagFor(MoonPetal p) => p switch { MoonPetal.Crescente => StoryFlag.GardenPetalCrescente, MoonPetal.Cheia => StoryFlag.GardenPetalCheia, _ => StoryFlag.GardenPetalMinguante };
    }
}
