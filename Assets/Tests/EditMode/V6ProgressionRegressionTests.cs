using MementoMori.Core;
using MementoMori.World;
using NUnit.Framework;
using UnityEngine;

namespace MementoMori.Tests.EditMode
{
    public sealed class V6ProgressionRegressionTests
    {
        private GameObject root;
        private string previousCheckpoint;
        private bool hadCheckpoint;

        [SetUp]
        public void SetUp()
        {
            hadCheckpoint = PlayerPrefs.HasKey("MementoMori.Checkpoint");
            previousCheckpoint = PlayerPrefs.GetString("MementoMori.Checkpoint");
            root = new GameObject("V6Regression");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            if (hadCheckpoint) PlayerPrefs.SetString("MementoMori.Checkpoint", previousCheckpoint);
            else PlayerPrefs.DeleteKey("MementoMori.Checkpoint");
        }

        [Test]
        public void IllusoryCorridorKeepsCompletedRoundsAfterWrongAnswer()
        {
            var state = root.AddComponent<GameState>();
            var puzzle = new MementoMori.Regions.RegionPuzzleState(state, "moon.illusory_corridor",
                MementoMori.Regions.RegionOperation.Sequence, new[] { 1, 2, 3 }, 4);
            Assert.That(puzzle.Choose(1), Is.True);
            Assert.That(puzzle.Choose(0), Is.False);
            Assert.That(puzzle.Progress, Is.EqualTo(1));
            Assert.That(puzzle.Choose(2), Is.True);
            Assert.That(puzzle.Choose(3), Is.True);
            Assert.That(puzzle.Solved, Is.True);
        }

        [Test]
        public void PoeRevealRequiresCompletedEchoesWithoutConsumingEarlyVisit()
        {
            var state = root.AddComponent<GameState>();
            typeof(GameState).GetField("<Instance>k__BackingField",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).SetValue(null, state);
            root.AddComponent<BoxCollider2D>();
            var trigger = root.AddComponent<MementoMori.Poe.PoeRevealTrigger>();
            var actor = new GameObject("Player");
            try
            {
                actor.tag = "Player";
                var collider = actor.AddComponent<BoxCollider2D>();
                var enter = typeof(MementoMori.Poe.PoeRevealTrigger).GetMethod("OnTriggerEnter2D",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                enter.Invoke(trigger, new object[] { collider });
                Assert.That(state.PoeRevealed, Is.False);
                state.SetFlag(StoryFlag.EchoTrial03Complete);
                enter.Invoke(trigger, new object[] { collider });
                Assert.That(state.PoeRevealed, Is.True);
            }
            finally { Object.DestroyImmediate(actor); }
        }

        [Test]
        public void RestoreCheckpointDiscardsProgressMadeAfterSnapshot()
        {
            var state = root.AddComponent<GameState>();
            typeof(GameState).GetField("<Instance>k__BackingField",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).SetValue(null, state);
            state.SetPuzzleProgress("sigil.Phase", 1);
            state.SaveCheckpoint("DominioLua");
            state.SetPuzzleProgress("sigil.Phase", 4);
            state.SetPuzzleProgress("laterPuzzle", 3);
            Assert.That(state.RestoreCheckpoint(), Is.True);
            Assert.That(state.GetPuzzleProgress("sigil.Phase"), Is.EqualTo(1));
            Assert.That(state.GetPuzzleProgress("laterPuzzle"), Is.Zero);
        }

        [Test]
        public void ConfiguredPortalRequiresEveryActualLabyrinthPuzzle()
        {
            var state=root.AddComponent<GameState>();
            typeof(GameState).GetField("<Instance>k__BackingField",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).SetValue(null,state);
            var portal=root.AddComponent<Portal>();
            portal.Configure("DominioLua","Entrar",false);
            portal.RequireConfiguredPuzzles();
            var required=state.Puzzles.LabyrinthIds;
            for(var i=0;i<required.Count;i++)
            {
                Assert.That(portal.CanInteract(default),Is.False,"Missing "+required[i]);
                state.Puzzles.MarkSolved(required[i]);
            }
            Assert.That(portal.CanInteract(default),Is.True);
            state.SaveCheckpoint("Labirinto",new Vector2(2,3));
            state.ResetSession();
            Assert.That(portal.CanInteract(default),Is.False);
        }

        [Test]
        public void ConfiguredPuzzleCompletionSurvivesCheckpointRestore()
        {
            var state=root.AddComponent<GameState>();
            state.Puzzles.MarkSolved("lab.core");
            state.SaveCheckpoint("Labirinto",new Vector2(2,3));
            state.Puzzles.MarkSolved("lab.false_door");
            Assert.That(state.RestoreCheckpoint(),Is.True);
            Assert.That(state.Puzzles.GetSolved("lab.core"),Is.True);
            Assert.That(state.Puzzles.GetSolved("lab.false_door"),Is.False);
        }
        [Test]
        public void LinkedPortalCannotConsumeLockedPortal()
        {
            var state = root.AddComponent<GameState>();
            typeof(GameState).GetField("<Instance>k__BackingField",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).SetValue(null, state);
            var portal = root.AddComponent<Portal>();
            portal.Configure("FinalBeta", "Entrar", false);
            portal.RequireFlag(nameof(StoryFlag.FragmentCollected));
            portal.ActivateFromLinkedInteraction();
            state.SetFragmentCollected();
            Assert.That(portal.CanInteract(default), Is.True,
                "An early linked activation must not permanently consume the portal.");
        }
    }
}

