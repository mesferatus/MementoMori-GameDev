using UnityEngine;

namespace MementoMori.Core
{
    public sealed class AccessibilitySettings : MonoBehaviour
    {
        public static AccessibilitySettings Instance { get; private set; }
        public bool ReduceFlashes { get; private set; }
        public bool SoundCaptions { get; private set; } = true;
        public float FontScale { get; private set; } = 1f;
        public bool RemapInteractToSpace { get; private set; }
        public float TextSpeed { get; private set; } = 1f;
        public float MusicVolume { get; private set; } = .32f;
        public float EffectsVolume { get; private set; } = .65f;
        void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; DontDestroyOnLoad(gameObject); }
        void OnDestroy() { if (Instance == this) Instance = null; }
        public void SetReduceFlashes(bool value) => ReduceFlashes = value;
        public void SetSoundCaptions(bool value) => SoundCaptions = value;
        public void SetTextSpeed(float value) => TextSpeed = Mathf.Clamp(value, .25f, 4f);
        public void SetMusicVolume(float value) => MusicVolume = Mathf.Clamp01(value);
        public void SetEffectsVolume(float value) => EffectsVolume = Mathf.Clamp01(value);
        public void SetFontScale(float value) => FontScale = Mathf.Clamp(value, .8f, 1.6f);
        public void SetRemapInteractToSpace(bool value) => RemapInteractToSpace = value;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1)) SetReduceFlashes(!ReduceFlashes);
            if (Input.GetKeyDown(KeyCode.F2)) SetSoundCaptions(!SoundCaptions);
            if (Input.GetKeyDown(KeyCode.F3)) SetFontScale(FontScale >= 1.6f ? .8f : FontScale + .2f);
            if (Input.GetKeyDown(KeyCode.F4)) SetRemapInteractToSpace(!RemapInteractToSpace);
        }
    }
}
