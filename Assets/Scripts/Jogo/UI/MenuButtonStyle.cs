using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MementoMori.UI
{
    /// <summary>
    /// The approved main-menu button look (9-slice lilac frame, colour-tint hover), shared by every
    /// non-dialogue screen: menu, pause, save slots, settings and credits.
    /// </summary>
    public static class MenuButtonStyle
    {
        private static Sprite frame;

        public static Sprite Frame => frame != null ? frame : frame = Resources.Load<Sprite>("MainMenu/BotaoMenu");

        public static void Apply(Button button, Vector2? size = null, float borderScale = 1f)
        {
            if (button == null || Frame == null) return;
            var rect = (RectTransform)button.transform;
            if (size.HasValue) rect.sizeDelta = size.Value;
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = Frame;
                image.overrideSprite = null; // a previous sprite-swap transition can leave its sprite here
                image.type = Image.Type.Sliced;
                // Short buttons (pause, settings) shrink the frame ends so they do not overlap.
                image.pixelsPerUnitMultiplier = 1f / Mathf.Max(.1f, borderScale);
                image.color = Color.white;
            }
            if (button.GetComponent<MenuButtonFeedback>() == null)
                button.gameObject.AddComponent<MenuButtonFeedback>();
            button.spriteState = new SpriteState();
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = new Color(.86f, .82f, .9f, 1f);
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(.7f, .62f, .78f, 1f);
            colors.disabledColor = new Color(.55f, .5f, .6f, 1f); // same shape as the others, only dimmer
            colors.colorMultiplier = 1.15f;
            colors.fadeDuration = .12f;
            button.colors = colors;
            foreach (var name in new[] { "SelectorLeft", "SelectorRight", "GlowOverlay" })
            {
                // The frame carries its own stars; the old side diamonds would double them.
                var extra = rect.Find(name);
                if (extra != null) extra.gameObject.SetActive(false);
            }
            var label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                // Text stays inside the stars at the ends; smaller frames leave more room for long labels.
                var inset = .1f + .1f * Mathf.Clamp01(borderScale);
                label.rectTransform.anchorMin = new Vector2(inset, .12f);
                label.rectTransform.anchorMax = new Vector2(1f - inset, .88f);
                label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
                label.color = new Color(.95f, .9f, 1f, 1f);
                // One line always ("REINICIAR CHECKPOINT"): shrink a little instead of wrapping.
                label.textWrappingMode = TextWrappingModes.NoWrap;
                if (!label.enableAutoSizing)
                {
                    label.fontSizeMax = label.fontSize;
                    label.fontSizeMin = Mathf.Max(12f, label.fontSize * .7f);
                    label.enableAutoSizing = true;
                }
            }
        }

        public static void ApplyAll(Transform root, Vector2? size = null, float borderScale = 1f)
        {
            if (root == null) return;
            foreach (var button in root.GetComponentsInChildren<Button>(true)) Apply(button, size, borderScale);
        }
    }
}
