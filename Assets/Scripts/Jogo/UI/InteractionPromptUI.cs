using MementoMori.Interaction;
using MementoMori.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MementoMori.UI
{
    // Runs after the camera follow so the prompt is placed against this frame's view, not the previous one.
    [DefaultExecutionOrder(10000)]
    public sealed class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text label;
        [SerializeField] private TMP_Text labelTmp;
        private string lastKey;
        private Component targetComponent;
        private Transform player;
        private bool placeBelow;
        private bool visibleTarget;
        private bool snapNext = true;

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
            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
            // Fade in where it will sit instead of popping; after a hidden spell it snaps to the new target.
            if (visible && (!visibleTarget || canvasGroup == null || canvasGroup.alpha < .05f)) snapNext = true;
            visibleTarget = visible;
            var nextTarget = visible ? target as Component : null;
            if (nextTarget != targetComponent) placeBelow = false;
            targetComponent = nextTarget;
            if (visible)
            {
                SetPromptText(target.InteractionVerb);
                PlaceNearTarget();
            }
        }

        private void LateUpdate()
        {
            // Switching between keyboard and controller relabels the key cap at once.
            if (targetComponent is IInteractable current && lastKey != null
                && lastKey != (GamepadInput.UsingGamepad ? "A" : InputBindings.Get(GameAction.Interact).ToString().ToUpperInvariant()))
                SetPromptText(current.InteractionVerb);
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, visibleTarget ? 1f : 0f, Time.unscaledDeltaTime * (visibleTarget ? 7f : 9f));
            if (visibleTarget) PlaceNearTarget();
        }

        private void PlaceNearTarget()
        {
            if (targetComponent == null || labelTmp == null) return;
            var background = labelTmp.transform.parent as RectTransform;
            var parent = background != null ? background.parent as RectTransform : null;
            var camera = Camera.main;
            if (parent == null || camera == null) return;

            background.localScale = Vector3.one * PromptScale;
            // Anchor on the object's own art when it has some: a wide trigger zone's centre drifts away from the prop.
            var world = InteractionAnchor.Position(targetComponent);
            var worldBottom = world;
            var art = targetComponent.GetComponentInChildren<SpriteRenderer>();
            if (player == null)
            {
                var controller = FindAnyObjectByType<MementoMori.Player.PlayerController>();
                if (controller != null) player = controller.transform;
            }
            if (art != null && art.enabled && art.sprite != null && art.bounds.size.x < 8f && art.bounds.size.y < 8f)
            {
                world = new Vector3(art.bounds.center.x, art.bounds.max.y, 0f);
                worldBottom = new Vector3(art.bounds.center.x, art.bounds.min.y, 0f);
            }
            else if (player != null && player.GetComponentInChildren<SpriteRenderer>() is SpriteRenderer self)
            {
                // Invisible choice zones (echo passages, doorways) have no art of their own: the prompt sits
                // over Melantha's head instead of at the far end of a wide trigger.
                world = new Vector3(self.bounds.center.x, self.bounds.max.y, 0f);
                worldBottom = new Vector3(self.bounds.center.x, self.bounds.min.y, 0f);
            }
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, camera.WorldToScreenPoint(world), null, out var local)
                || !RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, camera.WorldToScreenPoint(worldBottom), null, out var localBottom))
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
            var below = new Vector2(local.x, Mathf.Clamp(localBottom.y - halfHeight - PromptGap,
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
            // Glide between anchors (new target, side switch) instead of jumping; whole canvas units so
            // sub-pixel drift does not shimmer while the camera eases.
            Vector2 current = background.localPosition;
            var next = snapNext ? placement : Vector2.Lerp(current, placement, 1f - Mathf.Exp(-16f * Time.unscaledDeltaTime));
            snapNext = false;
            background.localPosition = new Vector3(Mathf.Round(next.x), Mathf.Round(next.y), 0f);
        }

        private void SetPromptText(string action)
        {
            var key = GamepadInput.UsingGamepad ? "A" : InputBindings.Get(GameAction.Interact).ToString().ToUpperInvariant();
            var value = string.IsNullOrWhiteSpace(action) ? Loc.T("INTERAGIR") : Loc.T(action);
            if (label != null) label.text = $"{key} - {value}";
            if (labelTmp != null && labelTmp.text == value && lastKey == key && labelTmp.transform.parent is Transform bg && bg.Find("Moldura") is Transform m && m.GetComponent<Image>() != null) return;
            lastKey = key;
            if (labelTmp != null) labelTmp.text = value;
            if (labelTmp == null) return;

            var background = labelTmp.transform.parent as RectTransform;
            if (background == null) return;
            // Standard HUD box (same frame as dialogue, objective, map and pause), sized to its text.
            var image = background.GetComponent<Image>();
            if (image != null) { image.sprite = null; image.color = new Color(.035f, .015f, .06f, .92f); image.preserveAspect = false; }
            HudStandardStyle.ClearOldFrames(background);
            MenuPanelStyle.FrameOnly(background);
            foreach (var star in background.GetComponentsInChildren<Image>(true))
                if (star.name == "Estrela" && star.sprite == null) star.sprite = MenuPanelStyle.Diamond();
            labelTmp.font = MenuPanelStyle.HeadingFont;
            labelTmp.enableAutoSizing = false; labelTmp.fontSize = 31f; labelTmp.characterSpacing = 3f;
            labelTmp.color = MenuPanelStyle.TitleColor;
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.textWrappingMode = TextWrappingModes.NoWrap;
            labelTmp.ForceMeshUpdate();
            const float height = 76f;
            var width = Mathf.Clamp(labelTmp.preferredWidth + 70f, 200f, 760f);
            background.sizeDelta = new Vector2(width, height);
            labelTmp.rectTransform.anchorMin = Vector2.zero; labelTmp.rectTransform.anchorMax = Vector2.one;
            labelTmp.rectTransform.offsetMin = new Vector2(30f, 6f); labelTmp.rectTransform.offsetMax = new Vector2(-30f, -6f);
            var keycap = background.Find("Keycap_E") as RectTransform;
            if (keycap != null)
            {
                var cap = keycap.GetComponent<Image>();
                if (cap != null) { cap.sprite = null; cap.color = new Color(.05f, .022f, .078f, .96f); cap.preserveAspect = false; }
                var face = keycap.Find("KeyFace"); if (face != null) face.gameObject.SetActive(false);
                HudStandardStyle.ClearOldFrames(keycap);
                MenuPanelStyle.FrameOnly(keycap);
                keycap.anchorMin = keycap.anchorMax = new Vector2(0f, .5f);
                keycap.sizeDelta = new Vector2(58f, 58f);
                keycap.anchoredPosition = new Vector2(-40f, 0f);
                keycap.pivot = new Vector2(1f, .5f);
                var keyLabel = keycap.GetComponentInChildren<TMP_Text>(true);
                if (keyLabel == null)
                {
                    // The keycap art had its letter painted in; the painted E is covered, so the live key needs a label.
                    keyLabel = new GameObject("KeyLabel", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
                    keyLabel.transform.SetParent(keycap, false);
                    keyLabel.font = MenuPanelStyle.HeadingFont; keyLabel.color = MenuPanelStyle.TitleColor; keyLabel.raycastTarget = false;
                }
                keyLabel.gameObject.SetActive(true);
                if (keyLabel != null)
                {
                    keyLabel.text = key; keyLabel.transform.SetAsLastSibling();
                    keyLabel.enableAutoSizing = true; keyLabel.fontSizeMin = 12f; keyLabel.fontSizeMax = 30f;
                    keyLabel.alignment = TextAlignmentOptions.Center;
                    keyLabel.rectTransform.anchorMin = Vector2.zero; keyLabel.rectTransform.anchorMax = Vector2.one;
                    keyLabel.rectTransform.offsetMin = new Vector2(8f, 6f); keyLabel.rectTransform.offsetMax = new Vector2(-8f, -6f);
                    keyLabel.font = MenuPanelStyle.HeadingFont;
                }
            }
            var icon = background.Find("InteractionIcon");
            if (icon != null) icon.gameObject.SetActive(false);
        }

    }
}