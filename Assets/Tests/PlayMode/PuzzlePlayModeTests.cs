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
        public IEnumerator ConditionalPuzzleFailuresShowApprovedLinesWithoutClosingThePuzzle()
        {
            foreach (var id in new[] { "lab.skyless_courtyard", "moon.illusory_corridor" })
            {
                var root = new GameObject("ConditionalFailure_" + id);
                var puzzle = root.AddComponent<MementoMori.Regions.V6Challenge>();
                puzzle.Configure(new MementoMori.Regions.V6ChallengeDefinition { id = id });
                puzzle.Interact(default);
                var courtyard = id == "lab.skyless_courtyard";
                if (courtyard)
                    typeof(MementoMori.Regions.V6Challenge).GetMethod("ConfirmDials",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(puzzle, null);
                else
                {
                    puzzle.Choose(1);
                    puzzle.Choose(0);
                    Assert.That(GameState.Instance.GetPuzzleProgress(id + ".step"), Is.EqualTo(1));
                }
                yield return null;
                var lineId = courtyard ? "E04_D006" : "P03_D006";
                Assert.That(GameState.Instance.GetCounter("line.seen." + lineId), Is.EqualTo(1));
                Assert.That(GameState.Instance.Puzzles.GetSolved(id), Is.False);
                Assert.That(MementoMori.Regions.V6Challenge.AnyOpen, Is.True);
                var feedback = (TMPro.TMP_Text)typeof(MementoMori.Regions.V6Challenge).GetField("feedback",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(puzzle);
                Assert.That(feedback.text, Does.Contain(courtyard ? "Não, não é esse lado." : "Droga. Voltei pro começo."));
                if (!courtyard)
                {
                    puzzle.Choose(2);
                    puzzle.Choose(3);
                    var successText = feedback.text;
                    var errors = GameState.Instance.GetCounter(id + ".errors");
                    puzzle.Choose(0);
                    Assert.That(feedback.text, Is.EqualTo(successText), "A completed puzzle must not show a new failure.");
                    Assert.That(GameState.Instance.GetCounter(id + ".errors"), Is.EqualTo(errors));
                }
                puzzle.Close();
                Object.Destroy(root);
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator MirrorErrorClearsSelectionButPreservesSeenDialogueAndAllowsRetry()
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
            puzzle.Configure(symbols, new[] { "Delayed", "Ahead", "Room" }, null, null);
            yield return null;
            Assert.That(puzzle.State, Is.EqualTo(PuzzleMirror.PuzzleState.Active));
            puzzle.Activate(symbols[1]);
            Assert.That(symbols[1].IsActive, Is.True);
            GameState.Instance.SetCounter("event.M03_EXTRA.Present", 1);
            puzzle.Activate(symbols[0]);
            Assert.That(puzzle.ErrorCount, Is.EqualTo(1));
            Assert.That(symbols[1].IsActive, Is.False);
            Assert.That(GameState.Instance.GetCounter("event.M03_EXTRA.Present"), Is.EqualTo(1));
            puzzle.Activate(symbols[2]);
            Assert.That(symbols[2].IsActive, Is.False, "A retry must begin with Delayed.");
            puzzle.Activate(symbols[1]);
            puzzle.Activate(symbols[2]);
            Assert.That(symbols[1].IsActive && symbols[2].IsActive, Is.True);
            Object.Destroy(root);
            foreach (var symbol in symbols) Object.Destroy(symbol.gameObject);
        }

        [UnityTest]
        public IEnumerator WaningErrorRestartsOnlyWaningProgress()
        {
            GameState.Instance.SetFlag(StoryFlag.GardenPetalCrescente);
            var root = new GameObject("WaningRecoveryTest");
            var puzzle = root.AddComponent<GardenPetalPuzzle>();
            puzzle.Configure(MoonPetal.Minguante, "Minguante");
            yield return null;
            Assert.That(puzzle.ExtinguishWaning(2), Is.True);
            Assert.That(puzzle.ExtinguishWaning(3), Is.False);
            Assert.That(puzzle.ExtinguishWaning(1), Is.False, "An error must restart the waning sequence.");
            Assert.That(GameState.Instance.HasFlag(StoryFlag.GardenPetalCrescente), Is.True);
            Assert.That(puzzle.ExtinguishWaning(2), Is.True);
            Assert.That(puzzle.ExtinguishWaning(1), Is.True);
            Assert.That(puzzle.Solved, Is.True);
            Object.Destroy(root);
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
