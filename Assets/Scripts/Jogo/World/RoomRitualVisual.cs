using System.Collections.Generic;
using MementoMori.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MementoMori.World
{
    public sealed class RoomRitualVisual : MonoBehaviour
    {
        private readonly HashSet<GameObject> styledFlames = new();
        [SerializeField] private GameObject ritualGlow;
        [SerializeField] private GameObject[] flames;
        [SerializeField] private SpriteRenderer rug;
        [SerializeField] private SpriteRenderer[] candles;
        [SerializeField] private Sprite litCandle;
        [SerializeField] private Sprite unlitCandle;
        [SerializeField] private Sprite inactiveRug;
        [SerializeField] private Sprite activeRug;
        [SerializeField] private bool returnedRoom;
        private bool initialized;
        private int lastProgress = -1;

        public void ConfigureRugStates(Sprite inactive, Sprite active)
        {
            inactiveRug = inactive; activeRug = active;
        }

        public void SetReturnedRoom(bool returned) => returnedRoom = returned;

        public void Configure(SpriteRenderer ritualRug, SpriteRenderer[] candleRenderers, Sprite off, Sprite on, bool returned)
        {
            rug = ritualRug; candles = candleRenderers; unlitCandle = off; litCandle = on; returnedRoom = returned;
            initialized = false;
        }
        private void LateUpdate()
        {
            var state = GameState.Instance;
            var active = !returnedRoom && state != null && state.RitualCompleted;
            var ritualMark = returnedRoom || active;
            var progress = !returnedRoom && state != null ? state.GetPuzzleProgress(RoomCandlePuzzle.ProgressId) : 0;
            if (rug != null)
            {
                var next = ritualMark ? activeRug : inactiveRug;
                if (next != null) rug.sprite = next;
                rug.color = next != null || active ? Color.white : new Color(.53f, .49f, .57f, 1f);
            }
            if (candles != null)
            {
                foreach (var candle in candles)
                {
                    if (candle == null) continue;
                    var order = candle.name switch
                    {
                        "CandleNorth" => 0,
                        "CandleEast" => 1,
                        "CandleSouth" => 2,
                        "CandleWest" => 3,
                        _ => 4
                    };
                    var lit = progress > order;
                    var sprite = lit ? litCandle : unlitCandle;
                    if (sprite == null) continue;
                    candle.sprite = sprite;
                    var scale = 1.25f / sprite.bounds.size.y;
                    candle.transform.localScale = new Vector3(scale, scale, 1);
                    var solid = candle.GetComponent<BoxCollider2D>();
                    if (solid != null)
                    {
                        solid.size = new Vector2(.72f / scale, .65f / scale);
                        solid.offset = new Vector2(0f, -.2f / scale);
                    }
                }
                initialized = true; lastProgress = progress;
            }
            foreach (var interactable in FindObjectsByType<RoomCandleInteractable>(FindObjectsSortMode.None))
            {
                var renderer = interactable.GetComponentInChildren<SpriteRenderer>();
                if (renderer == null || litCandle == null || unlitCandle == null) continue;
                renderer.sprite = progress > interactable.OrderIndex ? litCandle : unlitCandle;
            }
            if (ritualGlow != null) ritualGlow.SetActive(active);
            if (flames == null) return;
            for (var index = 0; index < flames.Length; index++)
            {
                var flame = flames[index];
                if (flame == null) continue;
                flame.SetActive(progress > index);
                if (flame.activeSelf) StyleFlame(flame);
            }
        }

        /// <summary>
        /// A lit flame reads as lit: larger than the authored sprite, always drawn over its own
        /// candlestick, and casting a small warm 2D light with a quiet flicker.
        /// </summary>
        private void StyleFlame(GameObject flame)
        {
            var renderer = flame.GetComponent<SpriteRenderer>();
            var holder = FindHolderRenderer(flame);
            if (renderer != null && holder != null)
            {
                renderer.sortingLayerID = holder.sortingLayerID;
                renderer.sortingOrder = holder.sortingOrder + 1;
            }
            var light = flame.GetComponent<Light2D>();
            if (styledFlames.Add(flame))
            {
                flame.transform.localScale = flame.transform.localScale * 1.7f;
                if (light == null)
                {
                    light = flame.AddComponent<Light2D>();
                    light.lightType = Light2D.LightType.Point;
                    light.color = new Color(1f, .78f, .45f, 1f);
                    light.pointLightInnerRadius = .12f;
                    light.pointLightOuterRadius = 1.5f;
                    light.falloffIntensity = .7f;
                }
            }
            if (light != null)
                light.intensity = AccessibilitySettings.Instance != null && AccessibilitySettings.Instance.ReduceFlashes
                    ? .95f : .85f + Mathf.PerlinNoise(Time.time * 2.3f, flame.transform.position.x) * .25f;
        }

        private SpriteRenderer FindHolderRenderer(GameObject flame)
        {
            SpriteRenderer best = null;
            var bestDistance = float.MaxValue;
            if (candles != null)
                foreach (var candle in candles)
                {
                    if (candle == null) continue;
                    var distance = (candle.transform.position - flame.transform.position).sqrMagnitude;
                    if (distance < bestDistance) { bestDistance = distance; best = candle; }
                }
            if (best == null && flame.transform.parent != null)
                best = flame.transform.parent.GetComponentInParent<SpriteRenderer>();
            return best;
        }
    }
}
