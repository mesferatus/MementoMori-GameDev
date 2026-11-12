using System.Text;
using TMPro;
using UnityEngine;

namespace MementoMori.UI
{
    /// <summary>
    /// Procedural ASCII night behind the final credits: moon phases, twinkling stars,
    /// Poe crossing the sky and the four ritual candles. Rendered as one TMP label with
    /// <mspace> so any project font behaves as a monospace grid.
    /// </summary>
    public sealed class AsciiCreditsView : MonoBehaviour
    {
        private const int Columns = 72;
        private const int Rows = 30;
        private const float FrameSeconds = .12f;

        private TMP_Text label;
        private float timer;
        private int tick;
        private char[,] grid;

        public static AsciiCreditsView Attach(RectTransform parent, TMP_FontAsset font)
        {
            var go = new GameObject("AsciiNight", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(24f, 16f); rect.offsetMax = new Vector2(-24f, -16f);
            var text = go.AddComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.fontSize = 30f;
            text.enableAutoSizing = true;
            text.fontSizeMin = 10f; text.fontSizeMax = 34f;
            text.color = new Color(.63f, .52f, .78f, .42f);
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.richText = true;
            text.raycastTarget = false;
            var view = go.AddComponent<AsciiCreditsView>();
            view.label = text;
            return view;
        }

        private void Awake() { grid = new char[Rows, Columns]; }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;
            if (timer < FrameSeconds || label == null) return;
            timer = 0f;
            tick++;
            label.text = "<mspace=0.6em>" + Compose(tick) + "</mspace>";
        }

        private static int Hash(int x, int y) { unchecked { var h = x * 374761393 + y * 668265263; h = (h ^ (h >> 13)) * 1274126177; return h ^ (h >> 16); } }

        private string Compose(int t)
        {
            for (var y = 0; y < Rows; y++) for (var x = 0; x < Columns; x++) grid[y, x] = ' ';
            DrawStars(t);
            DrawMoon(t);
            DrawRaven(t);
            DrawCandles(t);
            var sb = new StringBuilder(Rows * (Columns + 1));
            for (var y = 0; y < Rows; y++)
            {
                for (var x = 0; x < Columns; x++)
                {
                    var c = grid[y, x];
                    // escape rich-text brackets just in case
                    if (c == '<') c = '(';
                    sb.Append(c);
                }
                if (y < Rows - 1) sb.Append('\n');
            }
            return sb.ToString();
        }

        private void DrawStars(int t)
        {
            for (var y = 0; y < Rows - 6; y++)
                for (var x = 0; x < Columns; x++)
                {
                    var h = Hash(x, y) & 0x7fffffff;
                    if (h % 53 != 0) continue;
                    var phase = (t / 4 + (h >> 8)) % 7;
                    grid[y, x] = phase switch { 0 => '.', 1 => '+', 2 => '*', 3 => '+', _ => '.' };
                }
        }

        private void DrawMoon(int t)
        {
            const int cx = 56, cy = 8, r = 6;
            // slow phase cycle: -1 (new) .. +1 (full) — terminator slides across the disc
            var cycle = Mathf.PingPong(t * .01f, 2f) - 1f;
            for (var y = -r; y <= r; y++)
                for (var x = -r; x <= r; x++)
                {
                    var px = cx + x; var py = cy + y;
                    if (px < 0 || px >= Columns || py < 0 || py >= Rows) continue;
                    var d = (x * x) / (float)(r * r) + (y * y * 1.6f) / (r * r);
                    if (d > 1f) continue;
                    var half = Mathf.Sqrt(Mathf.Max(0f, 1f - (y * y * 1.6f) / (r * r))) * r;
                    var terminator = cycle * half;
                    var lit = x >= terminator - half * .15f;
                    grid[py, px] = lit ? (d > .72f ? 'o' : '@') : (d > .8f ? '.' : ' ');
                }
        }

        private void DrawRaven(int t)
        {
            // Poe glides across the sky, bobbing with the beat of his wings.
            var x0 = (t / 2) % (Columns + 24) - 12;
            var y0 = 13 + Mathf.RoundToInt(Mathf.Sin(t * .22f) * 2f);
            var flap = (t / 3) % 2 == 0;
            string[] shape = flap
                ? new[] { @"  \    /  ", @"   \,,/   ", @"   (o )>  " }
                : new[] { @"          ", @" __/,,\__ ", @"   (o )>  " };
            for (var row = 0; row < shape.Length; row++)
                for (var col = 0; col < shape[row].Length; col++)
                {
                    var c = shape[row][col];
                    if (c == ' ') continue;
                    var px = x0 + col; var py = y0 + row;
                    if (px >= 0 && px < Columns && py >= 0 && py < Rows) grid[py, px] = c;
                }
        }

        private void DrawCandles(int t)
        {
            int[] xs = { 10, 26, 44, 60 };
            var baseY = Rows - 3;
            for (var i = 0; i < xs.Length; i++)
            {
                var x = xs[i];
                if (x + 2 >= Columns) continue;
                var f = (t + i * 3) / 2 % 3;
                grid[baseY - 2, x + 1] = f == 0 ? ')' : f == 1 ? '(' : '|';
                grid[baseY - 1, x] = '['; grid[baseY - 1, x + 1] = '#'; grid[baseY - 1, x + 2] = ']';
                grid[baseY, x] = '['; grid[baseY, x + 1] = '#'; grid[baseY, x + 2] = ']';
            }
            // ground line
            for (var x = 2; x < Columns - 2; x++)
                if ((Hash(x, 97) & 3) != 0) grid[Rows - 1, x] = '_';
        }
    }
}
