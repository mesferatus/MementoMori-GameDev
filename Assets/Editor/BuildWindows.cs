using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MementoMori.EditorTools
{
    public static class BuildWindows
    {
        [MenuItem("Memento Mori/Build/Windows Beta")]
        public static void Build()
        {
            // The evidence build uses Mono so Unity's IL2CPP linker does not pull in
            // package test assemblies that reference an unavailable Unsafe facade.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            var enabledScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (enabledScenes.Length == 0)
            {
                Debug.LogError("No enabled scenes found in Build Settings.");
                return;
            }

            Directory.CreateDirectory("Builds");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = enabledScenes,
                locationPathName = "Builds/MementoMori-validated.exe",
                target = BuildTarget.StandaloneWindows64,
                // CT evidence is intentionally compiled only into the Development build.
                // A release build never accepts the --ct-evidence switch.
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            });

            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"Windows build failed: {report.summary.result}");

            Debug.Log($"Memento Mori Windows build created at {report.summary.outputPath} ({report.summary.totalSize} bytes).");
        }

        // Clean demonstration package: release player, no development overlay or CT switch.
        [MenuItem("Memento Mori/Build/Windows Demo (release)")]
        public static string BuildDemo()
        {
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            var enabledScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            const string folder = "Builds/MementoMori_Demo";
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = enabledScenes,
                locationPathName = folder + "/MementoMori.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            var burst = Directory.GetDirectories(folder, "*_BurstDebugInformation_DoNotShip");
            foreach (var dir in burst) Directory.Delete(dir, true);
            return $"{report.summary.result} {report.summary.totalSize} bytes, {report.summary.totalErrors} errors, {report.summary.totalWarnings} warnings, {report.summary.totalTime}";
        }
    }
}
