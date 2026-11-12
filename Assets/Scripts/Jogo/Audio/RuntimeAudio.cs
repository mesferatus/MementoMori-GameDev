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
        AudioSource sourceB;
        float sourceWeight = 1f;
        float sourceBWeight;
        AudioSource chatterSource;
        TMP_Text caption;
        AudioMixer mixer;
        Coroutine captionRoutine;
        float baseChatterVolume;
        Coroutine musicTransition;
        Coroutine loopRoutine;
        AudioClip requestedMusic;
        float musicGain = 1f;
        public static void ApplyVolumeSettings()
        {
            if (active == null || AccessibilitySettings.Instance == null) return;
            active.UpdateMusicVolumes();
            if (active.chatterSource != null) active.chatterSource.volume = active.baseChatterVolume
                * AccessibilitySettings.Instance.VoiceVolume;
        }

        void UpdateMusicVolumes()
        {
            var setting = AccessibilitySettings.Instance == null ? .32f : AccessibilitySettings.Instance.MusicVolume;
            if (source != null) source.volume = sourceWeight * musicGain * setting;
            if (sourceB != null) sourceB.volume = sourceBWeight * musicGain * setting;
        }
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
            if (active.requestedMusic == clip
                && (active.musicTransition != null || active.source.isPlaying || active.sourceB.isPlaying)) return;
            active.requestedMusic = clip;
            if (active.musicTransition != null) active.StopCoroutine(active.musicTransition);
            active.musicTransition = active.StartCoroutine(active.ChangeMusic(clip));
        }

        IEnumerator ChangeMusic(AudioClip clip)
        {
            // The loop pair remains authoritative, including across rapid scene changes.
            if (source.isPlaying || sourceB.isPlaying) yield return FadeMusic(musicGain, 0f, .25f);
            if (loopRoutine != null) StopCoroutine(loopRoutine);
            source.Stop(); sourceB.Stop();
            sourceWeight = 1f; sourceBWeight = 0f;
            musicGain = 0f;
            UpdateMusicVolumes();
            loopRoutine = StartCoroutine(PlayLooped(clip));
            yield return FadeMusic(0f, 1f, .6f);
            musicTransition = null;
        }

        /// <summary>
        /// Two alternating sources overlap near the clip's end and crossfade, so the loop point
        /// no longer lands as a hard cut.
        /// </summary>
        IEnumerator PlayLooped(AudioClip clip)
        {
            var overlap = Mathf.Clamp(clip.length * .08f, .5f, 2.5f);
            // Short stingers keep the plain hardware loop; a crossfade would eat most of them.
            if (clip.length < 8f)
            {
                source.clip = clip; source.loop = true; source.Play();
                UpdateMusicVolumes();
                yield break;
            }
            var front = source; var back = sourceB;
            var frontIsA = true;
            var start = AudioSettings.dspTime + .05;
            front.clip = clip; front.loop = false; front.PlayScheduled(start);
            var handover = start + clip.length - overlap;
            while (true)
            {
                back.clip = clip; back.loop = false; back.PlayScheduled(handover);
                while (AudioSettings.dspTime < handover) { yield return null; }
                var fadeEnd = handover + overlap;
                while (AudioSettings.dspTime < fadeEnd)
                {
                    var blend = Mathf.Clamp01((float)((AudioSettings.dspTime - handover) / overlap));
                    sourceWeight = frontIsA ? 1f - blend : blend;
                    sourceBWeight = frontIsA ? blend : 1f - blend;
                    UpdateMusicVolumes();
                    yield return null;
                }
                front.Stop();
                (front, back) = (back, front);
                frontIsA = !frontIsA;
                sourceWeight = frontIsA ? 1f : 0f;
                sourceBWeight = frontIsA ? 0f : 1f;
                UpdateMusicVolumes();
                handover += clip.length - overlap;
            }
        }

        IEnumerator FadeMusic(float from, float to, float duration)
        {
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                musicGain = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / duration));
                UpdateMusicVolumes();
                yield return null;
            }
            musicGain = to;
            UpdateMusicVolumes();
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
            // Footsteps repeat every stride; captioning them spams the bottom of the screen.
            if (resourceName != "17_footstep_stone" || AccessibilitySettings.Instance != null && AccessibilitySettings.Instance.FootstepCaptions)
                runtimeAudio.ShowCaption(Loc.T(CaptionFor(resourceName)));
            Object.Destroy(go, clip.length + .1f);
        }

        public static void PlayChatter(string speaker, int syllable)
        {
            if (!Application.isPlaying) return;
            string folder;
            int count;
            float volume;
            switch (speaker)
            {
                case "Melantha": folder = "Dialogue/Melantha/melantha_dialogue_"; count = 4; volume = .16f; break;
                case "Andrealphus": folder = "Dialogue/Andrealphus/andrealphus_dialogue_"; count = 3; volume = .19f; break;
                case "Poe": folder = "Dialogue/Poe/poe_dialogue_"; count = 4; volume = .16f; break;
                default: return;
            }
            var clip = Resources.Load<AudioClip>(AudioResourceFolder + folder + (syllable % count + 1).ToString("00"));
            if (clip == null) return;
            var runtimeAudio = EnsureActive();
            runtimeAudio.chatterSource.Stop();
            runtimeAudio.baseChatterVolume = volume;
            runtimeAudio.chatterSource.volume = volume * (AccessibilitySettings.Instance == null ? 1f : AccessibilitySettings.Instance.VoiceVolume);
            runtimeAudio.chatterSource.clip = clip;
            runtimeAudio.chatterSource.Play();
        }

        public static void StopChatter()
        {
            if (active != null && active.chatterSource != null) active.chatterSource.Stop();
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
            sourceB = gameObject.AddComponent<AudioSource>();
            chatterSource = gameObject.AddComponent<AudioSource>();
            mixer = Resources.Load<AudioMixer>("MementoMoriMixer");
            var music = FindGroup("Music");
            source.outputAudioMixerGroup = music;
            sourceB.outputAudioMixerGroup = music;
            chatterSource.outputAudioMixerGroup = FindGroup("SFX");
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
            var rect = caption.rectTransform; rect.anchorMin = new Vector2(.12f, .075f); rect.anchorMax = new Vector2(.88f, .165f); rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
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
            "06_interaction_click" => "[clique suave]",
            "09_sigil_success" => "[runa ressoa]",
            "10_sigil_error" => "[runa desafina]",
            "05_transition_sleep" => "[respiração curta]",
            "13_fragment_collect" => "[fragmento cintila]",
            "19_beta_ending_stinger" => "[acorde final]",
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
