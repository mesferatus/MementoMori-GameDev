using MementoMori.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MementoMori.UI
{
    /// <summary>
    /// Hover/press feedback shared by every MenuButtonStyle button: a soft click on hover or
    /// keyboard focus, a firmer click on activation, and a slight grow while hovered.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MenuButtonFeedback : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private const float HoverScale = 1.04f;
        private float scaleTarget = 1f;
        // The grow factor this component has already applied, kept separate so layout code that
        // sets its own localScale (the small credits button, settings rows) is never overwritten.
        private float applied = 1f;
        private Button button;
        private bool hovered;
        private bool selected;

        private void Awake()
        {
            button = GetComponent<Button>();
            Rebind();
        }

        public void Rebind()
        {
            if (button == null) button = GetComponent<Button>();
            if (button == null) return;
            button.onClick.RemoveListener(Activate);
            button.onClick.AddListener(Activate);
        }

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(Activate);
        }

        private void OnEnable()
        {
            scaleTarget = 1f;
            hovered = selected = false;
            if (Mathf.Approximately(applied, 1f)) return;
            transform.localScale /= applied;
            applied = 1f;
        }

        private void Update()
        {
            var next = Mathf.MoveTowards(applied, scaleTarget, Time.unscaledDeltaTime * .45f);
            if (Mathf.Approximately(next, applied)) return;
            transform.localScale *= next / applied;
            applied = next;
        }

        private bool Usable => button == null || button.interactable;

        private void Focus()
        {
            if (!Usable) return;
            if (scaleTarget == HoverScale) return;
            scaleTarget = HoverScale;
            RuntimeAudio.PlayOneShot("06_interaction_click", .12f);
        }

        private void Unfocus() { if (!hovered && !selected) scaleTarget = 1f; }

        private void Activate()
        {
            if (!Usable) return;
            RuntimeAudio.PlayOneShot("06_interaction_click", .4f);
        }

        public void OnPointerEnter(PointerEventData eventData) { hovered = true; Focus(); }
        public void OnPointerExit(PointerEventData eventData) { hovered = false; Unfocus(); }
        public void OnSelect(BaseEventData eventData) { selected = true; Focus(); }
        public void OnDeselect(BaseEventData eventData) { selected = false; Unfocus(); }
    }
}
