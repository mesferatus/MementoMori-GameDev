using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.World
{
    /// <summary>
    /// More life in the dream: every room of the Labyrinth and the Moon Domain gets the butterflies and
    /// fireflies that only some rooms had, plus a few pale moths drifting near the floor. Clones the
    /// authored particle systems, so the look stays the same; purely presentational.
    /// </summary>
    public sealed class AmbientCritters : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded -= OnLoaded;
            SceneManager.sceneLoaded += OnLoaded;
            OnLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void OnLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Labirinto" && scene.name != "DominioLua") return;
            var go = new GameObject("AmbientCritters");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.AddComponent<AmbientCritters>();
        }

        private IEnumerator Start()
        {
            // Region footprints are collected by the region title presenter on its first frames.
            yield return null; yield return null; yield return null;
            ParticleSystem butterfly = null, firefly = null;
            var existing = new System.Collections.Generic.HashSet<string>();
            foreach (var system in FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
            {
                if (system.gameObject.scene != gameObject.scene) continue;
                if (system.name.StartsWith("Butterflies_")) { butterfly ??= system; existing.Add(system.name); }
                else if (system.name.StartsWith("Fireflies_")) { firefly ??= system; existing.Add(system.name); }
            }
            var calm = MementoMori.Core.AccessibilitySettings.Instance != null && MementoMori.Core.AccessibilitySettings.Instance.ReduceFlashes;
            foreach (var region in MementoMori.UI.RegionTitlePresenter.MapRegions)
            {
                var centre = region.bounds.center; centre.z = 0f;
                if (butterfly != null && !existing.Contains("Butterflies_" + region.id))
                    Spawn(butterfly, "Butterflies_" + region.id, centre + new Vector3(-2f, 1f, 0f), null);
                if (firefly != null && !existing.Contains("Fireflies_" + region.id) && !calm)
                    Spawn(firefly, "Fireflies_" + region.id, centre, null);
                if (firefly != null)
                    Spawn(firefly, "Moths_" + region.id, centre + new Vector3(2f, -2f, 0f), new Color(.86f, .82f, .95f, .75f));
            }
            // Pure-white butterflies read as little glyphs over the dark stone: give them violet, half-lit wings.
            foreach (var system in GetComponentsInChildren<ParticleSystem>(true))
                if (system.name.StartsWith("Butterflies_")) Wings(system);
            foreach (var system in FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
                if (system.gameObject.scene == gameObject.scene && system.name.StartsWith("Butterflies_")) Wings(system);
        }

        private static void Wings(ParticleSystem system)
        {
            var main = system.main;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(.7f, .55f, .95f, .7f), new Color(.86f, .74f, 1f, .85f));
        }

        private void Spawn(ParticleSystem template, string name, Vector3 position, Color? moth)
        {
            var copy = Instantiate(template, position, template.transform.rotation, transform);
            copy.name = name;
            if (moth.HasValue)
            {
                // Moths: fewer, paler, slower and smaller than fireflies, with a wobbling flight.
                var main = copy.main;
                main.startColor = moth.Value;
                main.startSizeMultiplier *= .7f;
                main.startSpeedMultiplier *= .5f;
                main.maxParticles = Mathf.Max(3, main.maxParticles / 3);
                var emission = copy.emission;
                emission.rateOverTimeMultiplier *= .35f;
                var noise = copy.noise;
                noise.enabled = true; noise.strength = .6f; noise.frequency = .8f;
            }
            copy.Play();
        }
    }
}
