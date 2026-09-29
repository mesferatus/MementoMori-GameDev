using System.Collections;
using MementoMori.Audio;
using MementoMori.Core;
using UnityEngine;

namespace MementoMori.World
{
    /// <summary>Runtime-safe dream sequence used only after the bed is unlocked.</summary>
    public sealed class DreamTransitionController : MonoBehaviour
    {
        public IEnumerator Play(float duration)
        {
            duration = Mathf.Clamp(duration, 30f, 50f);
            InputGate.Instance?.Block("DreamTransition");
            var camera = Camera.main;
            var originalScale = camera == null ? Vector3.one : camera.transform.localScale;
            try
            {
                RuntimeAudio.PlayOneShot("05_transition_sleep", AccessibilitySettings.Instance != null && AccessibilitySettings.Instance.ReduceFlashes ? .45f : .8f);
                var elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    if (camera != null)
                    {
                        var progress = Mathf.Clamp01(elapsed / duration);
                        camera.transform.localScale = new Vector3(Mathf.Lerp(1f, -1f, progress), Mathf.Lerp(1f, -1f, progress), 1f);
                    }
                    yield return null;
                }
            }
            finally
            {
                if (camera != null) camera.transform.localScale = originalScale;
                InputGate.Instance?.Release("DreamTransition");
            }
        }
    }
}
