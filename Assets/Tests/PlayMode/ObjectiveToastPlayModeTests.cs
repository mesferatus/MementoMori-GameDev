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
            var objectives = ObjectiveToastController.Instance;
            if (objectives == null)
            {
                root = new GameObject("ObjectiveToastPlayModeRoot");
                objectives = root.AddComponent<ObjectiveToastController>();
            }
            objectives.ShowObjective("Objetivo de teste C5A.");
            Assert.That(objectives.IsVisible, Is.True);
            yield return new WaitForSecondsRealtime(3.2f);
            Assert.That(objectives.IsVisible, Is.False);
            if (root != null) Object.Destroy(root);
        }
    }
}
