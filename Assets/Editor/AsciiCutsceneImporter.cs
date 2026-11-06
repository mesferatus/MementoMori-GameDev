using System.IO;
using MementoMori.Dialogue;
using UnityEditor;
using UnityEngine;

namespace MementoMori.EditorTools
{
    /// <summary>
    /// Replaces the ASCII frames of the V6.1 illustrated sequences with regenerated compositions.
    /// Only asciiFrames/duration of beats with the same beforeLine change: ids, order, actions, choices,
    /// sounds and flags are preserved. Each original asset is copied to a backup folder first.
    /// </summary>
    public static class AsciiCutsceneImporter
    {
        [System.Serializable] private sealed class Beat { public int beforeLine; public string action; public float duration; public string[] frames; }
        [System.Serializable] private sealed class Sequence { public Beat[] beats; }

        public static string Import(string jsonDir, string backupDir, string ids, bool variantsToo)
        {
            Directory.CreateDirectory(backupDir);
            var report = new System.Text.StringBuilder();
            foreach (var id in ids.Split(','))
            {
                var assetPath = "Assets/Personagens/Resources/Cinematics/" + id + ".asset";
                var json = Path.Combine(jsonDir, id + ".json");
                if (!File.Exists(json)) { report.Append(id + ": no json; "); continue; }
                var backup = Path.Combine(backupDir, id + ".asset");
                if (!File.Exists(backup)) File.Copy(assetPath, backup);
                var seq = AssetDatabase.LoadAssetAtPath<IllustratedSequence>(assetPath);
                var data = JsonUtility.FromJson<Sequence>(File.ReadAllText(json));
                int replaced = 0, variants = 0;
                foreach (var beat in seq.beats)
                {
                    Beat match = null;
                    foreach (var b in data.beats) if (b.beforeLine == beat.beforeLine) { match = b; break; }
                    if (match == null || match.frames == null || match.frames.Length == 0) continue;
                    beat.asciiFrames = match.frames;
                    beat.duration = Mathf.Max(beat.duration, match.duration);
                    replaced++;
                    if (variantsToo && beat.choiceVariants != null)
                        foreach (var v in beat.choiceVariants) { v.asciiFrames = match.frames; variants++; }
                }
                EditorUtility.SetDirty(seq);
                report.Append($"{id}: {replaced}/{seq.beats.Length} beats, {variants} variants; ");
            }
            AssetDatabase.SaveAssets();
            return report.ToString();
        }

        public static string Restore(string backupDir, string ids)
        {
            foreach (var id in ids.Split(','))
            {
                var backup = Path.Combine(backupDir, id + ".asset");
                if (File.Exists(backup)) File.Copy(backup, "Assets/Personagens/Resources/Cinematics/" + id + ".asset", true);
            }
            AssetDatabase.Refresh();
            return "restored " + ids;
        }
    }
}
