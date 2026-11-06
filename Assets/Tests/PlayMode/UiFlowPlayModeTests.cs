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
        public IEnumerator ApprovedChoiceReactionAppearsInDialogueAndHistory()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return null;
            GameState.Instance.StartNewGame();
            var source = V61EventCatalog.CreateDialogue("FINAL");
            var original = source.Lines.Single(line => line.Id == "FINAL_D004");
            var probe = ScriptableObject.CreateInstance<DialogueData>();
            probe.Configure("ChoiceReactionProbe", new[] { original });
            var host = new GameObject("ChoiceReactionDialogue");
            var manager = host.AddComponent<DialogueManager>();
            try
            {
                Assert.That(V61EventCatalog.TryApplyChoice(GameState.Instance, "CH_Q_PHOTO", "keep", out _), Is.True);
                Assert.That(manager.TryStartDialogue(probe), Is.True);
                manager.Advance(); // reveal the current line immediately
                manager.Advance(); // complete and record exactly what was displayed
                Assert.That(manager.IsOpen, Is.False);
                Assert.That(manager.History.Last().Text, Is.EqualTo("A foto também. Ainda não sei o que falta nela."));
                Assert.That(manager.History.Last().Id, Is.EqualTo("FINAL_D004"));
                Assert.That(GameState.Instance.GetCounter("line.seen.FINAL_D004"), Is.EqualTo(1));
                Assert.That(probe.Lines[0].Text, Is.EqualTo(original.Text));
                Assert.That(InputGate.Instance.IsBlocked, Is.False);
            }
            finally { Object.Destroy(host); Object.Destroy(probe); Object.Destroy(source); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator IllustratedOpeningSkipPreservesBothEventsAndReleasesInput()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return null;
            GameManager.Instance.StartNewGame();
            yield return WaitForScene("Quarto");
            var deadline = Time.realtimeSinceStartup + 20f;
            var sawIllustration = false;
            while (Time.realtimeSinceStartup < deadline && GameState.Instance.GetCounter("event.C02") == 0)
            {
                var manager = DialogueManager.Instance;
                if (manager != null && manager.IsCinematic)
                {
                    if (!sawIllustration)
                    {
                        yield return null;
                        Canvas.ForceUpdateCanvases();
                        var drawing = Object.FindFirstObjectByType<AsciiIllustration>();
                        Assert.That(drawing, Is.Not.Null, "The opening must render authored ASCII.");
                        var renderer = drawing.GetComponent<CanvasRenderer>();
                        Assert.That(renderer, Is.Not.Null);
                        var mesh = renderer.GetMesh();
                        Assert.That(mesh, Is.Not.Null);
                        Assert.That(mesh.vertexCount, Is.GreaterThan(0), "The ASCII stage must produce visible character geometry.");
                    }
                    sawIllustration = true;
                    manager.SkipCinematic();
                }
                yield return null;
            }
            Assert.That(sawIllustration, Is.True, "Opening must use the illustrated presentation.");
            Assert.That(GameState.Instance.GetCounter("event.C01"), Is.EqualTo(1));
            Assert.That(GameState.Instance.GetCounter("event.C02"), Is.EqualTo(1));
            Assert.That(GameState.Instance.RitualCompleted, Is.False, "Skipping must not solve the ritual.");
            Assert.That(InputGate.Instance.IsBlocked, Is.False);
        }

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

        [UnityTest]
        public IEnumerator QuartoRitualCandleIsNotHiddenByItsOwnFootprintFromTheSouth()
        {
            yield return SceneManager.LoadSceneAsync("Quarto");
            yield return null;

            var detector = Object.FindFirstObjectByType<InteractionDetector>();
            var check = typeof(InteractionDetector).GetMethod("IsObstructed", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(detector, Is.Not.Null);
            Assert.That(check, Is.Not.Null);
            var candles = Object.FindObjectsByType<MementoMori.World.RoomCandleInteractable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Assert.That(candles.Length, Is.EqualTo(4));

            var oldPosition = detector.transform.position;
            foreach (var candle in candles)
            {
                // In front of the candle, just below its solid base: the line to the trigger crosses that base.
                detector.transform.position = candle.transform.position + Vector3.down * 1.35f;
                Physics2D.SyncTransforms();
                Assert.That((bool)check.Invoke(detector, new object[] { candle }), Is.False,
                    $"{candle.name} must not be hidden by its own drawn body or footprint.");
            }
            detector.transform.position = oldPosition;
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
