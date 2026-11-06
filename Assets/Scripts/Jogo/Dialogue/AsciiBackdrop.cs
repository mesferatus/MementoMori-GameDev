using UnityEngine;
using UnityEngine.UI;

namespace MementoMori.Dialogue
{
    /// <summary>
    /// ANSI-style cell backing for an <see cref="AsciiIllustration"/>: every "solid" cell (ink alpha 254)
    /// gets a dimmed block of its own colour, so characters read as volumes while glyphs keep the texture.
    /// Drawn behind the glyph mesh; frames without solid cells produce no geometry.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AsciiBackdrop : MaskableGraphic
    {
        [SerializeField, Range(0f, 1f)] private float strength = .46f;
        private AsciiIllustration source;
        public void Bind(AsciiIllustration illustration) { source = illustration; raycastTarget = false; SetVerticesDirty(); }

        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();
            if (source == null || !source.Layout(out var left, out var top, out var cell, out var line)) return;
            var rows = source.Rows; var inks = source.Inks;
            for (var y = 0; y < rows.Length && y < inks.Length; y++)
                for (var x = 0; x < inks[y].Length; x++)
                {
                    var ink = inks[y][x];
                    if (!ink.HasValue || !AsciiIllustration.IsSolid(ink.Value)) continue;
                    var c = ink.Value * (strength * AsciiIllustration.BackingWeight(ink.Value)); c.a = color.a * source.color.a;
                    float x0 = left + x * cell, x1 = x0 + cell + .5f, y1 = top - y * line, y0 = y1 - line - .5f;
                    var start = helper.currentVertCount;
                    helper.AddVert(new Vector3(x0, y0), c, Vector2.zero);
                    helper.AddVert(new Vector3(x0, y1), c, Vector2.zero);
                    helper.AddVert(new Vector3(x1, y1), c, Vector2.zero);
                    helper.AddVert(new Vector3(x1, y0), c, Vector2.zero);
                    helper.AddTriangle(start, start + 1, start + 2);
                    helper.AddTriangle(start + 2, start + 3, start);
                }
        }
    }
}
