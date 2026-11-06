using System.Linq;
using MementoMori.Core;
using MementoMori.Dialogue;
using NUnit.Framework;
using UnityEngine;

namespace MementoMori.Tests.EditMode
{
    public sealed class V61ChoiceReactivityTests
    {
        [TestCase("CH_Q_PHOTO", "observe", "FINAL", "FINAL_D004", "Preciso olhar direito")]
        [TestCase("CH_Q_PHOTO", "keep", "FINAL", "FINAL_D004", "Ainda não sei")]
        [TestCase("CH_POE", "approach", "M07", "M07_D009", "vem cá")]
        [TestCase("CH_POE", "observe", "M07", "M07_D009", "vou esperar")]
        [TestCase("CH_A1", "trust", "A2", "A2_D008", "pedi um caminho")]
        [TestCase("CH_A1", "challenge", "A2", "A2_D008", "pedi uma resposta")]
        public void LaterReactionUsesSavedChoiceWithoutMutatingCanonicalDialogue(string choice, string option,
            string eventId, string lineId, string expected)
        {
            var root = new GameObject("ChoiceReactionTest");
            var data = V61EventCatalog.CreateDialogue(eventId);
            try
            {
                var state = root.AddComponent<GameState>();
                var original = data.Lines.Single(line => line.Id == lineId);
                Assert.That(V61ChoiceReactivity.Resolve(original, state).Text, Is.EqualTo(original.Text));
                Assert.That(V61EventCatalog.TryApplyChoice(state, choice, option, out _), Is.True);
                var saved = state.CapturePersistentState("Quarto", Vector2.zero);
                state.SetCounter("choice." + choice, 0);
                state.SetCounter("choice." + choice + ".option." + option, 0);
                Assert.That(state.RestorePersistentState(saved), Is.True);
                var resolved = V61ChoiceReactivity.Resolve(original, state);
                Assert.That(resolved.Text, Does.Contain(expected));
                Assert.That(resolved.Id, Is.EqualTo(lineId));
                Assert.That(data.Lines.Single(line => line.Id == lineId).Text, Is.EqualTo(original.Text));
                Assert.That(V61ChoiceReactivity.Resolve(data.Lines[0], state).Text, Is.EqualTo(data.Lines[0].Text));
            }
            finally { Object.DestroyImmediate(data); Object.DestroyImmediate(root); }
        }

        [Test]
        public void PhotoRecordIsChoiceSpecificAndSupportsOlderSaves()
        {
            var root = new GameObject("PhotoRecordTest");
            try
            {
                var state = root.AddComponent<GameState>();
                Assert.That(V61ChoiceReactivity.GrimoireRecord("A02", state), Is.Empty);
                state.SetCounter("choice.CH_Q_PHOTO", 1);
                Assert.That(V61ChoiceReactivity.GrimoireRecord("A02", state), Does.Contain("Olhar a foto com atenção."));
                state.SetCounter("choice.CH_Q_PHOTO", 2);
                Assert.That(V61ChoiceReactivity.GrimoireRecord("A02", state), Does.Contain("Guardar a foto."));
                Assert.That(V61ChoiceReactivity.GrimoireRecord("A01", state), Is.Empty);
                state.SetCounter("choice.CH_Q_PHOTO.option.observe", 1);
                Assert.That(V61ChoiceReactivity.SelectedOption(state, "CH_Q_PHOTO"), Is.EqualTo("observe"),
                    "Stable IDs take precedence over an older ordinal.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void M07SelectsDistinctAuthoredFramesWithoutChangingCanonicalFrames()
        {
            var root = new GameObject("PoeChoiceFramesTest");
            try
            {
                var state = root.AddComponent<GameState>();
                var sequence = Resources.Load<IllustratedSequence>("Cinematics/M07");
                Assert.That(sequence.finalArtwork, Is.False);
                foreach (var beat in sequence.beats)
                {
                    Assert.That(beat.choiceVariants, Has.Length.EqualTo(2));
                    state.SetCounter("choice.CH_POE", 0);
                    Assert.That(beat.FramesFor(state), Is.SameAs(beat.asciiFrames));
                    state.SetCounter("choice.CH_POE", 1);
                    var approach = beat.FramesFor(state);
                    state.SetCounter("choice.CH_POE", 2);
                    var observe = beat.FramesFor(state);
                    Assert.That(approach, Has.Length.EqualTo(beat.asciiFrames.Length));
                    Assert.That(observe, Is.EqualTo(beat.asciiFrames));
                    Assert.That(approach[approach.Length - 1], Is.Not.EqualTo(observe[observe.Length - 1]));
                    Assert.That(approach.Distinct().Count(), Is.GreaterThan(2));
                }
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
