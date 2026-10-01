using MementoMori.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MementoMori.UI
{
    public sealed class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text label;
        [SerializeField] private TMP_Text labelTmp;
        private Sprite scalableBackground;

        public void Configure(CanvasGroup group, Text promptLabel)
        {
            canvasGroup = group;
            label = promptLabel;
            SetTarget(null);
        }

        public void Configure(CanvasGroup group, TMP_Text promptLabel)
        {
            canvasGroup = group;
            labelTmp = promptLabel;
            SetTarget(null);
        }

        public void SetTarget(IInteractable target)
        {
            var visible = target != null;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.blocksRaycasts = false;
            }
            if (visible)
                SetPromptText(target.InteractionVerb);
        }

        private void SetPromptText(string action)
        {
            var value = string.IsNullOrWhiteSpace(action) ? "[E]" : action;
            if (label != null) label.text = $"E - {value}";
            if (labelTmp != null && labelTmp.text == value && scalableBackground != null) return;
            if (labelTmp != null) labelTmp.text = value;
            if (labelTmp == null) return;

            var background = labelTmp.transform.parent as RectTransform;
            if (background == null) return;
            var image = background.GetComponent<Image>();
            if (image != null && image.sprite != null)
            {
                if (scalableBackground == null)
                {
                    var source = image.sprite;
                    var pivot = new Vector2(source.pivot.x / source.rect.width, source.pivot.y / source.rect.height);
                    scalableBackground = Sprite.Create(source.texture, source.rect, pivot,
                        source.pixelsPerUnit, 0, SpriteMeshType.FullRect, new Vector4(180, 180, 180, 180));
                }
                image.sprite = scalableBackground;
                image.type = Image.Type.Sliced;
                image.preserveAspect = false;
            }

            labelTmp.fontSize = 32;
            labelTmp.enableAutoSizing = false;
            labelTmp.textWrappingMode = TextWrappingModes.NoWrap;
            labelTmp.ForceMeshUpdate();
            var width = Mathf.Clamp(labelTmp.preferredWidth + 260f, 430f, 930f);
            background.sizeDelta = new Vector2(width, 170f);
            labelTmp.rectTransform.anchorMin = new Vector2(0f, .5f);
            labelTmp.rectTransform.anchorMax = new Vector2(1f, .5f);
            labelTmp.rectTransform.offsetMin = new Vector2(170f, -55f);
            labelTmp.rectTransform.offsetMax = new Vector2(-35f, 55f);
            var keycap = background.Find("Keycap_E") as RectTransform;
            if (keycap != null)
            {
                keycap.anchorMin = keycap.anchorMax = new Vector2(0f, .5f);
                keycap.sizeDelta = new Vector2(145f, 155f);
                keycap.anchoredPosition = new Vector2(65f, 0f);
            }
            var icon = background.Find("InteractionIcon");
            if (icon != null) icon.gameObject.SetActive(false);
        }
    }
}
