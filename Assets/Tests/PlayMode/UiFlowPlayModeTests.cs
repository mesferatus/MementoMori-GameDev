using System.Collections;
using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.Interaction;
using MementoMori.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Reflection;
using System.Linq;

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

            pause.ReturnToMenu();
            yield return WaitForScene("MainMenu");
            Assert.That(Time.timeScale, Is.EqualTo(1f));
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
            yield return WaitForScene("MainMenu");
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
            yield return WaitForScene("MainMenu");
            yield return null;

            Assert.That(CountSceneObjects<DialogueManager>(), Is.EqualTo(0));
            Assert.That(CountSceneObjects<InteractionPromptUI>(), Is.EqualTo(0));
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator QuartoInteractionSeesItsOwnPropButNotThroughAnObstacle()
        {
            yield return SceneManager.LoadSceneAsync("Quarto");
            yield return null;

            var detector = Object.FindFirstObjectByType<InteractionDetector>();
            var grimoire = Object.FindObjectsByType<MementoMori.World.GrimoireScreen>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate.gameObject.name == "Grimoire");
            var check = typeof(InteractionDetector).GetMethod("IsObstructed", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(detector, Is.Not.Null);
            Assert.That(grimoire, Is.Not.Null);
            Assert.That(check, Is.Not.Null);

            var oldPosition = detector.transform.position;
            detector.transform.position = grimoire.transform.position + Vector3.down * .9f;
            Physics2D.SyncTransforms();
            Assert.That((bool)check.Invoke(detector, new object[] { grimoire }), Is.False,
                "The physical Grimoire collider must not hide its own interaction.");

            var wall = new GameObject("TemporaryInteractionWall");
            wall.transform.position = grimoire.transform.position + Vector3.down * .45f;
            wall.AddComponent<BoxCollider2D>().size = new Vector2(.5f, .12f);
            Physics2D.SyncTransforms();
            Assert.That((bool)check.Invoke(detector, new object[] { grimoire }), Is.True,
                "A separate solid obstacle must still block interaction.");

            detector.transform.position = oldPosition;
            Object.Destroy(wall);
        }

        private static GameObject FindSceneObject(string objectName)
        {
            foreach (var candidate in Resources.FindObjectsOfTypeAll<GameObject>())
                if (candidate.name == objectName && candidate.scene.IsValid())
                    return candidate;
            return null;
        }

        private static IEnumerator WaitForScene(string sceneName)
        {
            var deadline = Time.realtimeSinceStartup + 10f;
            while (Time.realtimeSinceStartup < deadline && SceneManager.GetActiveScene().name != sceneName)
                yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(sceneName));
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
