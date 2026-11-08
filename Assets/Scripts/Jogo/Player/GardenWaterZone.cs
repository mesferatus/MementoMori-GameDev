using UnityEngine;

namespace MementoMori.Player
{
    /// <summary>Uses the authored lake sprite's alpha outline as a swim region without changing map geometry.</summary>
    public sealed class GardenWaterZone : MonoBehaviour
    {
        private static GardenWaterZone instance;
        private PolygonCollider2D area;

        public static GardenWaterZone Ensure()
        {
            if (!Application.isPlaying) return null;
            if (instance != null) return instance;
            var lake = GameObject.Find("M02_layer_agua");
            if (lake == null || lake.GetComponent<SpriteRenderer>()?.sprite == null) return null;
            var zone = lake.GetComponent<GardenWaterZone>();
            if (zone == null) zone = lake.AddComponent<GardenWaterZone>();
            return zone;
        }

        public static bool Contains(Vector2 position) => instance != null && instance.area != null
            && instance.area.OverlapPoint(position);

        private void Awake()
        {
            instance = this;
            area = GetComponent<PolygonCollider2D>();
            if (area == null) area = gameObject.AddComponent<PolygonCollider2D>();
            area.isTrigger = true;
        }

        private void OnDestroy() { if (instance == this) instance = null; }
    }
}
