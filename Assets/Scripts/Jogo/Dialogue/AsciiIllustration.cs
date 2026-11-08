using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

namespace MementoMori.Dialogue
{
    /// <summary>Fixed character cells for authored ASCII scenes and portraits sampled from approved sprites.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AsciiIllustration : MaskableGraphic
    {
        private Font font;
        private string[] rows = System.Array.Empty<string>();
        private Color?[][] inks = System.Array.Empty<Color?[]>();
        private const int GlyphSize = 24;
        private bool requestingGlyphs;
        private const string DrawingGlyphs = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
        public override Texture mainTexture => font != null ? font.material.mainTexture : base.mainTexture;

        public void Configure(Font value)
        {
            Font.textureRebuilt -= OnFontRebuilt;
            font = value;
            raycastTarget = false;
            Font.textureRebuilt += OnFontRebuilt;
        }
        private void OnFontRebuilt(Font rebuilt)
        {
            if (rebuilt == font && !requestingGlyphs) SetAllDirty();
        }
        protected override void OnDestroy() { Font.textureRebuilt -= OnFontRebuilt; base.OnDestroy(); }
        public void Show(string drawing)
        {
            var lines = new List<string>();
            var colors = new List<Color?[]>();
            var row = new StringBuilder();
            var ink = new List<Color?>();
            var stack = new Stack<Color>();
            var source = (drawing ?? string.Empty).Replace("\r", "");
            for (var i = 0; i < source.Length; i++)
            {
                if (source[i] == '<')
                {
                    var end = source.IndexOf('>', i);
                    if (end >= 0)
                    {
                        var tag = source.Substring(i + 1, end - i - 1);
                        if (tag.StartsWith("color=") && ColorUtility.TryParseHtmlString(tag.Substring(6), out var parsed))
                        { stack.Push(parsed); i = end; continue; }
                        if (tag == "/color" && stack.Count > 0)
                        { stack.Pop(); i = end; continue; }
                    }
                }
                if (source[i] == '\n')
                { lines.Add(row.ToString()); colors.Add(ink.ToArray()); row.Clear(); ink.Clear(); }
                else { row.Append(source[i]); ink.Add(stack.Count > 0 ? stack.Peek() : (Color?)null); }
            }
            lines.Add(row.ToString()); colors.Add(ink.ToArray());
            rows = lines.ToArray(); inks = colors.ToArray();
            EnsureBackdrop()?.SetVerticesDirty();
            font?.RequestCharactersInTexture(DrawingGlyphs + string.Join("\n", rows), GlyphSize, FontStyle.Normal);
            SetAllDirty();
        }
        /// <summary>Cells inked with alpha 253/254 ("solid") get a dimmed block of their own colour behind the glyph.</summary>
        public static bool IsSolid(Color c) => c.a > .989f && c.a < .9985f;
        /// <summary>Alpha 254 marks figure cells (strong backing), 253 scenery cells (faint backing).</summary>
        public static float BackingWeight(Color c) => c.a > .994f ? 1f : .42f;
        internal string[] Rows => rows;
        internal Color?[][] Inks => inks;
        private AsciiBackdrop backdrop;
        private AsciiBackdrop EnsureBackdrop()
        {
            if (backdrop != null || transform.parent == null) return backdrop;
            backdrop = transform.parent.GetComponentInChildren<AsciiBackdrop>();
            if (backdrop != null) return backdrop;
            var go = new GameObject("AsciiBackdrop", typeof(RectTransform), typeof(AsciiBackdrop));
            var rect = (RectTransform)go.transform;
            rect.SetParent(transform.parent, false);
            rect.SetSiblingIndex(transform.GetSiblingIndex());
            var own = (RectTransform)transform;
            rect.anchorMin = own.anchorMin; rect.anchorMax = own.anchorMax; rect.offsetMin = own.offsetMin; rect.offsetMax = own.offsetMax;
            backdrop = go.GetComponent<AsciiBackdrop>();
            backdrop.Bind(this);
            return backdrop;
        }
        /// <summary>Same layout the glyph mesh uses: left/top corner, cell width and line height in local units.</summary>
        internal bool Layout(out float left, out float top, out float cell, out float line)
        {
            left = top = cell = line = 0f;
            if (font == null || rows.Length == 0) return false;
            var columns = 1;
            foreach (var row in rows) columns = Mathf.Max(columns, row.Length);
            float advance = 1f, ascent = 1f, descent = 0f;
            foreach (var character in DrawingGlyphs)
                if (font.GetCharacterInfo(character, out var metric, GlyphSize, FontStyle.Normal))
                { advance = Mathf.Max(advance, metric.advance); ascent = Mathf.Max(ascent, metric.maxY); descent = Mathf.Min(descent, metric.minY); }
            var lineHeight = Mathf.Max(ascent - descent + 2, GlyphSize);
            var bounds = GetPixelAdjustedRect();
            // The grid fills the lilac frame on both axes; glyphs keep one undistorted scale.
            cell = bounds.width / columns; line = bounds.height / rows.Length;
            left = bounds.xMin;
            top = bounds.yMax;
            return true;
        }
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();
            if (font == null || rows.Length == 0) return;
            // Dialogue labels share the dynamic atlas and can evict drawing glyphs.
            requestingGlyphs = true;
            try { font.RequestCharactersInTexture(DrawingGlyphs + string.Join("\n", rows), GlyphSize, FontStyle.Normal); }
            finally { requestingGlyphs = false; }
            var columns = 1;
            foreach (var row in rows) columns = Mathf.Max(columns, row.Length);
            // One scale for the entire drawing: narrow punctuation must not be stretched.
            var advance = 1f;
            var ascent = 1f;
            var descent = 0f;
                foreach (var character in DrawingGlyphs)
                    if (font.GetCharacterInfo(character, out var metric, GlyphSize, FontStyle.Normal))
                    {
                        advance = Mathf.Max(advance, metric.advance);
                        ascent = Mathf.Max(ascent, metric.maxY);
                        descent = Mathf.Min(descent, metric.minY);
                    }
            var lineHeight = Mathf.Max(ascent - descent + 2, GlyphSize);
            var bounds = GetPixelAdjustedRect();
            var scaleX = bounds.width / (columns * advance);
            var scaleY = bounds.height / (rows.Length * lineHeight);
            var scale = Mathf.Min(scaleX, scaleY);
            var cell = advance * scaleX;
            var line = lineHeight * scaleY;
            var left = bounds.xMin;
            var top = bounds.yMax;
            for (var y = 0; y < rows.Length; y++)
                for (var x = 0; x < rows[y].Length; x++)
                {
                    var c = rows[y][x];
                    if (c == ' ' || !font.GetCharacterInfo(c, out var glyph, 24, FontStyle.Normal)) continue;
                    var cx = left + x * cell + (cell - glyph.advance * scale) * .5f;
                    var baseline = top - y * line - (line - lineHeight * scale) * .5f - ascent * scale;
                    var tint = inks[y][x] ?? color;
                    if (inks[y][x].HasValue) tint.a = (IsSolid(tint) ? 1f : tint.a) * color.a;
                    var start = helper.currentVertCount;
                    helper.AddVert(new Vector3(cx + glyph.minX * scale, baseline + glyph.minY * scale), tint, glyph.uvBottomLeft);
                    helper.AddVert(new Vector3(cx + glyph.minX * scale, baseline + glyph.maxY * scale), tint, glyph.uvTopLeft);
                    helper.AddVert(new Vector3(cx + glyph.maxX * scale, baseline + glyph.maxY * scale), tint, glyph.uvTopRight);
                    helper.AddVert(new Vector3(cx + glyph.maxX * scale, baseline + glyph.minY * scale), tint, glyph.uvBottomRight);
                    helper.AddTriangle(start, start + 1, start + 2);
                    helper.AddTriangle(start + 2, start + 3, start);
                }
        }
    }
}
