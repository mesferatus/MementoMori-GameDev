using System.Collections;
using System.Collections.Generic;
using MementoMori.Core;
using MementoMori.Puzzles;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MementoMori.Tests.PlayMode
{
    public sealed class PuzzlePlayModeTests
    {
        [SetUp]
        public void SetUpMirrorPuzzleSession()
        {
            GameState.Instance?.StartNewGame();
            GameState.Instance?.SetFlag(StoryFlag.GardenComplete);
        }

        [UnityTest]
        public IEnumerator MirrorPuzzleKeepsCorrectPlacementsAfterAnIncorrectMirror()
        {
            var root = new GameObject("MirrorPuzzlePlayModeTest");
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
            yield return null;
            puzzle.Activate(symbols[1]);
            Assert.That(symbols[1].IsActive, Is.True);
            puzzle.Activate(symbols[0]);
            Assert.That(puzzle.ErrorCount, Is.EqualTo(1));
            Assert.That(symbols[1].IsActive, Is.True);
            Object.Destroy(root);
            foreach (var symbol in symbols) Object.Destroy(symbol.gameObject);
        }

        [UnityTest]
        public IEnumerator SigilPuzzleResetsSequenceAfterAnIncorrectStep()
        {
            var root = new GameObject("SigilPuzzlePlayModeTest");
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
            yield return null;
            puzzle.Activate(parts[1]);
            Assert.That(puzzle.ErrorCount, Is.EqualTo(1));
            yield return new WaitForSeconds(.7f);
            Assert.That(parts[0].IsActive, Is.False);
            Assert.That(parts[1].IsActive, Is.False);
            Assert.That(puzzle.CanAcceptInput, Is.True);
            Object.Destroy(root);
            foreach (var part in parts) Object.Destroy(part.gameObject);
        }
    }
}
