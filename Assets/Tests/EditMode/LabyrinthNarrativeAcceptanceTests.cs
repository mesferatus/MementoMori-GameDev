using System.IO;
using MementoMori.Dialogue;
using NUnit.Framework;
using UnityEngine;

namespace MementoMori.Tests.EditMode
{
    public sealed class LabyrinthNarrativeAcceptanceTests
    {
        [Test]
        public void C4BLoadsAllNoVisualDependencyDialogueEntries()
        {
            var ids = new[]
            {
                "DLG_C4B_L02_POE_SIGNAL",
                "DLG_C4B_L06_ECHO_WRONG_ROUTE",
                "DLG_C4B_L07_ECHO_PROGRESS",
                "DLG_C4B_L08_ECHO_CONCLUSION",
                "DLG_C4B_L12_ANDREALPHUS_AFTER",
                "DLG_C4B_L13_POE_AFTER_ANDREALPHUS",
                "DLG_C4B_L15_PORTAL_UNLOCKED"
            };

            foreach (var id in ids)
            {
                var dialogue = Resources.Load<DialogueData>($"Dialogue/{id}");
                Assert.That(dialogue, Is.Not.Null, id);
                Assert.That(dialogue.Lines, Is.Not.Empty, id);
            }

            Assert.That(Resources.Load<DialogueData>("Dialogue/DLG_C4B_L02_POE_SIGNAL").Lines[0].Speaker, Is.EqualTo("Poe"));
            Assert.That(Resources.Load<DialogueData>("Dialogue/DLG_C4B_L12_ANDREALPHUS_AFTER").Lines[0].Speaker, Is.EqualTo("Andrealphus"));
        }

        [Test]
        public void MoonPortalRequiresEchoCompletionBeforeDominioLua()
        {
            var scene = File.ReadAllText("Assets/Scenes/Labirinto.unity");
            Assert.That(scene, Does.Contain("requiredStoryFlags:\n  - EchoTrial03Complete"));
            Assert.That(scene, Does.Contain("MementoMori.Narrative.LabyrinthNarrativeController"));
        }
    }
}
