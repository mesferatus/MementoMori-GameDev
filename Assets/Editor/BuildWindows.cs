using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace MementoMori.EditorTools
{
    public static class BuildWindows
    {
        public const string Folder = "Builds/MementoMori_v1.0";

        /// <summary>
        /// Release player for Windows. LZ4HC compresses the data files without touching texture quality
        /// (the map layers stay full size and uncompressed in memory), which is what shrinks the download.
        /// </summary>
        [MenuItem("Memento Mori/Build/Windows (release)")]
        public static string Build()
        {
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (Directory.Exists(Folder)) Directory.Delete(Folder, true);
            Directory.CreateDirectory(Folder);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Folder + "/MementoMori.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.CompressWithLz4HC
            });
            foreach (var dir in Directory.GetDirectories(Folder, "*_DoNotShip")) Directory.Delete(dir, true);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"Windows build failed: {report.summary.result}");
            return $"{report.summary.result} {report.summary.totalSize} bytes, {report.summary.totalErrors} errors, {report.summary.totalWarnings} warnings, {report.summary.totalTime}";
        }
    }
}
