using MementoMori.Core;
using MementoMori.Dialogue;
using NUnit.Framework;
using UnityEngine;

namespace MementoMori.Tests.EditMode
{
    public sealed class FinalBetaAcceptanceTests
    {
        [Test]
        public void FinalBetaContentLoadsTheFiveNoVisualDependencyEntries()
        {
            var ids = new[] { "FINALBETA_F01_RECOGNIZE", "FINALBETA_F02_MELANTHA", "FINALBETA_F03_POE", "FINALBETA_F05_CONTINUITY", "FINALBETA_F06_FORESHADOW", "FINALBETA_F07_CREDITS" };
            foreach (var id in ids)
            {
                var dialogue = Resources.Load<DialogueData>($"Dialogue/{id}");
                Assert.That(dialogue, Is.Not.Null, id);
                Assert.That(dialogue.Lines, Has.Length.EqualTo(1), id);
            }

            Assert.That(Resources.Load<DialogueData>("Dialogue/FINALBETA_F03_POE").Lines[0].Speaker, Is.EqualTo("Poe"));
            Assert.That(Resources.Load<DialogueData>("Dialogue/FINALBETA_F05_CONTINUITY").Lines[0].Text, Does.Contain("A hist").And.Contain("n\xE3o"));
        }

        [Test]
        public void NewGameResetClearsFragmentAndProgressionState()
        {
            var root = new GameObject("FinalBetaStateTest");
            var state = root.AddComponent<GameState>();
            state.SetFlag(StoryFlag.FragmentCollected);
            state.SetFlag(StoryFlag.SigilPuzzleComplete);
            state.SetFragmentCollected();
            state.SetCounter("echo", 3);

            state.StartNewGame();

            Assert.That(state.FragmentCollected, Is.False);
            Assert.That(state.HasFlag(StoryFlag.FragmentCollected), Is.False);
            Assert.That(state.HasFlag(StoryFlag.SigilPuzzleComplete), Is.False);
            Assert.That(state.GetCounter("echo"), Is.Zero);
            Object.DestroyImmediate(root);
        }
    }
}
