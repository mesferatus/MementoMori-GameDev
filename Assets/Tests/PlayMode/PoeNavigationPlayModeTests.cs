using System.Collections;
using MementoMori.Poe;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

public sealed class PoeNavigationPlayModeTests
{
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

