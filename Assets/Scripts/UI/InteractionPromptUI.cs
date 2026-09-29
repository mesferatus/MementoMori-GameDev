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
            if (labelTmp != null) labelTmp.text = value;
        }
    }
}
