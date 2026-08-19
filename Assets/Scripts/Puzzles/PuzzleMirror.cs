using System.Collections.Generic;
using System.Linq;
using MementoMori.Core;
using MementoMori.World;
using MementoMori.Poe;
using MementoMori.Dialogue;
using UnityEngine;
using UnityEngine.Events;

namespace MementoMori.Puzzles
{
    public sealed class PuzzleMirror : MonoBehaviour
    {
        [SerializeField] private List<MirrorSymbol> symbols = new();
        [SerializeField] private List<string> correctSymbolIds = new();
        [SerializeField] private DoorController targetDoor;
        [SerializeField] private HintController hintController;
        [SerializeField] private UnityEvent onSolved;
        [SerializeField] private DialogueData completionDialogue;
        public PuzzleState State { get; private set; } = PuzzleState.NotStarted;
        public int ErrorCount { get; private set; }
        public bool CanAcceptInput => State != PuzzleState.Solved && State != PuzzleState.Disabled && (GameState.Instance == null || GameState.Instance.HasFlag(StoryFlag.GardenComplete));

        public void Configure(IList<MirrorSymbol> configuredSymbols, IList<string> solution, DoorController door, HintController hints)
        {
            symbols = configuredSymbols == null ? new List<MirrorSymbol>() : new List<MirrorSymbol>(configuredSymbols);
            correctSymbolIds = solution == null ? new List<string>() : new List<string>(solution);
            targetDoor = door;
            hintController = hints;
        }
        public void ConfigureCompletionDialogue(DialogueData dialogue) => completionDialogue = dialogue;

        private void Start()
        {
            foreach (var symbol in symbols) symbol?.SetActive(false);
            State = GameState.Instance != null && GameState.Instance.HasFlag(StoryFlag.MirrorPuzzleComplete)
                ? PuzzleState.Solved : PuzzleState.Active;
            if (State == PuzzleState.Solved) targetDoor?.Open();
            hintController?.Begin();
        }

        public void Activate(MirrorSymbol symbol)
        {
            if (!CanAcceptInput || symbol == null || symbol.IsActive || correctSymbolIds.Count == 0)
                return;
            if (!correctSymbolIds.Contains(symbol.SymbolId))
            {
                ErrorCount++;
                Object.FindAnyObjectByType<PoeFollower>()?.ReactToError(false);
                hintController?.RegisterError(ErrorCount);
                return;
            }

            symbol.SetActive(true);
            SetMirrorFlag(symbol.SymbolId);
            if (correctSymbolIds.All(id => symbols.Exists(item => item != null && item.SymbolId == id && item.IsActive)))
                Solve();
        }

        private void ClearActiveSymbols()
        {
            foreach (var symbol in symbols) symbol?.SetActive(false);
        }

        private void Solve()
        {
            if (State == PuzzleState.Solved)
                return;
            State = PuzzleState.Solved;
            GameState.Instance?.SetMirrorPuzzleSolved();
            GameState.Instance?.SetFlag(StoryFlag.MirrorPuzzleComplete);
            GameState.Instance?.SaveCheckpoint();
            StoryProgression.Instance?.SaveCheckpoint(CheckpointId.Mirrors);
            targetDoor?.Open();
            onSolved?.Invoke();
            if (completionDialogue != null)
            {
                GameState.Instance?.SetFlag(StoryFlag.AndrealphusMeeting02Complete);
                DialogueManager.Instance?.StartDialogue(completionDialogue);
            }
        }

        private static void SetMirrorFlag(string id)
        {
            var state = GameState.Instance;
            if (state == null) return;
            switch (id)
            {
                case "Present": state.SetFlag(StoryFlag.MirrorPresentSolved); break;
                case "Delayed": state.SetFlag(StoryFlag.MirrorDelayedSolved); break;
                case "Ahead": state.SetFlag(StoryFlag.MirrorAheadSolved); break;
                case "Absent": state.SetFlag(StoryFlag.MirrorAbsentSolved); break;
                case "Double": state.SetFlag(StoryFlag.MirrorDoubleSolved); break;
                case "Room": state.SetFlag(StoryFlag.MirrorRoomSolved); break;
                case "Black": state.SetFlag(StoryFlag.MirrorBlackSolved); break;
            }
        }
    }
}
