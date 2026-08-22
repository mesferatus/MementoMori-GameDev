using System.IO;
using System.Reflection;
using MementoMori.Core;
using MementoMori.Dialogue;
using NUnit.Framework;
using UnityEngine;

namespace MementoMori.Tests.EditMode
{
    public sealed class MoonDomainNarrativeAcceptanceTests
    {
        [Test]
        public void C4CLoadsAllNoVisualDependencyMoonEntries()
        {
            var ids = new[]
            {
                "DLG_C4C_D01_MOON_ARRIVAL", "DLG_C4C_D04_CRESCENT_PROGRESS", "DLG_C4C_D06_FULL_HINT",
                "DLG_C4C_D07_FULL_PROGRESS", "DLG_C4C_D08_WANING_HINT", "DLG_C4C_D10_GARDEN_COMPLETE",
                "DLG_C4C_D11_MIRROR_INTRO", "DLG_C4C_D12_MIRROR_ERROR", "DLG_C4C_D13_MIRROR_PROGRESS",
                "DLG_C4C_D15_CORRIDOR_REPEAT", "DLG_C4C_D17_CORRIDOR_HINT", "DLG_C4C_D18_CORRIDOR_SUCCESS",
                "DLG_C4C_D20_SIGIL_ERROR", "DLG_C4C_D21_SIGIL_PROGRESS", "DLG_C4C_D22_SIGIL_SUCCESS"
            };

            foreach (var id in ids)
            {
                var dialogue = Resources.Load<DialogueData>("Dialogue/" + id);
                Assert.That(dialogue, Is.Not.Null, id);
                Assert.That(dialogue.Lines, Is.Not.Empty, id);
            }

            Assert.That(Resources.Load<DialogueData>("Dialogue/DLG_C4C_D01_MOON_ARRIVAL").Lines[2].Speaker, Is.EqualTo("Poe"));
            Assert.That(Resources.Load<DialogueData>("Dialogue/DLG_C4C_D22_SIGIL_SUCCESS").Lines[0].Speaker, Is.EqualTo("Poe"));
        }

        [Test]
        public void MoonSceneBindsNarrativeControllerWithoutChangingPuzzleContracts()
        {
            var scene = File.ReadAllText("Assets/Scenes/DominioLua.unity");
            Assert.That(scene, Does.Contain("MementoMori.Narrative.MoonDomainNarrativeController"));
            Assert.That(scene, Does.Contain("requiredTarget: Mirror_Ahead"));
            Assert.That(scene, Does.Contain("requiredTarget: Mirror_Absent"));
            Assert.That(scene, Does.Contain("requiredTarget: Mirror_Delayed"));
        }

        [Test]
        public void SigilFlowRemainsOrderedRecoverableAndSingleCompletion()
        {
            if (GameState.Instance != null) Object.DestroyImmediate(GameState.Instance.gameObject);
            var root = new GameObject("C4CSigilFlowTest");
            var state = root.AddComponent<GameState>();
            typeof(GameState).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, state);
            state.SetFlag(StoryFlag.MirrorPuzzleComplete);
            var puzzle = root.AddComponent<MementoMori.Puzzles.SigilRingPuzzle>();

            Assert.That(puzzle.SetRing(MementoMori.Puzzles.SigilRing.Phase, "Nova"), Is.False);
            Assert.That(puzzle.SetRing(MementoMori.Puzzles.SigilRing.Phase, "Minguante"), Is.True);
            Assert.That(puzzle.SetRing(MementoMori.Puzzles.SigilRing.Memory, "Grimório"), Is.True);
            Assert.That(puzzle.SetRing(MementoMori.Puzzles.SigilRing.Intention, "SUSTENTAR"), Is.True);
            Assert.That(state.HasFlag(StoryFlag.FragmentCollected), Is.False);
            Assert.That(puzzle.SetRing(MementoMori.Puzzles.SigilRing.Intention, "SUSTENTAR"), Is.False);
            Assert.That(state.HasFlag(StoryFlag.SigilPuzzleComplete), Is.True);
            Object.DestroyImmediate(root);
        }
    }
}
