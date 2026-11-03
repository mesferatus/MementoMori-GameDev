using System;
using System.IO;
using MementoMori.Core;
using NUnit.Framework;
using UnityEngine;

namespace MementoMori.Tests.EditMode
{
    public sealed class SaveSlotStoreTests
    {
        private string directory;
        private SaveSlotStore store;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Application.temporaryCachePath, "MementoMori-SaveTest-" + Guid.NewGuid().ToString("N"));
            store = new SaveSlotStore(directory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
            PlayerPrefs.DeleteKey("MementoMori.Checkpoint");
        }

        [Test]
        public void ManualSlotsRemainIndependentAndAutosaveHasOwnSlot()
        {
            var first = new GameState.PersistentSnapshot { scene = "Quarto", counterIds = Array.Empty<string>(),
                counterValues = Array.Empty<int>(), progressIds = Array.Empty<string>(), progressValues = Array.Empty<int>() };
            var second = new GameState.PersistentSnapshot { scene = "DominioLua", counterIds = Array.Empty<string>(),
                counterValues = Array.Empty<int>(), progressIds = Array.Empty<string>(), progressValues = Array.Empty<int>() };
            store.Save(0, first);
            store.Save(1, first);
            store.Save(2, second);
            store.Save(1, second);
            Assert.That(store.TryLoad(0, out var autosave, out _), Is.True);
            Assert.That(store.TryLoad(1, out var manual, out _), Is.True);
            Assert.That(store.TryLoad(2, out var other, out _), Is.True);
            Assert.That(autosave.scene, Is.EqualTo("Quarto"));
            Assert.That(manual.scene, Is.EqualTo("DominioLua"));
            Assert.That(other.scene, Is.EqualTo("DominioLua"));
        }

        [Test]
        public void CorruptSlotDoesNotDamageAnotherSlot()
        {
            var snapshot = new GameState.PersistentSnapshot { scene = "Labirinto" };
            store.Save(1, snapshot);
            store.Save(2, snapshot);
            File.WriteAllText(Path.Combine(directory, "slot-1.json"), "{corrupt");
            Assert.That(store.TryLoad(1, out _, out _), Is.False);
            Assert.That(store.TryLoad(2, out var intact, out _), Is.True);
            Assert.That(intact.scene, Is.EqualTo("Labirinto"));
        }

        [Test]
        public void UnsupportedSchemaIsRejected()
        {
            store.Save(1, new GameState.PersistentSnapshot { scene = "Quarto" });
            File.WriteAllText(Path.Combine(directory, "slot-1.json"), "{\"schemaVersion\":2,\"scene\":\"Quarto\"}");
            Assert.That(store.TryLoad(1, out _, out var error), Is.False);
            Assert.That(error, Does.Contain("incompatível"));
        }

        [Test]
        public void PreviousRevisionCanBeReadWhenCurrentSlotIsCorrupt()
        {
            store.Save(3, new GameState.PersistentSnapshot { scene = "Quarto" });
            store.Save(3, new GameState.PersistentSnapshot { scene = "Labirinto" });
            File.WriteAllText(Path.Combine(directory, "slot-3.json"), "{corrupt");
            Assert.That(store.TryLoad(3, out var recovered, out var message), Is.True);
            Assert.That(recovered.scene, Is.EqualTo("Quarto"));
            Assert.That(message, Does.Contain("segurança"));
        }

        [Test]
        public void PersistentStateRestoresPartialPuzzleAndDoesNotReplayCompletion()
        {
            var root = new GameObject("SaveRoundTrip");
            try
            {
                var state = root.AddComponent<GameState>();
                state.SetPuzzleProgress("lab.false_door.step", 2);
                state.SetCounter("choice.CH_Q_PHOTO", 1);
                state.SetFlag(StoryFlag.RoomGrimoireRead);
                state.Puzzles.MarkSolved("lab.echoes");
                var snapshot = state.CapturePersistentState("Labirinto", new Vector2(4, 5));
                store.Save(1, snapshot);
                state.SetPuzzleProgress("lab.false_door.step", 0);
                state.SetCounter("choice.CH_Q_PHOTO", 0);
                state.SetFlag(StoryFlag.RoomGrimoireRead, false);
                state.Puzzles.Reset();

                Assert.That(store.TryLoad(1, out var restored, out _), Is.True);
                var completionEvents = 0;
                state.Puzzles.PuzzleSolved += _ => completionEvents++;
                Assert.That(state.RestorePersistentState(restored), Is.True);
                Assert.That(state.GetPuzzleProgress("lab.false_door.step"), Is.EqualTo(2));
                Assert.That(state.GetCounter("choice.CH_Q_PHOTO"), Is.EqualTo(1));
                Assert.That(state.HasFlag(StoryFlag.RoomGrimoireRead), Is.True);
                Assert.That(state.Puzzles.GetSolved("lab.echoes"), Is.True);
                Assert.That(completionEvents, Is.Zero);
                state.SetPuzzleProgress("lab.false_door.step", 0);
                Assert.That(state.RestoreCheckpoint(), Is.True);
                Assert.That(state.GetPuzzleProgress("lab.false_door.step"), Is.EqualTo(2));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        [Test]
        public void InvalidPayloadDoesNotChangeCurrentSession()
        {
            var root = new GameObject("InvalidSaveTest");
            try
            {
                var state = root.AddComponent<GameState>();
                state.SetCounter("story.chapter", 4);
                var invalid = state.CapturePersistentState("Quarto", Vector2.zero);
                invalid.counterValues = new[] { -1 };
                Assert.That(state.RestorePersistentState(invalid), Is.False);
                Assert.That(state.GetCounter("story.chapter"), Is.EqualTo(4));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
    }
}
