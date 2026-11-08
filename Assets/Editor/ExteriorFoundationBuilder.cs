using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Presentation-only pass for architecture floating over the void: renders the coverage mask of the
/// authored architecture, then lays masonry foundations under every exposed south edge and a soft
/// contact shadow along exposed east/west edges. Sorted between the void and the architecture, so
/// it never covers authored art, colliders, triggers or routes. Re-running replaces the previous bake.
/// </summary>
public static class ExteriorFoundationBuilder
{
    public const string RootName = "ExteriorFoundation_Generated";
    const string FoundationPath = "Assets/Ambientes/Compartilhado/Fundacoes/ExteriorFoundation.png";
    const string ShadowPath = "Assets/Ambientes/Compartilhado/Fundacoes/ExteriorEdgeShadow.png";
    const int MaskPixelsPerUnit = 16;
    const int MinRunPixels = 4;
    const string LunarBasePath = "Assets/Ambientes/Compartilhado/Fundacoes/LunarWaterlineBase.png";
    const float TuckUnder = .1f, ShadowWidth = .375f;
    const int FoundationOrder = -29900, ShadowOrder = -29950;

    [MenuItem("Memento Mori/Ambiente/Reconstruir fundações externas (Labirinto)")]
    static void BuildLabyrinthMenu() => Debug.Log(BuildLabyrinth());

    public static string BuildLabyrinth() => Build("V65_Environment",
        @"^(Labirinto_corridor_|L\d\d_fachada|L\d\d_facade|Stone coping|L\d\d_\d+$|L\d\d_layer_(piso|paredes|escadas|margem|sigilos))",
        @"^(AnimatedNegativeSpace|Flame_|EnvironmentalParticles|LabCorridorPlant|L\d\d_layer_(agua|nenufares))",
        Color.white, FoundationPath, 1.625f);

    [MenuItem("Memento Mori/Ambiente/Reconstruir bases externas (Domínio da Lua)")]
    static void BuildMoonDomainMenu() => Debug.Log(BuildMoonDomain());

    // Platforms of the Moon Domain stand in water: a wet base with a waterline instead of a void foundation.
    public static string BuildMoonDomain() => Build("V65_Environment",
        @"^(DominioLua_corridor_|M\d\d_fachada|M\d\d_\d+$|M\d\d_layer_(piso|paredes|escadas|margem|sigilos))",
        @"^(AnimatedNegativeSpace|Flame_|EnvironmentalParticles|LunarFireflies|ImpactSpray|lua-exterior-|M\d\d_layer_(agua|nenufares))",
        Color.white, LunarBasePath, .875f);

    public static string Build(string environmentRoot, string includePattern, string excludePattern, Color tint, string foundationPath, float FoundationDepth)
    {
        var scene = EditorSceneManager.GetActiveScene();
        var env = GameObject.Find(environmentRoot);
        if (env == null) return "environment root not found: " + environmentRoot;
        var old = GameObject.Find(RootName);
        if (old != null) Undo.DestroyObjectImmediate(old);

        var foundation = PrepareSprite(foundationPath, TextureWrapMode.Repeat);
        var shadow = PrepareSprite(ShadowPath, TextureWrapMode.Clamp);
        var include = new Regex(includePattern);
        var exclude = new Regex(excludePattern);

        // Coverage mask: only authored architecture renders, onto a transparent target.
        var toggled = new List<Renderer>();
        var architecture = new List<Renderer>();
        Material litMaterial = null;
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
            var keep = r.transform.IsChildOf(env.transform) && include.IsMatch(r.name) && !exclude.IsMatch(r.name)
                       && (r is TilemapRenderer || r is SpriteRenderer);
            if (keep) { architecture.Add(r); if (litMaterial == null && r is TilemapRenderer) litMaterial = r.sharedMaterial; }
            else { r.enabled = false; toggled.Add(r); }
        }
        if (architecture.Count == 0) { foreach (var r in toggled) r.enabled = true; return "no architecture renderers"; }
        var bounds = architecture[0].bounds;
        foreach (var r in architecture) if (r.bounds.size.sqrMagnitude > 0) bounds.Encapsulate(r.bounds);
        bounds.Expand(new Vector3(2, 2, 0));

