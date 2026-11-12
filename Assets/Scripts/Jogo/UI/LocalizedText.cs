using System;
using MementoMori.Core;
using TMPro;
using UnityEngine;

namespace MementoMori.UI
{
    /// <summary>
    /// Keeps one TMP label in the active language. The label's current text is captured once as
    /// the PT key (or a provider rebuilds the text), and the label follows language switches live.
    /// Only for static labels: text that other code rewrites should call Loc.T at the write site.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LocalizedText : MonoBehaviour
    {
        private TMP_Text label;
        private string portuguese;
        private Func<string> provider;

        public static void Bind(TMP_Text target, Func<string> textProvider = null)
        {
            if (target == null) return;
            var existing = target.GetComponent<LocalizedText>();
            if (existing == null) existing = target.gameObject.AddComponent<LocalizedText>();
            if (!existing.enabled)
            {
                existing.portuguese = target.text;
                existing.enabled = true;
            }
            if (textProvider != null) { existing.provider = textProvider; existing.Apply(); }
        }

        public static void Unbind(TMP_Text target)
        {
            var existing = target == null ? null : target.GetComponent<LocalizedText>();
            if (existing == null) return;
            existing.enabled = false;
            existing.provider = null;
            existing.portuguese = null;
        }

        private void Awake()
        {
            label = GetComponent<TMP_Text>();
            if (label != null) portuguese = label.text;
        }

        private void OnEnable()
        {
            Loc.LanguageChanged += Apply;
            Apply();
        }

        private void OnDisable() => Loc.LanguageChanged -= Apply;

        private void Apply()
        {
            if (label == null) return;
            if (provider != null) { label.text = provider(); return; }
            if (!string.IsNullOrEmpty(portuguese)) label.text = Loc.T(portuguese);
        }
    }
}
