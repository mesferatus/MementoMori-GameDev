using MementoMori.Core;
using UnityEngine;

namespace MementoMori.World
{
    public sealed class RoomRitualVisual : MonoBehaviour
    {
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

        public void Configure(SpriteRenderer ritualRug, SpriteRenderer[] candleRenderers, Sprite off, Sprite on, bool returned)
        {
            rug = ritualRug; candles = candleRenderers; unlitCandle = off; litCandle = on; returnedRoom = returned;
            initialized = false;
        }
        private void LateUpdate()
        {
            var state = GameState.Instance;
            var active = !returnedRoom && state != null && state.RitualCompleted;
            var progress = !returnedRoom && state != null ? state.GetPuzzleProgress(RoomCandlePuzzle.ProgressId) : 0;
            if (rug != null)
            {
                var next = active ? activeRug : inactiveRug;
                if (next != null) rug.sprite = next;
                rug.color = next != null || active ? Color.white : new Color(.53f, .49f, .57f, 1f);
            }
            if (candles != null && (!initialized || progress != lastProgress))
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
                }
                initialized = true; lastProgress = progress;
            }
            if (ritualGlow != null) ritualGlow.SetActive(active);
            if (flames == null) return;
            foreach (var flame in flames)
                if (flame != null) flame.SetActive(progress == 4);
        }
    }
}