        int w = Mathf.CeilToInt(bounds.size.x * MaskPixelsPerUnit), h = Mathf.CeilToInt(bounds.size.y * MaskPixelsPerUnit);
        var camGo = new GameObject("__FoundationMaskCam") { hideFlags = HideFlags.HideAndDontSave };
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = h / (2f * MaskPixelsPerUnit); cam.aspect = w / (float)h;
        cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10);
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0, 0, 0, 0);
        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt; cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply();
        RenderTexture.active = null; cam.targetTexture = null;
        Object.DestroyImmediate(rt); Object.DestroyImmediate(camGo);
        foreach (var r in toggled) r.enabled = true;

        var px = tex.GetPixels32();
        Object.DestroyImmediate(tex);
        var solid = new bool[w * h];
        int solidCount = 0;
        for (int i = 0; i < px.Length; i++) if (px[i].a > 127) { solid[i] = true; solidCount++; }
        bool S(int x, int y) => x >= 0 && y >= 0 && x < w && y < h && solid[y * w + x];
        var origin = new Vector2(bounds.center.x - w / (2f * MaskPixelsPerUnit), bounds.center.y - h / (2f * MaskPixelsPerUnit));

        var root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Exterior foundations");
        SceneManager_MoveToScene(root, scene);
        int south = 0, sides = 0;

        // South edges: solid pixel with void directly beneath -> visible face drops into the void.
        for (int y = 1; y < h; y++)
            for (int x = 0; x < w;)
            {
                if (!(S(x, y) && !S(x, y - 1))) { x++; continue; }
                int x0 = x; while (x < w && S(x, y) && !S(x, y - 1)) x++;
                if (x - x0 < MinRunPixels) continue;
                var left = origin.x + x0 / (float)MaskPixelsPerUnit; var len = (x - x0) / (float)MaskPixelsPerUnit;
                var edgeY = origin.y + y / (float)MaskPixelsPerUnit;
                var sr = Piece(root.transform, "Foundation", foundation, litMaterial, tint, FoundationOrder);
                sr.drawMode = SpriteDrawMode.Tiled; sr.tileMode = SpriteTileMode.Continuous;
                sr.size = new Vector2(len, FoundationDepth);
                sr.transform.position = new Vector3(left + len / 2f, edgeY + TuckUnder - FoundationDepth / 2f, 0);
                south++;
            }
        // East/west edges: soft contact shadow cast onto the void beside the platform.
        for (int dir = -1; dir <= 1; dir += 2)
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h;)
                {
                    if (!(S(x, y) && !S(x + dir, y))) { y++; continue; }
                    int y0 = y; while (y < h && S(x, y) && !S(x + dir, y)) y++;
                    if (y - y0 < MinRunPixels) continue;
                    var len = (y - y0) / (float)MaskPixelsPerUnit;
                    var edgeX = origin.x + (dir > 0 ? x + 1 : x) / (float)MaskPixelsPerUnit;
                    var sr = Piece(root.transform, dir > 0 ? "ShadowEast" : "ShadowWest", shadow, litMaterial, new Color(0, 0, 0, .85f), ShadowOrder);
                    sr.drawMode = SpriteDrawMode.Sliced;
                    sr.size = new Vector2(ShadowWidth, len);
                    sr.flipX = dir < 0;
                    sr.transform.position = new Vector3(edgeX + dir * ShadowWidth / 2f, origin.y + (y0 + y) / (2f * MaskPixelsPerUnit), 0);
                    sides++;
                }
        EditorSceneManager.MarkSceneDirty(scene);
        return $"mask {w}x{h} solid={solidCount} renderers={architecture.Count} south={south} sides={sides}";
    }

    static void SceneManager_MoveToScene(GameObject go, UnityEngine.SceneManagement.Scene scene)
    {
        if (go.scene != scene) UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
    }

    static SpriteRenderer Piece(Transform parent, string name, Sprite sprite, Material material, Color color, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite; sr.color = color; sr.sortingLayerName = "Default"; sr.sortingOrder = order;
        if (material != null) sr.sharedMaterial = material;
        return sr;
    }

    static Sprite PrepareSprite(string path, TextureWrapMode wrap)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteGenerateFallbackPhysicsShape = false;
        importer.SetTextureSettings(settings);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 64;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = wrap;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
