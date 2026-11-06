using System.Collections.Generic;
using MementoMori.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.Interaction
{
    /// <summary>
    /// Presentation-only feedback for interaction:
    /// a pulsing lilac rim around the interactable the player is about to use, and soft light beacons
    /// on map connections (portals). Never changes interaction rules or input.
    /// </summary>
    public sealed class InteractionHighlighter : MonoBehaviour
    {
        private Material rimMaterial, beaconMaterial, sparkMaterial;
        private SpriteRenderer rim;
        private Object currentTarget;
        private readonly List<(Transform root, SpriteRenderer glow, ParticleSystem sparks)> beacons = new();

        public static void EnsureForScene()
        {
            var scene = SceneManager.GetActiveScene().name;
            if (scene != "Quarto" && scene != "Labirinto" && scene != "DominioLua" && scene != "FinalBeta") return;
            if (FindAnyObjectByType<InteractionHighlighter>() != null) return;
            new GameObject("InteractionHighlighter").AddComponent<InteractionHighlighter>();
        }

        private void Awake()
        {
            rimMaterial = Resources.Load<Material>("Presentation/FX/InteractionRim");
            beaconMaterial = Resources.Load<Material>("Presentation/FX/PortalBeacon");
            sparkMaterial = Resources.Load<Material>("Presentation/FX/PortalSpark");
        }

        private void Start()
        {
            if (beaconMaterial == null) return;
            foreach (var portal in FindObjectsByType<Portal>(FindObjectsSortMode.None)) AddBeacon(portal.transform);
        }

        private void AddBeacon(Transform portal)
        {
            var root = new GameObject("ConnectionBeacon").transform;
            root.SetParent(portal, false);
            var glow = new GameObject("Glow", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            glow.transform.SetParent(root, false);
            glow.sprite = WhiteSprite();
            glow.sharedMaterial = beaconMaterial;
            glow.transform.localScale = new Vector3(2.6f, 1.3f, 1f) / Mathf.Max(.01f, Mathf.Abs(portal.lossyScale.x));
            glow.sortingOrder = -15000;
            ParticleSystem sparks = null;
            if (sparkMaterial != null)
            {
                sparks = new GameObject("Sparks", typeof(ParticleSystem)).GetComponent<ParticleSystem>();
                sparks.transform.SetParent(root, false);
                sparks.transform.localScale = Vector3.one / Mathf.Max(.01f, Mathf.Abs(portal.lossyScale.x));
                var main = sparks.main;
                main.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.8f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(.15f, .4f);
                main.startSize = new ParticleSystem.MinMaxCurve(.06f, .13f);
                main.startColor = new Color(.78f, .62f, 1f, 1f);
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 40;
                main.gravityModifier = -.04f;
                var emission = sparks.emission; emission.rateOverTime = 5f;
                var shape = sparks.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(1.6f, .3f, 0f);
                var noise = sparks.noise; noise.enabled = true; noise.strength = .25f; noise.frequency = .6f;
                var color = sparks.colorOverLifetime; color.enabled = true;
                var gradient = new Gradient();
                gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                    new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, .25f), new GradientAlphaKey(0, 1) });
                color.color = gradient;
                var renderer = sparks.GetComponent<ParticleSystemRenderer>();
                renderer.sharedMaterial = sparkMaterial;
                renderer.sortingOrder = 30000;
            }
            beacons.Add((root, glow, sparks));
        }

        private void LateUpdate()
        {
            float t = Time.time;
            foreach (var b in beacons)
            {
                if (b.root == null) continue;
                bool selected = currentTarget != null && b.root.parent == ((Component)currentTarget).transform;
                var c = b.glow.color; c.a = (selected ? .95f : .55f) + .15f * Mathf.Sin(t * 1.7f); b.glow.color = c;
            }
            UpdateRim(t);
        }

        private void UpdateRim(float t)
        {
            var target = InteractionDetector.CurrentTarget as Component;
            currentTarget = target;
            var source = target != null ? FindVisual(target) : null;
            if (source == null || rimMaterial == null)
            {
                if (rim != null) rim.enabled = false;
                return;
            }
            if (rim == null)
            {
                rim = new GameObject("InteractionRim", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
                rim.sharedMaterial = rimMaterial;
            }
            if (rim.transform.parent != source.transform)
            {
                rim.transform.SetParent(source.transform, false);
                rim.transform.localRotation = Quaternion.identity;
            }
            rim.sprite = source.sprite;
            rim.flipX = source.flipX; rim.flipY = source.flipY;
            rim.sortingLayerID = source.sortingLayerID;
            rim.sortingOrder = source.sortingOrder - 1;
            // enlarge about the sprite centre so a ~0.07 u rim shows around the silhouette
            var size = source.sprite.bounds.size;
            var world = source.bounds.size;
            float k = 1f + .14f / Mathf.Max(.2f, Mathf.Max(world.x, world.y));
            var centre = source.sprite.bounds.center;
            rim.transform.localScale = new Vector3(k, k, 1f);
            rim.transform.localPosition = new Vector3(centre.x * (1 - k), centre.y * (1 - k), 0f);
            var color = rim.color; color.a = .55f + .35f * (.5f + .5f * Mathf.Sin(t * 4.2f)); rim.color = color;
            rim.enabled = source.enabled && source.gameObject.activeInHierarchy && size.x > 0f;
        }

        private static SpriteRenderer FindVisual(Component target)
        {
            if (target is Portal) return null; // portals use their beacon instead
            var own = target.GetComponent<SpriteRenderer>();
            if (own != null && own.sprite != null && own.enabled) return own;
            SpriteRenderer best = null; float area = 0f;
            foreach (var sr in target.GetComponentsInChildren<SpriteRenderer>())
            {
                if (sr.sprite == null || !sr.enabled || sr.name == "InteractionRim" || sr.sharedMaterial == null || sr.sharedMaterial.shader.name.StartsWith("Memento/FX")) continue;
                var s = sr.bounds.size; if (s.x * s.y > area) { area = s.x * s.y; best = sr; }
            }
            if (best != null) return best;
            // Interaction volumes are often separate from the approved art: use the smallest prop sprite under it.
            var at = (Vector2)target.transform.position; float smallest = float.MaxValue;
            foreach (var sr in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            {
                if (sr.sprite == null || !sr.enabled || sr.sortingOrder < 0 || sr.name == "InteractionRim") continue;
                if (sr.sharedMaterial != null && sr.sharedMaterial.shader.name.StartsWith("Memento/FX")) continue;
                if (sr.GetComponentInParent<MementoMori.Player.PlayerController>() != null) continue;
                var b = sr.bounds; var s = b.size;
                if (s.x > 5f || s.y > 5f) continue;
                var near = b.SqrDistance(new Vector3(at.x, at.y, b.center.z));
                if (near > .36f) continue;
                if (s.x * s.y < smallest) { smallest = s.x * s.y; best = sr; }
            }
            return best;
        }

        private static Sprite white;
        private static Sprite WhiteSprite()
        {
            if (white != null) return white;
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[64]; for (int i = 0; i < 64; i++) px[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(px); tex.Apply();
            white = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(.5f, .5f), 8f);
            return white;
        }
    }
}
