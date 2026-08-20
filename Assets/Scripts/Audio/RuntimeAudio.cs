using UnityEngine;
using MementoMori.Core;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using System.Collections;

namespace MementoMori.Audio
{
    public sealed class RuntimeAudio : MonoBehaviour
    {
        private const string AudioResourceFolder = "Audio/";
        static RuntimeAudio active;
        AudioSource source;
        TMP_Text caption;
        AudioMixer mixer;
        Coroutine captionRoutine;
        public static void PlayLoop(string resourceName)
        {
            if (!Application.isPlaying) return;
            var clip = Resources.Load<AudioClip>(AudioResourceFolder + resourceName);
            if (clip == null) return;
            if (active == null)
            {
                var go = new GameObject("RuntimeAudio");
                DontDestroyOnLoad(go); active = go.AddComponent<RuntimeAudio>(); active.Initialize();
            }
            if (active.source.clip == clip && active.source.isPlaying) return;
            active.source.clip = clip; active.source.loop = true; active.source.volume = AccessibilitySettings.Instance == null ? .32f : AccessibilitySettings.Instance.MusicVolume; active.source.outputAudioMixerGroup = active.FindGroup("Music"); active.source.Play();
        }
        public static void PlayOneShot(string resourceName, float volume = .65f)
        {
            if (!Application.isPlaying) return;
            var clip = Resources.Load<AudioClip>(AudioResourceFolder + resourceName);
            if (clip == null) return;
            var runtimeAudio = EnsureActive();
            var go = new GameObject("RuntimeOneShot");
            var source = go.AddComponent<AudioSource>();
            source.volume = volume * (AccessibilitySettings.Instance == null ? 1f : AccessibilitySettings.Instance.EffectsVolume);
            source.outputAudioMixerGroup = runtimeAudio.FindGroup("SFX");
            source.PlayOneShot(clip);
            runtimeAudio.ShowCaption(CaptionFor(resourceName));
            Object.Destroy(go, clip.length + .1f);
        }

        static RuntimeAudio EnsureActive()
        {
            if (active != null) return active;
            var go = new GameObject("RuntimeAudio");
            DontDestroyOnLoad(go); active = go.AddComponent<RuntimeAudio>(); active.Initialize();
            return active;
        }

        void Initialize()
        {
            source = gameObject.AddComponent<AudioSource>();
            mixer = Resources.Load<AudioMixer>("MementoMoriMixer");
            CreateCaptionOverlay();
        }

        AudioMixerGroup FindGroup(string name)
        {
            if (mixer == null) return null;
            var groups = mixer.FindMatchingGroups(name);
            return groups.Length == 0 ? null : groups[0];
        }

        void CreateCaptionOverlay()
        {
            var canvasObject = new GameObject("SoundCaptions", typeof(Canvas), typeof(CanvasGroup));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 80;
            caption = new GameObject("Caption", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
            caption.transform.SetParent(canvasObject.transform, false); caption.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF"); caption.fontSize = 18; caption.alignment = TextAlignmentOptions.Center; caption.color = Color.white;
            var rect = caption.rectTransform; rect.anchorMin = new Vector2(.12f, .05f); rect.anchorMax = new Vector2(.88f, .14f); rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            caption.gameObject.SetActive(false);
        }

        void ShowCaption(string value)
        {
            if (caption == null || AccessibilitySettings.Instance != null && !AccessibilitySettings.Instance.SoundCaptions) return;
            caption.text = value; caption.gameObject.SetActive(true); if (captionRoutine != null) StopCoroutine(captionRoutine); captionRoutine = StartCoroutine(HideCaption());
        }

        IEnumerator HideCaption() { yield return new WaitForSecondsRealtime(1.6f); if (caption != null) caption.gameObject.SetActive(false); }

        static string CaptionFor(string id) => id switch
        {
            "07_dialogue_blip" => "[pena risca o papel]",
            "05_transition_sleep" => "[respiração curta]",
            "14_poe_appear" or "15_poe_soft_call" => "[Poe mia]",
            "17_footstep_stone" => "[passos na pedra]",
            "18_candle_extinguish" => "[vela se apaga]",
            "11_mirror_shimmer" => "[espelho vibra]",
            "12_portal_open" => "[sino distante]",
            "16_daimon_appear" => "[presença próxima]",
            _ => "[som]"
        };
    }
}
