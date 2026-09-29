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
        var map = new GameObject("V3MapArt", typeof(Grid));
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
