using MementoMori.Core;
using MementoMori.UI;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;

namespace MementoMori.Tests.EditMode
{
    public sealed class ObjectiveToastAcceptanceTests
    {
        private GameObject root;
        private GameState state;
        private ObjectiveToastController objectives;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("ObjectiveToastAcceptanceRoot");
            state = root.AddComponent<GameState>();
            objectives = root.AddComponent<ObjectiveToastController>();
            typeof(ObjectiveToastController)
                .GetMethod("EnsureFallbackUi", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(objectives, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (root != null) Object.DestroyImmediate(root);
        }

        [Test]
        public void ObjectiveShowAndHideAreTransient()
        {
            objectives.ShowObjective("Explore o quarto.");
            Assert.That(objectives.IsVisible, Is.True);
            objectives.Hide();
            Assert.That(objectives.IsVisible, Is.False);
        }

        [Test]
        public void ObjectiveDoesNotDuplicateSameVisibleMessage()
        {
            objectives.ShowObjective("Siga Poe.");
            objectives.ShowObjective("Siga Poe.");
            Assert.That(objectives.ShowCount, Is.EqualTo(1));
        }

        [Test]
        public void ObjectiveFollowsRealStateTransition()
        {
            objectives.ShowObjective(ObjectiveToastController.ObjectiveFor("DominioLua", state));
            Assert.That(objectives.CurrentObjective, Is.EqualTo("Explore o domínio."));
            state.SetFlag(StoryFlag.GardenComplete);
            objectives.ShowObjective(ObjectiveToastController.ObjectiveFor("DominioLua", state));
            Assert.That(objectives.CurrentObjective, Is.EqualTo("Resolva os desafios."));
            state.SetFlag(StoryFlag.MirrorPuzzleComplete);
            objectives.ShowObjective(ObjectiveToastController.ObjectiveFor("DominioLua", state));
            Assert.That(objectives.CurrentObjective, Is.EqualTo("Complete o sigilo."));
        }

        [Test]
        public void ObjectiveDoesNotBlockGameplay()
        {
            var canvas = root.GetComponentInChildren<CanvasGroup>();
            Assert.That(canvas, Is.Not.Null);
            Assert.That(canvas.interactable, Is.False);
            Assert.That(canvas.blocksRaycasts, Is.False);
            Assert.That(InputGate.Instance == null || !InputGate.Instance.IsBlocked, Is.True);
        }
    }
}
