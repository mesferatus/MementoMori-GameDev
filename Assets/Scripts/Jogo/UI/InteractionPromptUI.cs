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
        private bool placeBelow;

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
            var nextTarget = visible ? target as Component : null;
            if (nextTarget != targetComponent) placeBelow = false;
            targetComponent = nextTarget;
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
            var world = InteractionAnchor.Position(targetComponent);
            if (player == null)
            {
                var controller = FindAnyObjectByType<MementoMori.Player.PlayerController>();
                if (controller != null) player = controller.transform;
            }
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, camera.WorldToScreenPoint(world), null, out var local))
                return;

            var halfHeight = background.rect.height * PromptScale * .5f;
            var halfWidth = background.rect.width * PromptScale * .5f;
            var bounds = parent.rect;
            // Keep object prompts above the room tutorial and the ornamental bottom edge.
            var bottomMargin = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Quarto"
                ? Mathf.Max(ScreenMargin, bounds.height * .17f) : ScreenMargin;
            local.x = Mathf.Clamp(local.x, bounds.xMin + halfWidth + ScreenMargin, bounds.xMax - halfWidth - ScreenMargin);
            var above = new Vector2(local.x, Mathf.Clamp(local.y + halfHeight + PromptGap,
                bounds.yMin + halfHeight + bottomMargin, bounds.yMax - halfHeight - ScreenMargin));
            var below = new Vector2(local.x, Mathf.Clamp(local.y - halfHeight - PromptGap,
                bounds.yMin + halfHeight + bottomMargin, bounds.yMax - halfHeight - ScreenMargin));
            // Keep the same side for this object. Crossing its pivot is not a reason to jump the box.
            // Switch only when the current placement actually covers the actor and the other is clear.
            if (player != null)
            {
                var actor = player.GetComponentInChildren<SpriteRenderer>();
                if (actor != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,
                        camera.WorldToScreenPoint(actor.bounds.min), null, out var actorMin)
                    && RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,
                        camera.WorldToScreenPoint(actor.bounds.max), null, out var actorMax))
                {
                    var actorRect = Rect.MinMaxRect(actorMin.x - 12f, actorMin.y - 12f, actorMax.x + 12f, actorMax.y + 12f);
                    bool CoversActor(Vector2 centre) => new Rect(centre.x - halfWidth, centre.y - halfHeight,
                        halfWidth * 2f, halfHeight * 2f).Overlaps(actorRect);
                    if (CoversActor(placeBelow ? below : above) && !CoversActor(placeBelow ? above : below))
                        placeBelow = !placeBelow;
                }
            }
            var placement = placeBelow ? below : above;
            background.localPosition = new Vector3(placement.x, placement.y, 0f);
        }

        private void SetPromptText(string action)
        {
            var key = InputBindings.Get(GameAction.Interact).ToString().ToUpperInvariant();
            var value = string.IsNullOrWhiteSpace(action) ? Loc.T("INTERAGIR") : Loc.T(action);
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
