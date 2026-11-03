using System.Collections;
using MementoMori.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace MementoMori.Tests.PlayMode
{
    public sealed class SwimmingPlayModeTests
    {
        [UnityTest]
        public IEnumerator MoonDomainLakeActivatesTheAuthoredSwimController()
        {
            SceneManager.LoadScene("DominioLua");
            yield return null;
            var lake = GameObject.Find("M02_layer_agua");
            var player = Object.FindAnyObjectByType<PlayerController>();
            Assert.That(lake, Is.Not.Null);
            Assert.That(player, Is.Not.Null);
            var zone = GardenWaterZone.Ensure();
            Assert.That(zone, Is.Not.Null);
            var area = lake.GetComponent<PolygonCollider2D>();
            var bounds = lake.GetComponent<SpriteRenderer>().bounds;
            Assert.That(area, Is.Not.Null);
            var waterPoint = Vector2.positiveInfinity;
            for (var x = bounds.min.x + 1f; x < bounds.max.x && float.IsInfinity(waterPoint.x); x += 1f)
                for (var y = bounds.min.y + 1f; y < bounds.max.y; y += 1f)
                    if (area.OverlapPoint(new Vector2(x, y))) { waterPoint = new Vector2(x, y); break; }
            Assert.That(float.IsInfinity(waterPoint.x), Is.False, "The authored lake must have a swimmable region.");
            var body = player.GetComponent<Rigidbody2D>();
            var animator = player.GetComponentInChildren<Animator>();
            var dryPoint = new Vector2(bounds.max.x + 2f, bounds.max.y + 2f);
            body.position = dryPoint;
            yield return null;
            Assert.That(player.IsSwimming, Is.False);
            body.position = waterPoint;
            yield return null;
            Assert.That(player.IsSwimming, Is.True);
            Assert.That(animator.runtimeAnimatorController.name, Is.EqualTo("MelanthaLunarSwim"));
            Assert.That(Vector2.Distance(player.SafeSavePosition, dryPoint), Is.LessThan(.01f));
            body.position = dryPoint;
            yield return null;
            Assert.That(player.IsSwimming, Is.False);
            Assert.That(animator.runtimeAnimatorController.name, Is.Not.EqualTo("MelanthaLunarSwim"));
        }

        [UnityTest]
        public IEnumerator EnteringAndLeavingWaterChangesMovementAndSafeSavePosition()
        {
            var texture = new Texture2D(8, 8);
            var pixels = new Color[64];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(.5f, .5f), 8);
            var lake = new GameObject("M02_layer_agua");
            lake.transform.position = new Vector3(5, 0);
            lake.AddComponent<SpriteRenderer>().sprite = sprite;
            var player = new GameObject("SwimmingTestPlayer");
            player.transform.position = Vector3.zero;
            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0;
            var controller = player.AddComponent<PlayerController>();
            yield return null;
            Assert.That(controller.IsSwimming, Is.False);

            body.position = new Vector2(5, 0);
            yield return null;
            Assert.That(controller.IsSwimming, Is.True);
            Assert.That(Vector2.Distance(controller.SafeSavePosition, Vector2.zero), Is.LessThan(.001f));
            controller.SetAutomationMoveInput(Vector2.right);
            var waterStart = body.position;
            yield return new WaitForFixedUpdate();
            var waterStep = body.position.x - waterStart.x;
            Assert.That(waterStep, Is.GreaterThan(0));

            controller.ClearAutomationMoveInput();
            body.position = new Vector2(0, 0);
            yield return null;
            Assert.That(controller.IsSwimming, Is.False);
            controller.SetAutomationMoveInput(Vector2.right);
            var dryStart = body.position;
            yield return new WaitForFixedUpdate();
            Assert.That(body.position.x - dryStart.x, Is.GreaterThan(waterStep));
            controller.ClearAutomationMoveInput();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocity.sqrMagnitude, Is.LessThan(.0001f));

            Object.Destroy(player);
            Object.Destroy(lake);
            Object.Destroy(sprite);
            Object.Destroy(texture);
        }
    }
}
