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
                    SetCandleGlow(candle, lit);
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
                var lit = progress > interactable.OrderIndex;
                renderer.sprite = lit ? litCandle : unlitCandle;
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
                    light.color = new Color(1f, .74f, .42f, 1f);
                    light.pointLightInnerRadius = .2f;
                    light.pointLightOuterRadius = 2.4f;
                    light.falloffIntensity = .6f;
                }
                AddHalo(flame, renderer);
            }
            if (light != null)
                light.intensity = AccessibilitySettings.Instance != null && AccessibilitySettings.Instance.ReduceFlashes
                    ? 1.25f : 1.1f + Mathf.PerlinNoise(Time.time * 2.3f, flame.transform.position.x) * .35f;
        }

        /// <summary>
        /// The ritual candles have no separate flame objects: the glow (additive halo + warm 2D light)
        /// lives at the wick of the lit candle sprite and switches with it.
        /// </summary>
        private static void SetCandleGlow(SpriteRenderer candle, bool lit)
        {
            var wick = candle.transform.Find("Wick");
            if (!lit) { if (wick != null) wick.gameObject.SetActive(false); return; }
            if (wick == null)
            {
                EnsureAmbientLight();
                wick = new GameObject("Wick").transform;
                wick.SetParent(candle.transform, false);
                var top = candle.sprite != null ? candle.sprite.bounds.max.y : .5f;
                wick.localPosition = new Vector3(0f, top * .08f, 0f);
                var light = wick.gameObject.AddComponent<Light2D>();
                light.lightType = Light2D.LightType.Point;
                light.color = new Color(1f, .74f, .42f, 1f);
                light.intensity = 1.2f;
                light.pointLightInnerRadius = .15f;
                light.pointLightOuterRadius = 2.2f;
                light.falloffIntensity = .65f;
                var holder = wick.gameObject.AddComponent<SpriteRenderer>();
                holder.enabled = false;
                holder.sortingLayerID = candle.sortingLayerID;
                holder.sortingOrder = candle.sortingOrder;
                AddHalo(wick.gameObject, holder);
                var halo = wick.Find("Halo");
                if (halo != null) halo.localScale *= .75f;
            }
            wick.gameObject.SetActive(true);
        }

        /// <summary>
        /// A scene with no Light2D renders at full brightness; the first point light switches 2D lighting on
        /// and everything outside it would go black. A neutral global light keeps the room as authored.
        /// </summary>
        private static void EnsureAmbientLight()
        {
            foreach (var existing in FindObjectsByType<Light2D>(FindObjectsSortMode.None))
                if (existing.lightType == Light2D.LightType.Global) return;
            var ambient = new GameObject("RoomAmbientLight").AddComponent<Light2D>();
            ambient.lightType = Light2D.LightType.Global;
            ambient.color = Color.white;
            ambient.intensity = 1f;
        }

        private static Sprite haloSprite;
        private static Material haloMaterial;

        /// <summary>Warm additive glow around a lit flame; the shader adds its own soft flicker.</summary>
        private static void AddHalo(GameObject flame, SpriteRenderer flameRenderer)
        {
            if (haloMaterial == null) haloMaterial = Resources.Load<Material>("Presentation/FX/CandleHalo");
            if (haloMaterial == null || flame.transform.Find("Halo") != null) return;
            if (haloSprite == null)
                haloSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(.5f, .5f), 4f);
            var halo = new GameObject("Halo", typeof(SpriteRenderer));
            halo.transform.SetParent(flame.transform, false);
            var scale = flame.transform.lossyScale.x;
            halo.transform.localScale = Vector3.one * (1.5f / Mathf.Max(.01f, scale));
            var glow = halo.GetComponent<SpriteRenderer>();
            glow.sprite = haloSprite;
            glow.sharedMaterial = haloMaterial;
            if (flameRenderer != null)
            {
                glow.sortingLayerID = flameRenderer.sortingLayerID;
                glow.sortingOrder = flameRenderer.sortingOrder + 1;
            }
            var block = new MaterialPropertyBlock();
            block.SetFloat("_Seed", flame.transform.position.x * 3.1f + flame.transform.position.y);
            glow.SetPropertyBlock(block);
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
