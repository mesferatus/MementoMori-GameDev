using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MementoMori.UI
{
    /// <summary>
    /// Panel look shared by the screens that open over the menu or the game (settings, credits): a dark veil
    /// over what is behind, a translucent panel with the menu's thin lilac double frame and star corners, and a
    /// spaced title over an ornamental divider. Built from code so no new bitmap is needed.
    /// </summary>
    public static class MenuPanelStyle
    {
        public static readonly Color Lilac = new(.78f, .62f, .92f, .9f);
        public static readonly Color LilacSoft = new(.72f, .55f, .86f, .45f);
        public static readonly Color TitleColor = new(.95f, .9f, 1f, 1f);
        public static readonly Color CaptionColor = new(.73f, .63f, .84f, 1f);
        private static Sprite diamond;

        /// <summary>Full-screen veil; the menu art or the game stays faintly visible behind it.</summary>
        public static RectTransform Veil(Transform parent, float alpha = .8f)
        {
            var veil = NewImage("Veu", parent, null, new Color(.018f, .006f, .032f, alpha));
            Stretch(veil);
            veil.GetComponent<Image>().raycastTarget = true; // blocks clicks on what is behind
            var vignette = Resources.Load<Sprite>("MainMenu/VinhetaMenu");
            if (vignette != null) Stretch(NewImage("Vinheta", veil, vignette, Color.white));
            return veil;
        }

        /// <summary>Translucent panel with the double frame; returns the panel rect (children go inside).</summary>
        public static RectTransform Panel(Transform parent, Vector2 size)
        {
            var panel = NewImage("Painel", parent, null, new Color(.05f, .022f, .078f, .975f));
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(.5f, .5f);
            panel.sizeDelta = size;
            Frame(panel, 0f, 2f, Lilac);
            Frame(panel, 9f, 1f, LilacSoft);
            foreach (var corner in new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one })
            {
                var star = NewImage("Estrela", panel, Diamond(), Lilac);
                star.anchorMin = star.anchorMax = corner;
                star.sizeDelta = new Vector2(22f, 22f);
                star.anchoredPosition = Vector2.zero;
            }
            return panel;
        }

        /// <summary>Credits text shared by the main menu and the closing card (panel of 1000 x 820).</summary>
        public static void CreditsBody(RectTransform panel, TMP_FontAsset bodyFont)
        {
            var names = NewText("Equipe", panel, bodyFont,
                "<size=22><color=#B9A2D5>ARTE, ORGANIZAÇÃO VISUAL E COLLIDERS</color></size>\n<size=44>Luiza Canova</size>\n\n" +
                "<size=22><color=#B9A2D5>PROGRAMAÇÃO, SHADERS E ÁUDIO</color></size>\n<size=44>Callisto Monteiro Mascoloti</size>\n\n" +
                "<size=22><color=#B9A2D5>ILUSTRAÇÃO DO MENU</color></size>\n<size=44>Camila David Ribeiro</size>",
                40f, TitleColor);
            names.characterSpacing = 2f;
            names.rectTransform.anchorMin = names.rectTransform.anchorMax = new Vector2(.5f, .5f);
            names.rectTransform.sizeDelta = new Vector2(860f, 440f);
            names.rectTransform.anchoredPosition = new Vector2(0f, 30f);

            var fonts = NewText("Fontes", panel, bodyFont,
                "Fontes Cinzel, Cormorant Garamond e UnifrakturMaguntia (SIL Open Font License) · Feito com Unity",
                20f, CaptionColor);
            fonts.rectTransform.anchorMin = fonts.rectTransform.anchorMax = new Vector2(.5f, 0f);
            fonts.rectTransform.sizeDelta = new Vector2(900f, 40f);
            fonts.rectTransform.anchoredPosition = new Vector2(0f, 200f);
            Divider(panel, -panel.sizeDelta.y + 172f, 300f);
        }

        /// <summary>Spaced title with a divider (line, star, line) under it.</summary>
        public static TMP_Text Title(RectTransform panel, string text, TMP_FontAsset font, float top = 64f, float size = 46f)
        {
            var title = NewText("Titulo", panel, font, text, size, TitleColor);
            title.characterSpacing = 12f;
            var rect = title.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
            rect.sizeDelta = new Vector2(panel.sizeDelta.x - 120f, 70f);
            rect.anchoredPosition = new Vector2(0f, -top);
            Divider(panel, -top - 48f, Mathf.Min(520f, panel.sizeDelta.x * .5f));
            return title;
        }

        public static void Divider(RectTransform panel, float y, float width)
        {
            var root = new GameObject("Divisor", typeof(RectTransform));
            var rect = (RectTransform)root.transform;
            rect.SetParent(panel, false);
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
            rect.sizeDelta = new Vector2(width, 18f);
            rect.anchoredPosition = new Vector2(0f, y);
            var left = NewImage("Linha", rect, null, LilacSoft);
            left.anchorMin = new Vector2(0f, .5f); left.anchorMax = new Vector2(.5f, .5f);
            left.offsetMin = new Vector2(0f, -.5f); left.offsetMax = new Vector2(-16f, .5f);
            var right = NewImage("Linha", rect, null, LilacSoft);
            right.anchorMin = new Vector2(.5f, .5f); right.anchorMax = new Vector2(1f, .5f);
            right.offsetMin = new Vector2(16f, -.5f); right.offsetMax = new Vector2(0f, .5f);
            var star = NewImage("Estrela", rect, Diamond(), Lilac);
            star.anchorMin = star.anchorMax = new Vector2(.5f, .5f);
            star.sizeDelta = new Vector2(16f, 16f);
        }

        public static TMP_Text NewText(string name, Transform parent, TMP_FontAsset font, string text, float size, Color color)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(parent, false);
            if (font != null) label.font = font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        public static RectTransform NewImage(string name, Transform parent, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void Frame(RectTransform panel, float inset, float thickness, Color color)
        {
            var root = new GameObject("Moldura", typeof(RectTransform));
            var rect = (RectTransform)root.transform;
            rect.SetParent(panel, false);
            Stretch(rect);
            rect.offsetMin = Vector2.one * inset; rect.offsetMax = -Vector2.one * inset;
            Edge(rect, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -thickness), Vector2.zero, color);
            Edge(rect, Vector2.zero, new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, thickness), color);
            Edge(rect, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(thickness, 0f), color);
            Edge(rect, new Vector2(1f, 0f), Vector2.one, new Vector2(-thickness, 0f), Vector2.zero, color);
        }

        private static void Edge(RectTransform parent, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var line = NewImage("Linha", parent, null, color);
            line.anchorMin = min; line.anchorMax = max; line.offsetMin = offsetMin; line.offsetMax = offsetMax;
        }

        /// <summary>Four-point star, the same motif as the menu button ends.</summary>
        public static Sprite Diamond()
        {
            if (diamond != null) return diamond;
            const int size = 33;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var c = (size - 1) * .5f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - c) / c, dy = Mathf.Abs(y - c) / c;
                // concave four-point star: |x|^.5 + |y|^.5 <= 1
                var v = Mathf.Sqrt(dx) + Mathf.Sqrt(dy);
                var a = Mathf.Clamp01((1f - v) * 6f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
            texture.Apply();
            diamond = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100f);
            diamond.name = "EstrelaMenu";
            return diamond;
        }
    }
}
