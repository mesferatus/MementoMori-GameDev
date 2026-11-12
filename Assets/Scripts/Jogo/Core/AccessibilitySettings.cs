using UnityEngine;

namespace MementoMori.Core
{
    public sealed class AccessibilitySettings : MonoBehaviour
    {
        public static AccessibilitySettings Instance { get; private set; }
        public bool ReduceFlashes { get; private set; }
        public bool SoundCaptions { get; private set; } = true;
        public bool FootstepCaptions { get; private set; }
        public float FontScale { get; private set; } = 1f;
        public bool RemapInteractToSpace { get; private set; }
        public float TextSpeed { get; private set; } = 1f;
        public float MusicVolume { get; private set; } = .32f;
        public float EffectsVolume { get; private set; } = .65f;
        public float MasterVolume { get; private set; } = 1f;
        public float VoiceVolume { get; private set; } = 1f;
        public string Language { get; private set; } = "pt";
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            if (!Application.isPlaying) return;
            SetReduceFlashes(PlayerPrefs.GetInt("MM.Settings.ReduceFlashes", 0) != 0);
            SetSoundCaptions(PlayerPrefs.GetInt("MM.Settings.SoundCaptions", 1) != 0);
            SetFootstepCaptions(PlayerPrefs.GetInt("MM.Settings.FootstepCaptions", 0) != 0);
            SetTextSpeed(PlayerPrefs.GetFloat("MM.Settings.TextSpeed", 1f));
            SetMusicVolume(PlayerPrefs.GetFloat("MM.Settings.MusicVolume", .32f));
            SetEffectsVolume(PlayerPrefs.GetFloat("MM.Settings.EffectsVolume", .65f));
            SetMasterVolume(PlayerPrefs.GetFloat("MM.Settings.MasterVolume", 1f));
            SetVoiceVolume(PlayerPrefs.GetFloat("MM.Settings.VoiceVolume", 1f));
            SetFontScale(PlayerPrefs.GetFloat("MM.Settings.FontScale", 1f));
            SetRemapInteractToSpace(PlayerPrefs.GetInt("MM.Settings.InteractSpace", 0) != 0);
            Language = PlayerPrefs.GetString("MM.Settings.Language", "pt") == "en" ? "en" : "pt";
#if !UNITY_EDITOR
            if (PlayerPrefs.HasKey("MM.Settings.Fullscreen")) Screen.fullScreen = PlayerPrefs.GetInt("MM.Settings.Fullscreen") != 0;
            var width = PlayerPrefs.GetInt("MM.Settings.Width", 0);
            var height = PlayerPrefs.GetInt("MM.Settings.Height", 0);
            if (width > 0 && height > 0) Screen.SetResolution(width, height, Screen.fullScreen);
#endif
        }
        void OnDestroy() { if (Instance == this) Instance = null; }
        private static void Store(string key, float value) { if (Application.isPlaying) PlayerPrefs.SetFloat("MM.Settings." + key, value); }
        private static void Store(string key, bool value) { if (Application.isPlaying) PlayerPrefs.SetInt("MM.Settings." + key, value ? 1 : 0); }
        public void SetReduceFlashes(bool value) { ReduceFlashes = value; Store("ReduceFlashes", value); }
        public void SetSoundCaptions(bool value) { SoundCaptions = value; Store("SoundCaptions", value); }
        public void SetFootstepCaptions(bool value) { FootstepCaptions = value; Store("FootstepCaptions", value); }
        public void SetTextSpeed(float value) { TextSpeed = Mathf.Clamp(value, .25f, 4f); Store("TextSpeed", TextSpeed); }
        public void SetMusicVolume(float value) { MusicVolume = Mathf.Clamp01(value); Store("MusicVolume", MusicVolume); MementoMori.Audio.RuntimeAudio.ApplyVolumeSettings(); }
        public void SetEffectsVolume(float value) { EffectsVolume = Mathf.Clamp01(value); Store("EffectsVolume", EffectsVolume); }
        public void SetMasterVolume(float value) { MasterVolume = Mathf.Clamp01(value); Store("MasterVolume", MasterVolume); if (Application.isPlaying) AudioListener.volume = MasterVolume; }
        public void SetVoiceVolume(float value) { VoiceVolume = Mathf.Clamp01(value); Store("VoiceVolume", VoiceVolume); MementoMori.Audio.RuntimeAudio.ApplyVolumeSettings(); }
        public void SetFontScale(float value) { FontScale = Mathf.Clamp(value, .8f, 1.6f); Store("FontScale", FontScale); }
        public void SetRemapInteractToSpace(bool value) { RemapInteractToSpace = value; Store("InteractSpace", value); }
        public void SetLanguage(string value)
        {
            Language = value == "en" ? "en" : "pt";
            if (Application.isPlaying) PlayerPrefs.SetString("MM.Settings.Language", Language);
            Loc.NotifyLanguageChanged();
        }
        public void RestoreDefaults()
        {
            SetReduceFlashes(false); SetSoundCaptions(true); SetFootstepCaptions(false); SetTextSpeed(1f);
            SetMusicVolume(.32f); SetEffectsVolume(.65f); SetMasterVolume(1f); SetVoiceVolume(1f);
            SetFontScale(1f); SetRemapInteractToSpace(false);
            InputBindings.RestoreDefaults();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1)) SetReduceFlashes(!ReduceFlashes);
            if (Input.GetKeyDown(KeyCode.F2)) SetSoundCaptions(!SoundCaptions);
            if (Input.GetKeyDown(KeyCode.F3)) SetFontScale(FontScale >= 1.6f ? .8f : FontScale + .2f);
            if (Input.GetKeyDown(KeyCode.F4)) SetRemapInteractToSpace(!RemapInteractToSpace);
        }
    }
}
