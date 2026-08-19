using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MementoMori.EditorTools
{
    /// <summary>Minimal command-line startup probe. It intentionally avoids scenes, prefabs and migrations.</summary>
    public static class UnityBatchLaunchDiagnostic
    {
        public static void Run()
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var sentinelPath = Path.Combine(projectRoot, "Logs", "f2-unity-launch-diagnostic.sentinel");
            Directory.CreateDirectory(Path.GetDirectoryName(sentinelPath));
            File.WriteAllText(sentinelPath, $"executeMethod reached {DateTime.UtcNow:O}{Environment.NewLine}");
            Debug.Log("[F2-UNITY-DIAGNOSTIC] executeMethod reached; sentinel written.");
            EditorApplication.Exit(73);
        }
    }
}
