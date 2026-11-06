using System.Collections;
using MementoMori.Poe;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

public sealed class PoeNavigationPlayModeTests
{
    // Regression: the reveal trigger sat on the L01 entry path, which Melantha crosses before
    // E01 and never revisits, so Poe was never revealed. C_POE happens at the L03 Echo exit.
    [UnityTest]
    public IEnumerator LabyrinthRevealTriggerSitsAtEchoExitNotOnEntryPath()
    {
        yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Labirinto");
        yield return null;
        var trigger = Object.FindAnyObjectByType<PoeRevealTrigger>(FindObjectsInactive.Include);
        Assert.That(trigger, Is.Not.Null);
        var zone = trigger.GetComponent<Collider2D>();
        var player = GameObject.FindGameObjectWithTag("Player");
        var echo = Object.FindAnyObjectByType<MementoMori.World.EchoCorridorPuzzle>(FindObjectsInactive.Include);
        Assert.That(player, Is.Not.Null);
        Assert.That(echo, Is.Not.Null);
        Assert.That(zone.OverlapPoint(player.transform.position), Is.False, "Reveal must not sit at the labyrinth spawn.");
        const System.Reflection.BindingFlags Private = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var exit = (Vector2)typeof(MementoMori.World.EchoCorridorPuzzle).GetField("returnPosition", Private).GetValue(echo);
        Assert.That(Vector2.Distance(zone.bounds.center, exit), Is.LessThan(2.5f), "Reveal must cover the Echo corridor exit.");
        var spawn = (Transform)typeof(PoeRevealTrigger).GetField("revealSpawn", Private).GetValue(trigger);
        Assert.That(spawn, Is.Not.Null);
    }

