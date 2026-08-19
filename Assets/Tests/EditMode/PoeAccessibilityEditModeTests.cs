using MementoMori.Core;
using MementoMori.Poe;
using NUnit.Framework;
using UnityEngine;

namespace MementoMori.Tests.EditMode
{
    public sealed class PoeAccessibilityEditModeTests
    {
        [Test]
        public void PoeSupportsAllNarrativeStatesWithoutSpeech()
        {
            var go = new GameObject("PoeStateTest");
            var poe = go.AddComponent<PoeFollower>();
            poe.Reveal();
            poe.BeginFollowing();
            poe.Lead(); Assert.That(poe.State, Is.EqualTo(PoeState.Leading));
            poe.SetStoryState(PoeState.Waiting); Assert.That(poe.State, Is.EqualTo(PoeState.Waiting));
            poe.Refuse(); Assert.That(poe.State, Is.EqualTo(PoeState.Refusing));
            poe.Frighten(); Assert.That(poe.State, Is.EqualTo(PoeState.Frightened));
            poe.Inspect(); Assert.That(poe.State, Is.EqualTo(PoeState.Inspecting));
            poe.Mirror(); Assert.That(poe.State, Is.EqualTo(PoeState.Mirrored));
            poe.Dissolve(); Assert.That(poe.State, Is.EqualTo(PoeState.Dissolving));
            Assert.That(go.GetComponents<AudioSource>(), Is.Empty);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void AccessibilitySettingsClampUserFacingValues()
        {
            var go = new GameObject("AccessibilitySettingsTest");
            var settings = go.AddComponent<AccessibilitySettings>();
            settings.SetFontScale(99f);
            settings.SetTextSpeed(-1f);
            settings.SetMusicVolume(2f);
            settings.SetEffectsVolume(-1f);
            Assert.That(settings.FontScale, Is.EqualTo(1.6f));
            Assert.That(settings.TextSpeed, Is.EqualTo(.25f));
            Assert.That(settings.MusicVolume, Is.EqualTo(1f));
            Assert.That(settings.EffectsVolume, Is.EqualTo(0f));
            Object.DestroyImmediate(go);
        }
    }
}
