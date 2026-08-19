using System;
using UnityEngine;
using MementoMori.Core;
using MementoMori.World;
using MementoMori.Poe;
using MementoMori.Audio;

namespace MementoMori.Puzzles
{
    public enum SigilRing { Phase, Memory, Intention }

    /// <summary>Three independently persistent rings; incorrect input never erases other rings.</summary>
    public sealed class SigilRingPuzzle : MonoBehaviour
    {
        [SerializeField] DoorController targetDoor;
        [SerializeField] string phase = "Nova";
        [SerializeField] string memory = "Tigela";
        [SerializeField] string intention = "CHAMAR";
        public int Attempts { get; private set; }
        public int HintLevel { get; private set; }
        public bool Solved { get; private set; }
        public event Action<SigilRing, bool> RingEvaluated;

        public void Configure(DoorController door) { targetDoor = door; }
        void Start() { Solved = GameState.Instance != null && GameState.Instance.HasFlag(StoryFlag.SigilPuzzleComplete); if (Solved) targetDoor?.Open(); }
        public bool SetRing(SigilRing ring, string value)
        {
            if (Solved || string.IsNullOrEmpty(value)) return false;
            if (GameState.Instance != null && !GameState.Instance.HasFlag(StoryFlag.MirrorPuzzleComplete)) return false;
            Attempts++;
            bool ok = ring switch
            {
                SigilRing.Phase => value == "Minguante",
                SigilRing.Memory => value == "Grimório",
                SigilRing.Intention => value == "SUSTENTAR",
                _ => false
            };
            if (ok)
            {
                RuntimeAudio.PlayOneShot("09_sigil_success", .5f);
                if (StoryProgression.Instance != null) StoryProgression.Instance.SetPuzzleProgress("sigil." + ring, 1);
                RingEvaluated?.Invoke(ring, true);
                if (GetProgress() == 3) Solve();
            }
            else
            {
                RuntimeAudio.PlayOneShot("10_sigil_error", .55f);
                HintLevel = Attempts >= 8 ? 3 : Attempts >= 5 ? 2 : Attempts >= 1 ? 1 : 0;
                GameState.Instance?.IncrementCounter("sigil.errors");
                UnityEngine.Object.FindAnyObjectByType<PoeFollower>()?.ReactToError(true);
                RingEvaluated?.Invoke(ring, false);
            }
            return ok;
        }
        public int GetProgress()
        {
            if (StoryProgression.Instance == null) return 0;
            return StoryProgression.Instance.GetPuzzleProgress("sigil." + SigilRing.Phase)
                 + StoryProgression.Instance.GetPuzzleProgress("sigil." + SigilRing.Memory)
                 + StoryProgression.Instance.GetPuzzleProgress("sigil." + SigilRing.Intention);
        }
        void Solve()
        {
            if (Solved) return; Solved = true; targetDoor?.Open();
            GameState.Instance?.SetSigilPuzzleSolved(); GameState.Instance?.SetFlag(StoryFlag.SigilPuzzleComplete);
            GameState.Instance?.SetFlag(StoryFlag.MoonDomainUnlocked); StoryProgression.Instance?.SaveCheckpoint(CheckpointId.MoonDomain);
        }
    }
}
