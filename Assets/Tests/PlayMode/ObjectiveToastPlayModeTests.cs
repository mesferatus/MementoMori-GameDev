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
        public IEnumerator ObjectiveAutomaticallyHidesAfterShortDuration()
        {
            root = new GameObject("ObjectiveToastPlayModeRoot");
            var objectives = root.AddComponent<ObjectiveToastController>();
            objectives.ShowObjective("Explore o quarto.");
            Assert.That(objectives.IsVisible, Is.True);
            yield return new WaitForSecondsRealtime(3.2f);
            Assert.That(objectives.IsVisible, Is.False);
            Object.Destroy(root);
        }
    }
}