    [UnityTest]
    public IEnumerator RevealResolvesInactiveCompanionAndRequiresCompletedEchoes()
    {
        var stateObject = new GameObject("RevealTestState");
        var cat = new GameObject("RevealTestPoe");
        var zone = new GameObject("RevealTestZone");
        var actor = new GameObject("RevealTestPlayer");
        // Isolate from any gameplay scene left loaded by earlier tests: the trigger only
        // resolves a companion authored in its own scene.
        var isolated = UnityEngine.SceneManagement.SceneManager.CreateScene("RevealTestScene");
        foreach (var go in new[] { cat, zone, actor })
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, isolated);
        var state = MementoMori.Core.GameState.Instance;
        if (state == null) state = stateObject.AddComponent<MementoMori.Core.GameState>();
        var snapshot = state.CapturePersistentState("Labirinto", Vector2.zero);
        try
        {
            state.SetFlag(MementoMori.Core.StoryFlag.EchoTrial03Complete, false);
            state.SetFlag(MementoMori.Core.StoryFlag.PoeRevealed, false);
            var follower = cat.AddComponent<PoeFollower>();
            cat.SetActive(false);
            actor.tag = "Player";
            var actorCollider = actor.AddComponent<BoxCollider2D>();
            zone.AddComponent<BoxCollider2D>().isTrigger = true;
            var trigger = zone.AddComponent<PoeRevealTrigger>();
            Assert.That(trigger, Is.Not.Null);
            zone.SendMessage("OnTriggerEnter2D", actorCollider);
            Assert.That(cat.activeSelf, Is.False);
            Assert.That(state.HasFlag(MementoMori.Core.StoryFlag.PoeRevealed), Is.False);
            state.SetFlag(MementoMori.Core.StoryFlag.EchoTrial03Complete);
            zone.SendMessage("OnTriggerEnter2D", actorCollider);
            Assert.That(cat.activeSelf, Is.True);
            Assert.That(follower.State, Is.EqualTo(PoeState.Following));
            Assert.That(state.PoeRevealed, Is.True);
            yield return null;
        }
        finally
        {
            state.RestorePersistentState(snapshot);
            Object.Destroy(zone); Object.Destroy(actor);
            Object.Destroy(cat); Object.Destroy(stateObject);
        }
        yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(isolated);
    }

    [UnityTest]
    public IEnumerator HiddenPoeDoesNotRenderUntilRevealed()
    {
        var cat = new GameObject("HiddenPoeVisualTest");
        try
        {
            var renderer = cat.AddComponent<SpriteRenderer>();
            var follower = cat.AddComponent<PoeFollower>();
            yield return null;
            Assert.That(renderer.enabled, Is.False);
            follower.Reveal();
            yield return null;
            yield return null;
            Assert.That(renderer.enabled, Is.True);
            follower.SetStoryState(PoeState.Hidden);
            yield return null;
            yield return null;
            Assert.That(renderer.enabled, Is.False);
        }
        finally { Object.Destroy(cat); }
    }

    [UnityTest]
    public IEnumerator FollowingTurnsAlongFloorInsteadOfCrossingVoid()
    {
        return FollowCorner(false);
    }

    [UnityTest]
    public IEnumerator FollowingUsesExplicitFloorWithDifferentHierarchy()
    {
        return FollowCorner(true);
    }

    [UnityTest]
    public IEnumerator InactivePoeDoesNotConsumeTheEventTrigger()
    {
        var cat = new GameObject("InactivePoe");
        var target = new GameObject("PoeEventTarget");
        var zone = new GameObject("PoeEventZone");
        var actor = new GameObject("TriggerPlayer");
        try
        {
            actor.tag = "Player";
            var actorCollider = actor.AddComponent<BoxCollider2D>();
            target.transform.position = new Vector3(4, 0, 0);
            var follower = cat.AddComponent<PoeFollower>();
            var point = target.AddComponent<PoeEventPoint>();
            var trigger = zone.AddComponent<PoeEventTrigger>();
            trigger.Configure(follower, point, true);
            cat.SetActive(false);
            zone.SendMessage("OnTriggerEnter2D", actorCollider);
            yield return null;
            Assert.AreEqual(PoeState.Hidden, follower.State);
            cat.SetActive(true);
            zone.SendMessage("OnTriggerEnter2D", actorCollider);
            Assert.AreEqual(PoeState.MovingToEventPoint, follower.State, "An early visit must not consume the one-shot event.");
            yield return null;
        }
        finally
        {
            Object.Destroy(cat); Object.Destroy(target); Object.Destroy(zone); Object.Destroy(actor);
        }
    }
    private static IEnumerator FollowCorner(bool explicitFloor)
    {
        var map = new GameObject(explicitFloor ? "LuizaOriginalGeometry" : "V3MapArt", typeof(Grid));
        var floorObject = new GameObject("Floor", typeof(Tilemap));
        floorObject.transform.SetParent(map.transform);
        var floor = floorObject.GetComponent<Tilemap>();
        var tile = ScriptableObject.CreateInstance<Tile>();
        var target = new GameObject("NavigationTarget");
        var cat = new GameObject("NavigationPoe");
        try
        {
            for (var i = 0; i < 5; i++)
            {
                floor.SetTile(new Vector3Int(i, 0, 0), tile);
                floor.SetTile(new Vector3Int(4, i, 0), tile);
            }
            cat.transform.position = new Vector3(.5f, .5f);
            target.transform.position = new Vector3(4.5f, 4.5f);
            var follower = cat.AddComponent<PoeFollower>();
            follower.Configure(target.transform, 5f, .05f);
            if (explicitFloor) follower.ConfigureNavigationFloor(floor);
            follower.BeginFollowing();
            var deadline = Time.time + 4f;
            while (Vector2.Distance(cat.transform.position, target.transform.position) > .1f && Time.time < deadline)
            {
                yield return null;
                Assert.IsTrue(floor.HasTile(floor.WorldToCell(cat.transform.position)), "Poe cut across the void at the corridor corner.");
            }
            Assert.Less(Vector2.Distance(cat.transform.position, target.transform.position), .1f, "Poe did not reach the target around the corner.");
        }
        finally
        {
            Object.Destroy(cat);
            Object.Destroy(target);
            Object.Destroy(map);
            Object.Destroy(tile);
        }
    }
}

