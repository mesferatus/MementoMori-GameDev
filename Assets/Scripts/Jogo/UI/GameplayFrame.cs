using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.UI
{
    /// <summary>Presentation-only border. Never participates in input or world geometry.</summary>
    public sealed class GameplayFrame : UnityEngine.UI.MaskableGraphic
    {
        public static void EnsureForScene()
        {
            var scene = SceneManager.GetActiveScene().name;
            if (scene != "Quarto" && scene != "Labirinto" && scene != "DominioLua" && scene != "FinalBeta") return;
            if (Object.FindAnyObjectByType<GameplayFrame>() != null) return;
            var prefab = Resources.Load<GameObject>("Presentation/GameplayFrame");
            if (prefab == null) return;
            var root = Object.Instantiate(prefab);
            root.name = "GameplayFrame";
            var frame = root.GetComponentInChildren<GameplayFrame>();
            // The prefab's EdgeShade lacked a CanvasRenderer, so the vignette was never drawn.
            if (!frame.TryGetComponent<CanvasRenderer>(out _)) frame.gameObject.AddComponent<CanvasRenderer>();
            // Visible but discreet vignette: darkens the outer band only, never the play centre.
            frame.color = new Color(.02f, .008f, .045f, scene == "Labirinto" ? .78f : scene == "DominioLua" ? .7f : .72f);
            var canvasRoot = frame.transform.parent != null ? frame.transform.parent : frame.transform;
            // The shader vignette replaces the flat ring mesh when its material is available.
            if (AddGothicVignette(canvasRoot, scene)) frame.enabled = false;
            AddGothicBorder(canvasRoot);
            frame.SetAllDirty();
        }

        // The approved lilac gothic frame of the cinematics, reused as the gameplay HUD border.
        private static void AddGothicBorder(Transform canvasRoot)
        {
            // V2: the lilac arabesque frame prepared in the Codex round (magenta cut-out halo removed).
            var sprite = Resources.Load<Sprite>("Presentation/MementoGothicFrameV2");
            var isV2 = sprite != null;
            if (sprite == null) sprite = Resources.Load<Sprite>("Presentation/MementoGothicFrame");
            if (sprite == null) return;
            foreach (Transform child in canvasRoot)
                if (child.name.StartsWith("Ornament")) child.gameObject.SetActive(false);
            var go = new GameObject("GothicBorder", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(canvasRoot, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            // Slightly taller than the screen: the rules sit closer to the edges (top ~7%, bottom ~4%).
            if (isV2) rect.offsetMin = rect.offsetMax = Vector2.zero;
            else { rect.offsetMin = new Vector2(0f, -22f); rect.offsetMax = new Vector2(0f, 30f); }
            var image = go.GetComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            image.type = UnityEngine.UI.Image.Type.Simple;
            image.color = new Color(1f, 1f, 1f, isV2 ? .95f : .82f);
            image.raycastTarget = false;
        }

        private static bool AddGothicVignette(Transform canvasRoot, string scene)
        {
            var material = Resources.Load<Material>("Presentation/GothicVignette");
            if (material == null) return false;
            var go = new GameObject("GothicVignette", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(canvasRoot, false);
            rect.SetAsFirstSibling();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<UnityEngine.UI.Image>();
            image.material = new Material(material);
            // Darker sectors keep more of their centre lit; the moon domain gets a bluer rim.
            // Darker purple rim across the whole game (Codex proposal, slightly stronger); the centre stays lit.
            image.material.SetFloat("_Inner", scene == "Labirinto" ? .47f : scene == "Quarto" ? .55f : .53f);
            image.material.SetFloat("_Intensity", 1f);
            image.material.SetFloat("_Outer", 1.12f);
            image.material.SetFloat("_Fog", .35f);
            image.material.SetColor("_Color", new Color(.024f, .008f, .055f, 1f));
            image.material.SetColor("_Glow", new Color(.25f, .1f, .4f, 1f));
            if (scene == "DominioLua") image.material.SetColor("_Glow", new Color(.16f, .14f, .48f, 1f));
            image.raycastTarget = false;
            return true;
        }

        protected override void Awake() { base.Awake(); raycastTarget = false; }

        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();
            var rect = GetPixelAdjustedRect();
            float depth = Mathf.Min(rect.width, rect.height) * .26f;
            const int steps = 12;
            for (int ring = 0; ring <= steps; ring++)
            {
                float t = ring / (float)steps;
                float inset = depth * t;
                var tint = color; tint.a *= 1f - Mathf.SmoothStep(0f, 1f, t);
                mesh.AddVert(new Vector3(rect.xMin + inset, rect.yMin + inset), tint, Vector2.zero);
                mesh.AddVert(new Vector3(rect.xMin + inset, rect.yMax - inset), tint, Vector2.zero);
                mesh.AddVert(new Vector3(rect.xMax - inset, rect.yMax - inset), tint, Vector2.zero);
                mesh.AddVert(new Vector3(rect.xMax - inset, rect.yMin + inset), tint, Vector2.zero);
                if (ring == 0) continue;
                for (int edge = 0; edge < 4; edge++)
                {
                    int a = (ring - 1) * 4 + edge, b = (ring - 1) * 4 + (edge + 1) % 4;
                    int c = ring * 4 + (edge + 1) % 4, d = ring * 4 + edge;
                    mesh.AddTriangle(a, b, c); mesh.AddTriangle(c, d, a);
                }
            }
        }
    }
}
