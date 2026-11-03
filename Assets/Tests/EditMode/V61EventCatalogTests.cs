using System.Linq;
using MementoMori.Dialogue;
using MementoMori.Core;
using NUnit.Framework;
using UnityEngine;

namespace MementoMori.Tests.EditMode
{
    public sealed class V61EventCatalogTests
    {
        [Test]
        public void CatalogHasAllDistinctEventsAndLines()
        {
            var events = V61EventCatalog.Events;
            Assert.That(events.Count, Is.EqualTo(44));
            Assert.That(events.SelectMany(entry => entry.lines).Count(), Is.EqualTo(302));
            Assert.That(events.Select(entry => entry.id).Distinct().Count(), Is.EqualTo(44));
            Assert.That(events.SelectMany(entry => entry.lines).Select(line => line.id).Distinct().Count(), Is.EqualTo(302));
        }

        [Test]
        public void ConditionalFailureLineDoesNotAppearInDefaultDialogue()
        {
            var entry = V61EventCatalog.Events.First(e => e.lines.Any(line => line.condition == "on_wrong_candle_order"));
            var conditional = entry.lines.First(line => line.condition == "on_wrong_candle_order");
            var ordinary = V61EventCatalog.CreateDialogue(entry.id);
            var failure = V61EventCatalog.CreateDialogue(entry.id, "on_wrong_candle_order");
            try
            {
                Assert.That(ordinary.Lines.Any(line => line.Text == conditional.text), Is.False);
                Assert.That(failure.Lines.Any(line => line.Text == conditional.text), Is.True);
            }
            finally { Object.DestroyImmediate(ordinary); Object.DestroyImmediate(failure); }
        }

        [Test]
        public void MoonGateUnlockedDialogueOmitsLockedWarning()
        {
            var dialogue = V61EventCatalog.CreateDialogue("L08", "on_gate_unlocked");
            try
            {
                Assert.That(dialogue.Lines.Any(line => line.Text.Contains("Ainda faltam marcas")), Is.False);
                Assert.That(dialogue.Lines.Any(line => line.Text.Contains("Ecos, a porta")), Is.True);
            }
            finally { Object.DestroyImmediate(dialogue); }
        }

        [Test]
        public void RoomPhotoSegmentContainsOnlyItsOwnObservation()
        {
            var dialogue = V61EventCatalog.CreateDialogueSegment("Q01", "Q01.Photo", 4, 2);
            try
            {
                Assert.That(dialogue.SequenceId, Is.EqualTo("Q01.Photo"));
                Assert.That(dialogue.Lines, Has.Length.EqualTo(2));
                Assert.That(dialogue.Lines[0].Text, Does.Contain("Fotografia"));
                Assert.That(dialogue.Lines[0].Id, Is.EqualTo("Q01_D005"));
                Assert.That(dialogue.Lines.Any(line => line.Text.Contains("Escrivaninha")), Is.False);
            }
            finally { Object.DestroyImmediate(dialogue); }
        }

        [Test]
        public void ChoiceIsAppliedOnceAndSignedNarrativeValueSurvivesSave()
        {
            var root = new GameObject("V61ChoiceTest");
            try
            {
                var state = root.AddComponent<GameState>();
                Assert.That(V61EventCatalog.Choices.Count, Is.EqualTo(9));
                Assert.That(V61EventCatalog.TryApplyChoice(state, "CH_A1", "challenge", out var reply), Is.True);
                Assert.That(reply, Is.Not.Empty);
                Assert.That(state.GetNarrativeValue("confianca_andrealphus"), Is.EqualTo(-1));
                Assert.That(state.GetNarrativeValue("discernimento"), Is.EqualTo(1));
                Assert.That(V61EventCatalog.TryApplyChoice(state, "CH_A1", "trust", out _), Is.False);

                var snapshot = state.CapturePersistentState("Quarto", Vector2.zero);
                state.StartNewGame();
                Assert.That(state.RestorePersistentState(snapshot), Is.True);
                Assert.That(state.GetNarrativeValue("confianca_andrealphus"), Is.EqualTo(-1));
                Assert.That(state.GetCounter("choice.CH_A1"), Is.EqualTo(2));
                Assert.That(state.GetCounter("choice.CH_A1.option.challenge"), Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(root); PlayerPrefs.DeleteKey("MementoMori.Checkpoint"); }
        }
    }
}
