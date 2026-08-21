using System.Collections;
using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MementoMori.Tests.PlayMode
{
    public sealed class UiFlowPlayModeTests
    {
        [UnityTest]
        public IEnumerator MainMenuCreditsOpenCloseWithoutDuplicatingPanel()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return null;

            var controller = Object.FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);
            var credits = FindSceneObject("Credits");

            Assert.That(controller, Is.Not.Null);
            Assert.That(credits, Is.Not.Null);
            Assert.That(CountSceneObjects("Credits"), Is.EqualTo(1));

            controller.ShowCredits();
            Assert.That(credits.activeSelf, Is.True);
            controller.HideCredits();
            Assert.That(credits.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator PauseBlocksInputAndRestoresTimeScale()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("Quarto");
            yield return null;
            Assert.That(InputGate.Instance, Is.Not.Null);
            InputGate.Instance.ClearAll();

            var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);
            Assert.That(pause, Is.Not.Null);

            pause.Toggle();
            Assert.That(Time.timeScale, Is.EqualTo(0f));
            Assert.That(InputGate.Instance, Is.Not.Null);
            Assert.That(InputGate.Instance.IsBlocked, Is.True);

            pause.Toggle();
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(InputGate.Instance.IsBlocked, Is.False);
        }

        [UnityTest]
        public IEnumerator ReturningToMenuClearsObjectiveSessionState()
        {
            yield return SceneManager.LoadSceneAsync("Quarto");
            yield return null;

            var objective = ObjectiveToastController.Instance;
            Assert.That(objective, Is.Not.Null);
            objective.ShowObjective("Objetivo temporário C5B.");
            Assert.That(objective.IsVisible, Is.True);

            GameManager.Instance.ReturnToMenu();
            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "MainMenu");
            yield return null;

            Assert.That(objective.IsVisible, Is.False);
            Assert.That(objective.CurrentObjective, Is.Empty);
            Assert.That(GameState.Instance.HasFlag(StoryFlag.GardenComplete), Is.False);
        }

        [UnityTest]
        public IEnumerator SceneUiDoesNotRemainAfterReturningToMenu()
        {
            yield return SceneManager.LoadSceneAsync("Quarto");
            yield return null;
            Assert.That(CountSceneObjects<DialogueManager>(), Is.EqualTo(1));
            Assert.That(CountSceneObjects<InteractionPromptUI>(), Is.EqualTo(1));

            GameManager.Instance.ReturnToMenu();
            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "MainMenu");
            yield return null;

            Assert.That(CountSceneObjects<DialogueManager>(), Is.EqualTo(0));
            Assert.That(CountSceneObjects<InteractionPromptUI>(), Is.EqualTo(0));
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        private static GameObject FindSceneObject(string objectName)
        {
            foreach (var candidate in Resources.FindObjectsOfTypeAll<GameObject>())
                if (candidate.name == objectName && candidate.scene.IsValid())
                    return candidate;
            return null;
        }

        private static int CountSceneObjects(string objectName)
        {
            var count = 0;
            foreach (var candidate in Resources.FindObjectsOfTypeAll<GameObject>())
                if (candidate.name == objectName && candidate.scene.IsValid()) count++;
            return count;
        }

        private static int CountSceneObjects<T>() where T : Object
        {
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        }
    }
}
