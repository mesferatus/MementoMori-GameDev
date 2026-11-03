using UnityEngine;
using UnityEngine.UI;

namespace MementoMori.Dialogue
{
    /// <summary>Fixed character cells: authored ASCII drawings, never a raster-to-text conversion.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AsciiIllustration : MaskableGraphic
    {
        private Font font;
        private string[] rows = System.Array.Empty<string>();
        public override Texture mainTexture => font != null ? font.material.mainTexture : base.mainTexture;

        public void Configure(Font value)
        {
            font = value;
            raycastTarget = false;
            Font.textureRebuilt += OnFontRebuilt;
        }
        private void OnFontRebuilt(Font rebuilt) { if (rebuilt == font) SetVerticesDirty(); }
        protected override void OnDestroy() { Font.textureRebuilt -= OnFontRebuilt; base.OnDestroy(); }
        public void Show(string drawing)
        {
            rows = (drawing ?? string.Empty).Replace("\r", "").Split('\n');
            font?.RequestCharactersInTexture(drawing ?? string.Empty, 24, FontStyle.Normal);
            SetAllDirty();
        }
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();
            if (font == null || rows.Length == 0) return;
            var columns = 1;
            foreach (var row in rows) columns = Mathf.Max(columns, row.Length);
            var bounds = GetPixelAdjustedRect();
            var cell = Mathf.Min(bounds.width / columns, bounds.height / (rows.Length * 1.3f));
            var left = bounds.center.x - columns * cell * .5f;
            var top = bounds.center.y + rows.Length * cell * 1.3f * .5f;
            for (var y = 0; y < rows.Length; y++)
                for (var x = 0; x < rows[y].Length; x++)
                {
                    var c = rows[y][x];
                    if (c == ' ' || !font.GetCharacterInfo(c, out var glyph, 24, FontStyle.Normal)) continue;
                    var scale = cell / Mathf.Max(1, glyph.advance);
                    var cx = left + x * cell;
                    var baseline = top - (y + .8f) * cell * 1.3f;
                    var start = helper.currentVertCount;
                    helper.AddVert(new Vector3(cx + glyph.minX * scale, baseline + glyph.minY * scale), color, glyph.uvBottomLeft);
                    helper.AddVert(new Vector3(cx + glyph.minX * scale, baseline + glyph.maxY * scale), color, glyph.uvTopLeft);
                    helper.AddVert(new Vector3(cx + glyph.maxX * scale, baseline + glyph.maxY * scale), color, glyph.uvTopRight);
                    helper.AddVert(new Vector3(cx + glyph.maxX * scale, baseline + glyph.minY * scale), color, glyph.uvBottomRight);
                    helper.AddTriangle(start, start + 1, start + 2);
                    helper.AddTriangle(start + 2, start + 3, start);
                }
        }
    }
}
