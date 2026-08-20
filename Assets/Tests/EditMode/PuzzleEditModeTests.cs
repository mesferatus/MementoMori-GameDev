using System.Collections.Generic;
using System.Reflection;
using MementoMori.Core;
using MementoMori.Puzzles;
using NUnit.Framework;
using UnityEngine;

namespace MementoMori.Tests.EditMode
{
    public sealed class PuzzleEditModeTests
    {
        [Test]
        public void MirrorPuzzleRequiresTheThreeNarrativeCorrectMirrors()
        {
            var root = new GameObject("MirrorPuzzleTest");
            var puzzle = root.AddComponent<PuzzleMirror>();
            var symbols = new List<MirrorSymbol>();
            foreach (var id in new[] { "Present", "Delayed", "Ahead", "Absent", "Double", "Room", "Black" })
            {
                var symbolObject = new GameObject(id);
                var symbol = symbolObject.AddComponent<MirrorSymbol>();
                symbol.Configure(id, puzzle, symbolObject.AddComponent<SpriteRenderer>());
                symbols.Add(symbol);
            }
            puzzle.Configure(symbols, new[] { "Delayed", "Ahead", "Absent" }, null, null);
            InvokeStart(puzzle);

            puzzle.Activate(symbols[0]);
            Assert.That(puzzle.ErrorCount, Is.EqualTo(1));
            puzzle.Activate(symbols[1]);
            puzzle.Activate(symbols[2]);
            puzzle.Activate(symbols[3]);
            Assert.That(puzzle.State, Is.EqualTo(PuzzleMirror.PuzzleState.Solved));

            Object.DestroyImmediate(root);
            foreach (var symbol in symbols) Object.DestroyImmediate(symbol.gameObject);
        }

        [Test]
        public void SigilRingPuzzleAcceptsTheThreeNarrativeValuesAfterAnError()
        {
            var root = new GameObject("SigilRingPuzzleTest");
            var state = GameState.Instance;
            if (state == null) state = root.AddComponent<GameState>();
            SetGameStateInstance(state);
            state.StartNewGame();
            state.SetFlag(StoryFlag.MirrorPuzzleComplete);
            var puzzle = root.AddComponent<SigilRingPuzzle>();
            InvokeStart(puzzle);

            Assert.That(puzzle.SetRing(SigilRing.Phase, "Nova"), Is.False);
            Assert.That(puzzle.SetRing(SigilRing.Phase, "Minguante"), Is.True);
            Assert.That(puzzle.GetProgress(), Is.EqualTo(1));
            Assert.That(puzzle.SetRing(SigilRing.Memory, "Grimório"), Is.True);
            Assert.That(puzzle.GetProgress(), Is.EqualTo(2));
            Assert.That(puzzle.SetRing(SigilRing.Intention, "SUSTENTAR"), Is.True);
            Assert.That(puzzle.Solved, Is.True);
            Assert.That(state.HasFlag(StoryFlag.SigilPuzzleComplete), Is.True);

            Object.DestroyImmediate(root);
        }

        private static void InvokeStart(MonoBehaviour component)
        {
            component.GetType().GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(component, null);
        }

        private static void SetGameStateInstance(GameState state)
        {
            typeof(GameState).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, state);
        }
    }
}
