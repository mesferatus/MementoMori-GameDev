using System.Collections;
using MementoMori.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MementoMori.Tests.PlayMode
{
    public sealed class ObjectiveToastPlayModeTests
    {
        private GameObject root;

        [UnityTest]
        public IEnumerator ObjectiveRemainsVisibleUntilExplicitlyHidden()
        {
            var objectives = ObjectiveToastController.Instance;
            if (objectives == null)
            {
                root = new GameObject("ObjectiveToastPlayModeRoot");
                objectives = root.AddComponent<ObjectiveToastController>();
            }
            yield return null; // Let Start evaluate the active scene before presenting the test objective.
            objectives.ShowObjective("Objetivo de teste C5A.");
            Assert.That(objectives.IsVisible, Is.True);
            yield return new WaitForSecondsRealtime(3.2f);
            Assert.That(objectives.IsVisible, Is.True, "R3 requires the objective to remain available for exploration and screenshots.");
            objectives.Hide();
            Assert.That(objectives.IsVisible, Is.False);
            objectives.ShowObjective("Objetivo de teste C5A.");
            Assert.That(objectives.IsVisible, Is.True, "The same objective must be able to reappear after hiding.");
            if (root != null) Object.Destroy(root);
        }
    }
}
