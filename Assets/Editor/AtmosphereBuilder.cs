using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MementoMori.EditorTools
{
    /// <summary>
    /// Reproducible atmosphere pass (menu Memento Mori/Ambiente/Atmosfera):
    /// FX materials, moonlight through the bedroom window, fireflies in garden areas and flickering flame halos.
    /// Everything it creates lives under one "Atmosphere_Generated" root per scene and is rebuilt on each run.
    /// </summary>
    public static class AtmosphereBuilder
    {
        private const string FxDir = "Assets/Ambientes/Compartilhado/FX";
        private const string ResDir = "Assets/Resources/Presentation/FX";
        private const string RootName = "Atmosphere_Generated";

        [MenuItem("Memento Mori/Ambiente/Atmosfera - materiais")]
        public static string EnsureAssets()
        {
            Directory.CreateDirectory(FxDir + "/Materiais"); Directory.CreateDirectory(ResDir);
            var whitePath = FxDir + "/FX_White.png";
            if (!File.Exists(whitePath))
            {
                var t = new Texture2D(8, 8, TextureFormat.RGBA32, false);
                t.SetPixels32(Enumerable.Repeat(new Color32(255, 255, 255, 255), 64).ToArray()); t.Apply();
                File.WriteAllBytes(whitePath, t.EncodeToPNG());
                AssetDatabase.ImportAsset(whitePath);
                var ti = (TextureImporter)AssetImporter.GetAtPath(whitePath);
                ti.textureType = TextureImporterType.Sprite; ti.spritePixelsPerUnit = 8; ti.filterMode = FilterMode.Point;
                ti.textureCompression = TextureImporterCompression.Uncompressed; ti.mipmapEnabled = false; ti.SaveAndReimport();
            }
            Mat("Assets/Resources/Presentation/GothicVignette.mat", "Memento/UI/GothicVignette", m => { });
            Mat(ResDir + "/InteractionRim.mat", "Memento/FX/SpriteSilhouetteGlow", m => { m.SetColor("_Color", new Color(.72f, .52f, 1f)); m.SetFloat("_Intensity", 1.1f); });
            Mat(ResDir + "/PortalBeacon.mat", "Memento/FX/Light", m => { m.SetFloat("_Shape", 1); m.SetColor("_Color", new Color(.62f, .42f, 1f)); m.SetFloat("_Intensity", .42f); m.SetVector("_Pixels", new Vector4(40, 20)); });
            Mat(ResDir + "/PortalSpark.mat", "Memento/FX/Light", m => { m.SetFloat("_Shape", 2); m.SetColor("_Color", Color.white); m.SetFloat("_Intensity", 1.3f); m.SetFloat("_Flicker", .4f); m.SetVector("_Pixels", new Vector4(8, 8)); });
            Mat(FxDir + "/Materiais/MoonShaft.mat", "Memento/FX/Light", m => { m.SetFloat("_Shape", 0); m.SetColor("_Color", new Color(.50f, .62f, 1f)); m.SetFloat("_Intensity", .42f); m.SetFloat("_TopWidth", .2f); m.SetFloat("_BottomWidth", .44f); m.SetFloat("_Skew", .18f); m.SetFloat("_Dust", .8f); });
            Mat(FxDir + "/Materiais/MoonPool.mat", "Memento/FX/Light", m => { m.SetFloat("_Shape", 1); m.SetColor("_Color", new Color(.46f, .58f, 1f)); m.SetFloat("_Intensity", .3f); m.SetVector("_Pixels", new Vector4(80, 36)); });
            Mat(FxDir + "/Materiais/FireflyGlow.mat", "Memento/FX/Light", m => { m.SetFloat("_Shape", 2); m.SetColor("_Color", Color.white); m.SetFloat("_Intensity", 2.8f); m.SetFloat("_Flicker", .55f); m.SetVector("_Pixels", new Vector4(10, 10)); });
            Mat(FxDir + "/Materiais/FlameHalo.mat", "Memento/FX/Light", m => { m.SetFloat("_Shape", 2); m.SetColor("_Color", Color.white); m.SetFloat("_Intensity", .32f); m.SetFloat("_Flicker", .85f); m.SetVector("_Pixels", new Vector4(56, 56)); });
            AssetDatabase.SaveAssets();
            return "fx assets ready";
        }

        private static void Mat(string path, string shader, System.Action<Material> setup)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            var s = Shader.Find(shader);
            if (s == null) throw new System.Exception("missing shader " + shader);
            if (m == null) { m = new Material(s); AssetDatabase.CreateAsset(m, path); }
            m.shader = s; setup(m); EditorUtility.SetDirty(m);
        }

        private static Sprite White => AssetDatabase.LoadAssetAtPath<Sprite>(FxDir + "/FX_White.png");
        private static Material Load(string name) => AssetDatabase.LoadAssetAtPath<Material>(FxDir + "/Materiais/" + name + ".mat");

        private static Transform FreshRoot()
        {
            var scene = EditorSceneManager.GetActiveScene();
            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == RootName)) Object.DestroyImmediate(old);
            return new GameObject(RootName).transform;
        }

        [MenuItem("Memento Mori/Ambiente/Atmosfera - cena aberta")]
        public static string BuildForActiveScene()
        {
            EnsureAssets();
            var scene = EditorSceneManager.GetActiveScene();
            var root = FreshRoot();
            string report = scene.name + ": ";
            switch (scene.name)
            {
                case "FinalBeta":
                    report += MoonWindow(root, new Vector2(.53f, 4.74f), 3.6f, 6.6f);
                    break;
                case "Quarto":
                    report += MoonWindow(root, new Vector2(.53f, 4.74f), 3.4f, 6.0f);
                    break;
                case "DominioLua":
                    report += Fireflies(root, "M02", 1.6f, new Color(.62f, .95f, 1f), new Color(.82f, .78f, 1f));
                    report += Fireflies(root, "M01", .55f, new Color(.62f, .95f, 1f), new Color(.86f, .8f, 1f));
                    report += Fireflies(root, "M08", .5f, new Color(.7f, .85f, 1f), new Color(.86f, .8f, 1f));
                    report += FlameHalos(root);
                    break;
                case "Labirinto":
                    report += Fireflies(root, "L06", .7f, new Color(.86f, 1f, .62f), new Color(1f, .9f, .6f));
                    report += Fireflies(root, "L12", .8f, new Color(.86f, 1f, .62f), new Color(.78f, .7f, 1f));
                    report += FlameHalos(root);
                    break;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            return report;
        }

        private static string MoonWindow(Transform root, Vector2 window, float height, float width)
        {
            // shaft top sits on the window sill and its top centre is under the window (the lean only moves the foot)
            var skew = Load("MoonShaft").GetFloat("_Skew");
            var shaft = Quad(root, "MoonShaft", Load("MoonShaft"), new Vector3(window.x + skew * .5f * width, window.y - height * .5f, 0f), new Vector2(width, height), 32000);
            shaft.sharedMaterial.SetVector("_Pixels", new Vector4(width * 16f, height * 16f));
            // pool where the shaft (skew .12) meets the floor
            var pool = Quad(root, "MoonPool", Load("MoonPool"), new Vector3(window.x + skew * width, window.y - height + .25f, 0f), new Vector2(width * .62f, 1.5f), 31990);
            return "moon shaft + pool; ";
        }

        private static SpriteRenderer Quad(Transform root, string name, Material m, Vector3 pos, Vector2 size, int order)
        {
            var go = new GameObject(name, typeof(SpriteRenderer)); go.transform.SetParent(root, false);
            go.transform.position = pos; go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.GetComponent<SpriteRenderer>(); sr.sprite = White; sr.sharedMaterial = m; sr.sortingOrder = order;
            return sr;
        }

        private static Bounds? SectorBounds(string prefix)
        {
            Bounds? total = null;
            foreach (var tm in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            {
                if (!tm.name.StartsWith(prefix + "_layer_piso")) continue;
                tm.CompressBounds(); if (tm.GetUsedTilesCount() == 0) continue;
                var b = new Bounds(tm.CellToWorld(tm.cellBounds.min), Vector3.zero); b.Encapsulate(tm.CellToWorld(tm.cellBounds.max));
                if (total == null) total = b; else { var t = total.Value; t.Encapsulate(b); total = t; }
            }
            return total;
        }

        private static string Fireflies(Transform root, string sector, float density, Color a, Color b)
        {
            var bounds = SectorBounds(sector);
            if (bounds == null) return sector + " not found; ";
            var area = bounds.Value;
            var ps = new GameObject("Fireflies_" + sector, typeof(ParticleSystem)).GetComponent<ParticleSystem>();
            ps.transform.SetParent(root, false);
            ps.transform.position = new Vector3(area.center.x, area.center.y, 0f);
            var main = ps.main;
            main.loop = true; main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(.04f, .16f);
            main.startSize = new ParticleSystem.MinMaxCurve(.2f, .34f);
            main.startColor = new ParticleSystem.MinMaxGradient(a, b);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = Mathf.CeilToInt(area.size.x * area.size.y * density * .12f) + 8;
            var emission = ps.emission; emission.rateOverTime = area.size.x * area.size.y * density * .016f;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(area.size.x * .92f, area.size.y * .88f, 0f);
            var noise = ps.noise; noise.enabled = true; noise.strength = .55f; noise.frequency = .22f; noise.scrollSpeed = .15f; noise.octaveCount = 2; noise.damping = true;
            var size = ps.sizeOverLifetime; size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0, 0), new Keyframe(.08f, 1), new Keyframe(.22f, .35f), new Keyframe(.34f, 1), new Keyframe(.5f, .55f),
                new Keyframe(.63f, 1), new Keyframe(.78f, .3f), new Keyframe(.88f, .95f), new Keyframe(1, 0)));
            var color = ps.colorOverLifetime; color.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, .1f), new GradientAlphaKey(.8f, .7f), new GradientAlphaKey(0, 1) });
            color.color = g;
            var r = ps.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = Load("FireflyGlow"); r.sortingOrder = 30000;
            return $"fireflies {sector} max {main.maxParticles}; ";
        }

        private static string FlameHalos(Transform root)
        {
            int n = 0;
            var parent = new GameObject("FlameHalos").transform; parent.SetParent(root, false);
            foreach (var flame in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            {
                if (!flame.name.StartsWith("Flame_") || flame.sprite == null || !flame.gameObject.activeInHierarchy) continue;
                var b = flame.bounds;
                var sr = Quad(parent, "Halo_" + flame.name, Load("FlameHalo"), new Vector3(b.center.x, b.center.y, 0f), Vector2.one * Mathf.Clamp(b.size.y * 4.2f, 1.2f, 2.6f), flame.sortingOrder - 1);
                sr.color = FlameColour(flame.sprite);
                n++;
            }
            return $"{n} flame halos; ";
        }

        private static Color FlameColour(Sprite sprite)
        {
            var tex = sprite.texture; var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            Graphics.Blit(tex, rt); var prev = RenderTexture.active; RenderTexture.active = rt;
            var r = sprite.textureRect; var o = new Texture2D((int)r.width, (int)r.height, TextureFormat.RGBA32, false);
            o.ReadPixels(new Rect(r.x, tex.height - r.y - r.height, r.width, r.height), 0, 0); o.Apply();
            RenderTexture.active = prev; RenderTexture.ReleaseTemporary(rt);
            Color sum = Color.black; float w = 0;
            foreach (var c in o.GetPixels()) { float k = c.a * Mathf.Max(c.r, Mathf.Max(c.g, c.b)); sum += c * k; w += k; }
            Object.DestroyImmediate(o);
            if (w <= 0) return new Color(1f, .62f, .3f);
            var avg = sum / w; Color.RGBToHSV(avg, out var h, out var s, out _);
            // keep a real flame tint: bright cores average out to near-white
            return Color.HSVToRGB(h, Mathf.Clamp(s * 1.4f, .55f, 1f), 1f);
        }
    }
}
