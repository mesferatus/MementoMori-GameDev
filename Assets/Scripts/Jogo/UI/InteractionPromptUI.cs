using MementoMori.Interaction;
using MementoMori.Core;
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
        private string lastKey;
        private Component targetComponent;
        private Transform player;

        // The prompt follows the object it refers to instead of sitting under Melantha, so it no longer
        // covers her or the props around her (candles on the ritual rug).
        private const float PromptScale = .78f;
        private const float PromptGap = 34f;
        private const float ScreenMargin = 24f;

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
            targetComponent = visible ? target as Component : null;
            if (visible)
            {
                SetPromptText(target.InteractionVerb);
                PlaceNearTarget();
            }
        }

        private void LateUpdate() => PlaceNearTarget();

        private void PlaceNearTarget()
        {
            if (targetComponent == null || labelTmp == null) return;
            var background = labelTmp.transform.parent as RectTransform;
            var parent = background != null ? background.parent as RectTransform : null;
            var camera = Camera.main;
            if (parent == null || camera == null) return;

            background.localScale = Vector3.one * PromptScale;
            var world = targetComponent.transform.position;
            if (player == null)
            {
                var controller = FindAnyObjectByType<MementoMori.Player.PlayerController>();
                if (controller != null) player = controller.transform;
            }
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, camera.WorldToScreenPoint(world), null, out var local))
                return;

            var halfHeight = background.rect.height * PromptScale * .5f;
            var halfWidth = background.rect.width * PromptScale * .5f;
            // Above the object, unless Melantha stands above it: then below, so the box never sits on her.
            var below = player != null && player.position.y > world.y + .1f;
            local.y += below ? -(halfHeight + PromptGap) : halfHeight + PromptGap;
            var bounds = parent.rect;
            local.x = Mathf.Clamp(local.x, bounds.xMin + halfWidth + ScreenMargin, bounds.xMax - halfWidth - ScreenMargin);
            local.y = Mathf.Clamp(local.y, bounds.yMin + halfHeight + ScreenMargin, bounds.yMax - halfHeight - ScreenMargin);
            background.localPosition = new Vector3(local.x, local.y, 0f);
        }

        private void SetPromptText(string action)
        {
            var key = InputBindings.Get(GameAction.Interact).ToString().ToUpperInvariant();
            var value = string.IsNullOrWhiteSpace(action) ? "INTERAGIR" : action;
            if (label != null) label.text = $"{key} - {value}";
            if (labelTmp != null && labelTmp.text == value && scalableBackground != null && lastKey == key) return;
            lastKey = key;
            if (labelTmp != null) labelTmp.text = value;
            if (labelTmp == null) return;

            var background = labelTmp.transform.parent as RectTransform;
            if (background == null) return;
            var image = background.GetComponent<Image>();
            if (image != null && image.sprite != null)
            {
                if (scalableBackground == null) scalableBackground = image.sprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
            }

            labelTmp.fontSize = 32;
            labelTmp.enableAutoSizing = false;
            labelTmp.textWrappingMode = TextWrappingModes.NoWrap;
            labelTmp.ForceMeshUpdate();
            var width = Mathf.Clamp(labelTmp.preferredWidth + 250f, 500f, 1090f);
            background.sizeDelta = new Vector2(width, 185f);
            labelTmp.rectTransform.anchorMin = new Vector2(0f, .5f);
            labelTmp.rectTransform.anchorMax = new Vector2(1f, .5f);
            labelTmp.rectTransform.offsetMin = new Vector2(125f, -58f);
            labelTmp.rectTransform.offsetMax = new Vector2(-60f, 58f);
            var keycap = background.Find("Keycap_E") as RectTransform;
            if (keycap != null)
            {
                keycap.anchorMin = keycap.anchorMax = new Vector2(0f, .5f);
                keycap.sizeDelta = new Vector2(155f, 165f);
                keycap.anchoredPosition = new Vector2(72f, 0f);
                var keyLabel = keycap.GetComponentInChildren<TMP_Text>();
                if (keyLabel != null) keyLabel.text = key;
            }
            var icon = background.Find("InteractionIcon");
            if (icon != null) icon.gameObject.SetActive(false);
        }
    }
}
