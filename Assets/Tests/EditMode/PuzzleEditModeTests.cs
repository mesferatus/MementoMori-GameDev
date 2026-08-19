using System.Collections.Generic;
using System.Reflection;
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
            Assert.That(puzzle.State, Is.EqualTo(PuzzleState.Solved));

            Object.DestroyImmediate(root);
            foreach (var symbol in symbols) Object.DestroyImmediate(symbol.gameObject);
        }

        [Test]
        public void SigilPuzzleAcceptsMoonEyeSpiralAndRejectsWrongFirstStep()
        {
            var root = new GameObject("SigilPuzzleTest");
            var puzzle = root.AddComponent<PuzzleSigilSequence>();
            var parts = new List<SigilPart>();
            foreach (var id in new[] { "Moon", "Eye", "Spiral" })
            {
                var partObject = new GameObject(id);
                var part = partObject.AddComponent<SigilPart>();
                part.Configure(id, puzzle, partObject.AddComponent<SpriteRenderer>());
                parts.Add(part);
            }
            puzzle.Configure(parts, new[] { "Moon", "Eye", "Spiral" }, null, null);
            InvokeStart(puzzle);
            puzzle.Activate(parts[1]);
            Assert.That(puzzle.ErrorCount, Is.EqualTo(1));
            puzzle.Activate(parts[0]);
            puzzle.Activate(parts[1]);
            puzzle.Activate(parts[2]);
            Assert.That(puzzle.State, Is.EqualTo(PuzzleState.Solved));

            Object.DestroyImmediate(root);
            foreach (var part in parts) Object.DestroyImmediate(part.gameObject);
        }

        private static void InvokeStart(MonoBehaviour component)
        {
            component.GetType().GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(component, null);
        }
    }
}
