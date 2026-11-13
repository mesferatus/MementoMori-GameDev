using MementoMori.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace MementoMori.Audio
{
    /// <summary>
    /// Scene ambience under the music: low wind in the Labyrinth, calm water in the Moon Domain, and a fire
    /// crackle that grows near the closest flame lights. XY distance drives volume and pan, so the camera Z
    /// offset never silences it in this 2D game.
    /// </summary>
    public sealed class AmbientSoundscape : MonoBehaviour
    {
        private const int CrackleVoices = 2;
        private const float CrackleRange = 6f;
        private AudioSource bed;
        private readonly AudioSource[] crackles = new AudioSource[CrackleVoices];
        private Light2D[] flames;
        private Transform listener;
        private float bedVolume, nextFlameScan;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded -= OnLoaded;
            SceneManager.sceneLoaded += OnLoaded;
            Attach(SceneManager.GetActiveScene());
        }

        private static void OnLoaded(Scene scene, LoadSceneMode mode) => Attach(scene);

        private static void Attach(Scene scene)
        {
            string clip; float volume;
            switch (scene.name)
            {
                case "Labirinto": clip = "26_wind_ambience_loop"; volume = .16f; break;
                case "DominioLua": clip = "27_water_ambience_loop"; volume = .2f; break;
                default: return;
            }
            if (FindAnyObjectByType<AmbientSoundscape>() != null) return;
            var go = new GameObject("AmbientSoundscape");
            SceneManager.MoveGameObjectToScene(go, scene);
            var soundscape = go.AddComponent<AmbientSoundscape>();
            soundscape.bed = soundscape.CreateSource(clip);
            soundscape.bedVolume = volume;
        }

        private AudioSource CreateSource(string clipName)
        {
            var clip = Resources.Load<AudioClip>("Audio/" + clipName);
            if (clip == null) return null;
            var source = gameObject.AddComponent<AudioSource>();
            var mixer = Resources.Load<UnityEngine.Audio.AudioMixer>("MementoMoriMixer");
            var groups = mixer == null ? null : mixer.FindMatchingGroups("SFX");
            if (groups != null && groups.Length > 0) source.outputAudioMixerGroup = groups[0];
            source.clip = clip; source.loop = true; source.playOnAwake = false; source.spatialBlend = 0f; source.volume = 0f;
            source.time = Random.Range(0f, clip.length * .9f);
            source.Play();
            return source;
        }

        private void Update()
        {
            var effects = AccessibilitySettings.Instance == null ? .65f : AccessibilitySettings.Instance.EffectsVolume;
            // Fade the bed in after a scene load instead of starting at full level.
            if (bed != null) bed.volume = Mathf.MoveTowards(bed.volume, bedVolume * effects, Time.unscaledDeltaTime * .1f);

            if (listener == null) listener = FindAnyObjectByType<MementoMori.Player.PlayerController>()?.transform;
            if (listener == null) return;
            if (flames == null || Time.unscaledTime >= nextFlameScan)
            {
                flames = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
                nextFlameScan = Time.unscaledTime + 5f;
            }
            // Two closest flame lights (braziers, candles, torches) get a crackle voice each.
            Light2D first = null, second = null; float d1 = float.MaxValue, d2 = float.MaxValue;
            var at = (Vector2)listener.position;
            foreach (var light in flames)
            {
                if (light == null || !light.isActiveAndEnabled || light.lightType != Light2D.LightType.Point) continue;
                var name = light.name;
                if (!name.Contains("Flame") && !name.Contains("Wick") && !name.Contains("Brazier")) continue;
                var d = Vector2.Distance(at, light.transform.position);
                if (d < d1) { second = first; d2 = d1; first = light; d1 = d; }
                else if (d < d2) { second = light; d2 = d; }
            }
            UpdateCrackle(0, first, d1, effects);
            UpdateCrackle(1, second, d2, effects);
        }

        private void UpdateCrackle(int index, Light2D flame, float distance, float effects)
        {
            if (crackles[index] == null) crackles[index] = CreateSource("25_brazier_crackle_loop");
            var voice = crackles[index];
            if (voice == null) return;
            var target = 0f;
            if (flame != null && distance < CrackleRange)
            {
                target = .22f * (1f - Mathf.SmoothStep(0f, 1f, distance / CrackleRange)) * effects;
                voice.panStereo = Mathf.Clamp((flame.transform.position.x - listener.position.x) / 5f, -.8f, .8f);
            }
            voice.volume = Mathf.MoveTowards(voice.volume, target, Time.unscaledDeltaTime * .5f);
        }
    }
}
