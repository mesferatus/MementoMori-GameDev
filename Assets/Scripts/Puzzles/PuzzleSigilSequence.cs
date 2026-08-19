using System.Collections;
using System.Collections.Generic;
using MementoMori.Core;
using MementoMori.World;
using UnityEngine;
using UnityEngine.Events;

namespace MementoMori.Puzzles
{
    public sealed class PuzzleSigilSequence : MonoBehaviour
    {
        [SerializeField] private List<string> expectedSequence = new() { "Moon", "Eye", "Spiral" };
        [SerializeField] private List<SigilPart> parts = new();
        [SerializeField] private DoorController lunarSeal;
        [SerializeField] private HintController hintController;
        [SerializeField, Min(0f)] private float resetDelay = 0.5f;
        [SerializeField] private UnityEvent onStepCorrect;
        [SerializeField] private UnityEvent onSequenceFailed;
        [SerializeField] private UnityEvent onSolved;
        private int currentIndex;
        private bool inputLocked;
        public PuzzleState State { get; private set; } = PuzzleState.NotStarted;
        public int ErrorCount { get; private set; }
        public bool CanAcceptInput => State == PuzzleState.Active && !inputLocked;

        public void Configure(IList<SigilPart> configuredParts, IList<string> sequence, DoorController door, HintController hints)
        {
            parts = configuredParts == null ? new List<SigilPart>() : new List<SigilPart>(configuredParts);
            expectedSequence = sequence == null ? new List<string>() : new List<string>(sequence);
            lunarSeal = door;
            hintController = hints;
        }

        private void Start()
        {
            State = expectedSequence.Count == 0 ? PuzzleState.Disabled
                : GameState.Instance != null && GameState.Instance.HasFlag(StoryFlag.SigilPuzzleComplete) ? PuzzleState.Solved : PuzzleState.Active;
            if (State == PuzzleState.Solved) lunarSeal?.Open();
            hintController?.Begin();
        }
        public void Activate(SigilPart part)
        {
            if (!CanAcceptInput || part == null) return;
            inputLocked = true;
            if (part.PartId == expectedSequence[currentIndex])
            {
                part.SetActive(true);
                currentIndex++;
                onStepCorrect?.Invoke();
                if (currentIndex == expectedSequence.Count) { Solve(); return; }
                inputLocked = false;
                return;
            }
            ErrorCount++;
            onSequenceFailed?.Invoke();
            hintController?.RegisterError(ErrorCount);
            if (Application.isPlaying)
                StartCoroutine(ResetRoutine());
            else
                ResetSequence();
        }
        private IEnumerator ResetRoutine()
        {
            yield return new WaitForSeconds(resetDelay);
            ResetSequence();
        }
        private void ResetSequence()
        {
            foreach (var part in parts) part?.SetActive(false);
            currentIndex = 0;
            inputLocked = false;
        }
        private void Solve()
        {
            if (State == PuzzleState.Solved) return;
            State = PuzzleState.Solved;
            lunarSeal?.Open();
            GameState.Instance?.SetSigilPuzzleSolved();
            GameState.Instance?.SetFlag(StoryFlag.SigilPuzzleComplete);
            GameState.Instance?.SetFlag(StoryFlag.MoonDomainUnlocked);
            GameState.Instance?.SaveCheckpoint();
            onSolved?.Invoke();
        }
    }
}
