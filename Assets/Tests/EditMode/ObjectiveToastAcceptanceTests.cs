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
        public void LabyrinthObjectiveFollowsV61OrderAndNeverAsksToFollowHiddenPoe()
        {
            string Next() => ObjectiveToastController.ObjectiveFor("Labirinto", state);
            Assert.That(Next(), Is.EqualTo("Siga as vozes nos Corredores dos Ecos."));
            Assert.That(Next(), Does.Not.Contain("Poe"));
            state.SetFlag(StoryFlag.EchoTrial03Complete);
            Assert.That(Next(), Is.EqualTo("Deixe os Corredores dos Ecos."));
            state.SetFlag(StoryFlag.PoeRevealed);
            Assert.That(Next(), Is.EqualTo("Investigue a porta que devolve."));
            state.SetFlag(StoryFlag.FalseDoorTriggered);
            Assert.That(Next(), Is.EqualTo("Repare no que mudou na porta."));
            state.Puzzles.MarkSolved("lab.false_door");
            Assert.That(Next(), Is.EqualTo("Escute o Poço das Vozes."));
            state.SetFlag(StoryFlag.VoiceWellComplete);
            Assert.That(Next(), Is.EqualTo("Faça as sombras convergirem no Pátio sem Céu."));
            state.Puzzles.MarkSolved("lab.skyless_courtyard");
            Assert.That(Next(), Is.EqualTo("Abra o Núcleo Central."));
            state.Puzzles.MarkSolved("lab.core");
            Assert.That(Next(), Is.EqualTo("Leve as marcas ao Santuário da Lua."));
            state.SetFlag(StoryFlag.MoonDomainUnlocked);
            Assert.That(Next(), Is.EqualTo("Atravesse o portal da Lua."));
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
