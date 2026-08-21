using System.Collections;
using MementoMori.Core;
using MementoMori.Puzzles;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MementoMori.Tests.PlayMode
{
    public sealed class PuzzlePlayModeTests
    {
        private GameObject sessionHost;

        [SetUp]
        public void SetUpPuzzleSession()
        {
            if (GameState.Instance == null)
                sessionHost = new GameObject("PuzzleTestState");
            if (GameState.Instance == null)
                sessionHost.AddComponent<GameState>();
            GameState.Instance?.StartNewGame();
            GameState.Instance?.SetFlag(StoryFlag.GardenComplete);
        }

        [TearDown]
        public void TearDownPuzzleSession()
        {
            if (sessionHost != null) Object.Destroy(sessionHost);
        }

        [UnityTest]
        public IEnumerator MirrorPuzzleKeepsCorrectPlacementsAfterAnIncorrectMirror()
        {
            var root = new GameObject("MirrorPuzzlePlayModeTest");
            var puzzle = root.AddComponent<PuzzleMirror>();
            var names = new[] { "Present", "Delayed", "Ahead", "Absent", "Double", "Room", "Black" };
            var symbols = new MirrorSymbol[names.Length];
            for (var i = 0; i < names.Length; i++)
            {
                var symbolObject = new GameObject(names[i]);
                symbols[i] = symbolObject.AddComponent<MirrorSymbol>();
                symbols[i].Configure(names[i], puzzle, symbolObject.AddComponent<SpriteRenderer>());
            }
            puzzle.Configure(symbols, new[] { "Delayed", "Ahead", "Absent" }, null, null);
            yield return null;
            Assert.That(puzzle.State, Is.EqualTo(PuzzleMirror.PuzzleState.Active));
            puzzle.Activate(symbols[1]);
            Assert.That(symbols[1].IsActive, Is.True);
            puzzle.Activate(symbols[0]);
            Assert.That(puzzle.ErrorCount, Is.EqualTo(1));
            Assert.That(symbols[1].IsActive, Is.True);
            Object.Destroy(root);
            foreach (var symbol in symbols) Object.Destroy(symbol.gameObject);
        }

        [UnityTest]
        public IEnumerator SigilRingPuzzleKeepsSolvedRingsAfterAnIncorrectInput()
        {
            var root = new GameObject("SigilRingPuzzlePlayModeTest");
            GameState.Instance?.SetFlag(StoryFlag.MirrorPuzzleComplete);
            var puzzle = root.AddComponent<SigilRingPuzzle>();
            yield return null;
            Assert.That(puzzle.SetRing(SigilRing.Phase, "Nova"), Is.False);
            Assert.That(puzzle.SetRing(SigilRing.Phase, "Minguante"), Is.True);
            Assert.That(puzzle.GetProgress(), Is.EqualTo(1));
            Assert.That(puzzle.SetRing(SigilRing.Memory, "Grimório"), Is.True);
            Assert.That(puzzle.GetProgress(), Is.EqualTo(2));
            Object.Destroy(root);
        }
    }
}
