using System.Collections;
using System.Collections.Generic;
using MementoMori.Audio;
using MementoMori.Core;
using UnityEngine;

namespace MementoMori.World
{
    /// <summary>Runtime-safe dream sequence used only after the bed is unlocked.</summary>
    public sealed class DreamTransitionController : MonoBehaviour
    {
        readonly List<Transform> fragments = new();
        readonly List<Vector3> origins = new();

        public IEnumerator Play(float duration)
        {
            duration = Mathf.Clamp(duration, 30f, 50f);
            InputGate.Instance?.Block("DreamTransition");
            var camera = Camera.main;
            var originalScale = camera == null ? Vector3.one : camera.transform.localScale;
            try
            {
                CreateInvertedBedroom();
                CreateFragments();
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
                    for (var i = 0; i < fragments.Count; i++)
                    {
                        if (fragments[i] == null) continue;
                        var wave = Mathf.Sin(elapsed * (.7f + i * .09f)) * (AccessibilitySettings.Instance != null && AccessibilitySettings.Instance.ReduceFlashes ? .12f : .35f);
                        fragments[i].position = origins[i] + Vector3.up * wave + Vector3.right * (elapsed / duration - .5f) * .25f;
                    }
                    yield return null;
                }
            }
            finally
            {
                foreach (var fragment in fragments) if (fragment != null) Destroy(fragment.gameObject);
                fragments.Clear(); origins.Clear();
                if (camera != null) camera.transform.localScale = originalScale;
                InputGate.Instance?.Release("DreamTransition");
            }
        }

        void CreateInvertedBedroom()
        {
            var root = new GameObject("InvertedBedroomEcho");
            root.transform.position = transform.position;
            var labels = new[] { "A FOTOGRAFIA VAZIA", "A TIGELA CHEIA", "O GRIMÓRIO ABERTO", "A CAMA AFUNDA" };
            for (var i = 0; i < labels.Length; i++)
            {
                var go = new GameObject("InvertedBedroomEcho_" + i);
                go.transform.SetParent(root.transform, false);
                go.transform.localPosition = new Vector3(-2.4f + i * 1.6f, .5f + (i % 2) * .7f, 0f);
                var mesh = go.AddComponent<TextMesh>();
                mesh.text = labels[i]; mesh.fontSize = 28; mesh.characterSize = .055f;
                mesh.anchor = TextAnchor.MiddleCenter; mesh.alignment = TextAlignment.Center;
                mesh.color = new Color(.72f, .78f, .96f, .8f);
            }
            Destroy(root, 31f);
        }

        void CreateFragments()
        {
            var lines = new[] { "Ainda não.", "Encerrado.", "Uma imagem preserva…", "Um vínculo sustenta.", "Poe" };
            for (var i = 0; i < lines.Length; i++)
            {
                var go = new GameObject("DreamFragment_" + i);
                go.transform.position = transform.position + new Vector3(-2.5f + i * 1.15f, .8f + (i % 2) * 1.25f, 0f);
                var mesh = go.AddComponent<TextMesh>();
                mesh.text = lines[i];
                mesh.fontSize = 42;
                mesh.characterSize = .08f;
                mesh.anchor = TextAnchor.MiddleCenter;
                mesh.color = i == lines.Length - 1 ? new Color(.52f, .38f, .7f) : new Color(.75f, .8f, .98f);
                mesh.GetComponent<Renderer>().sortingOrder = 50;
                fragments.Add(go.transform); origins.Add(go.transform.position);
            }
        }
    }
}
