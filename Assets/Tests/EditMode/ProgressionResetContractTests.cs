using System;
using MementoMori.Core;
using NUnit.Framework;
using UnityEngine;

namespace MementoMori.Tests.EditMode
{
    public sealed class ProgressionResetContractTests
    {
        [Test]
        public void NewGameClearsEveryPersistibleProgressionValue()
        {
            var root = new GameObject("ProgressionResetContract");
            var state = root.AddComponent<GameState>();

            foreach (StoryFlag flag in Enum.GetValues(typeof(StoryFlag)))
                state.SetFlag(flag);
            state.SetRitualCompleted();
            state.SetPoeRevealed();
            state.SetMirrorPuzzleSolved();
            state.SetSigilPuzzleSolved();
            state.SetFragmentCollected();
            state.SetCounter("mirror.errors", 2);
            state.SetPuzzleProgress("sigil.Phase", 1);
            state.SaveCheckpoint("DominioLua", new Vector2(4f, 5f));

            state.StartNewGame();

            foreach (StoryFlag flag in Enum.GetValues(typeof(StoryFlag)))
                Assert.That(state.HasFlag(flag), Is.False, flag.ToString());
            Assert.That(state.RitualCompleted, Is.False);
            Assert.That(state.PoeRevealed, Is.False);
            Assert.That(state.MirrorPuzzleSolved, Is.False);
            Assert.That(state.SigilPuzzleSolved, Is.False);
            Assert.That(state.FragmentCollected, Is.False);
            Assert.That(state.GetCounter("mirror.errors"), Is.Zero);
            Assert.That(state.GetPuzzleProgress("sigil.Phase"), Is.Zero);
            Assert.That(state.CheckpointScene, Is.Null);
            Assert.That(PlayerPrefs.HasKey("MementoMori.Checkpoint"), Is.False);

            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
