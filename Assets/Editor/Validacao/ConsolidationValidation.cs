using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using MementoMori.V65;

/// <summary>Read-only scene inspection for the consolidation and future maintenance.</summary>
public static class ConsolidationValidation
{
    public static string AuditActiveScene()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Inspect the saved scene outside Play Mode.");
        var scene = SceneManager.GetActiveScene();
        if (scene.isDirty) throw new InvalidOperationException("Saved scene required; this audit never saves changes.");
        var transforms = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
        var world = transforms.Select(t => t.GetComponent<V65World>()).FirstOrDefault(w => w);
        var renderers = transforms.SelectMany(t => t.GetComponents<Renderer>()).ToArray();
        var shaders = renderers.SelectMany(r => r.sharedMaterials).Where(m => m && m.shader).Select(m => m.shader).Distinct().ToArray();
        var missing = transforms.Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
        var missingPrefabs = transforms.Count(t => PrefabUtility.IsPrefabAssetMissing(t.gameObject));
        var shaderErrors = shaders.Where(ShaderUtil.ShaderHasError).Select(s => s.name).ToArray();
        var colliders = transforms.SelectMany(t => t.GetComponents<Collider2D>()).Where(c => c.enabled && c.gameObject.activeInHierarchy).ToArray();
        var report = new {
            project = Application.dataPath, unity = Application.unityVersion, scene = scene.path,
            checkedAtUtc = DateTime.UtcNow.ToString("O"), objects = transforms.Length,
            regions = world ? world.regionIds : Array.Empty<string>(), missingScripts = missing, missingPrefabs,
            shaderErrors, shaders = shaders.Select(shader => shader.name).ToArray(), colliders = colliders.Length, triggers = colliders.Count(c => c.isTrigger),
            particles = transforms.Count(t => t.GetComponent<ParticleSystem>()),
            ambientComponents = transforms.Count(t => t.GetComponent<V65Ambient>()),
            animatedSpriteComponents = transforms.Select(t => t.GetComponent<V65Ambient>()).Count(a => a && a.frames != null && a.frames.Length > 1),
            dependencies = AssetDatabase.GetDependencies(scene.path, true).Length,
            buildScenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray()
        };
        Directory.CreateDirectory("TestResults/Consolidation");
        File.WriteAllText("TestResults/Consolidation/" + scene.name + "_audit.json", Newtonsoft.Json.JsonConvert.SerializeObject(report, Newtonsoft.Json.Formatting.Indented));
        if (missing != 0 || missingPrefabs != 0 || shaderErrors.Length != 0)
            throw new InvalidOperationException("Scene inspection failed; see its audit JSON.");
        return scene.path + ": missing scripts 0; missing prefabs 0; shader errors 0; regions " + (world ? world.regionIds.Length : 0);
    }
}
