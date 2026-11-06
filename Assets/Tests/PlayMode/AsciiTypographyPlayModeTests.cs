using System.Collections;
using MementoMori.Dialogue;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MementoMori.Tests.PlayMode
{
    public sealed class AsciiTypographyPlayModeTests
    {
        [UnityTest]
        public IEnumerator ColorMarkupPreservesCellsAndGlyphProportions()
        {
            var root = new GameObject("TypographyTest", typeof(Canvas));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var go = new GameObject("Drawing", typeof(RectTransform), typeof(AsciiIllustration));
            go.transform.SetParent(root.transform, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(600, 300);
            var drawing = go.GetComponent<AsciiIllustration>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            drawing.Configure(font);
            try
            {
                drawing.Show("M.i");
                yield return null;
                Canvas.ForceUpdateCanvases();
                var plain = Object.Instantiate(go.GetComponent<CanvasRenderer>().GetMesh());
                drawing.Show("<color=#80C0FF>M.</color>i");
                yield return null;
                Canvas.ForceUpdateCanvases();
                var colored = go.GetComponent<CanvasRenderer>().GetMesh();
                Assert.That(colored.vertexCount, Is.EqualTo(12), "Markup is not visible geometry.");
                Assert.That(colored.vertices, Is.EqualTo(plain.vertices), "Color does not move cells.");
                Assert.That(colored.colors32[0], Is.EqualTo(new Color32(128, 192, 255, 255)));
                Assert.That(colored.colors32[8], Is.EqualTo((Color32)drawing.color));
                font.GetCharacterInfo('M', out var wide, 24);
                font.GetCharacterInfo('.', out var narrow, 24);
                var vertices = colored.vertices;
                var wideScale = (vertices[2].x - vertices[1].x) / (wide.maxX - wide.minX);
                var narrowScale = (vertices[6].x - vertices[5].x) / (narrow.maxX - narrow.minX);
                Assert.That(narrowScale, Is.EqualTo(wideScale).Within(.001f), "Punctuation retains its native proportions.");
                Object.Destroy(plain);
            }
            finally { Object.Destroy(root); }
        }
    }
}
