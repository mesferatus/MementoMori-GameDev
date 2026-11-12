using System;
using System.Collections.Generic;
using UnityEngine;

namespace MementoMori.Core
{
    /// <summary>
    /// Lightweight runtime localization. PT-BR stays authored in place (scenes, JSON, code);
    /// English comes from two Resources tables: Localization/ui_en (exact PT text → EN) and
    /// Localization/lines_en (dialogue line id → EN). A missing entry falls back to PT, so the
    /// tables can grow screen by screen without ever breaking the game.
    /// </summary>
    public static class Loc
    {
        [Serializable] private sealed class Table { public Entry[] entries; }
        [Serializable] private sealed class Entry { public string id; public string pt; public string en; }

        private static Dictionary<string, string> ui;
        private static Dictionary<string, string> lines;
        public static event Action LanguageChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            ui = null;
            lines = null;
            LanguageChanged = null;
        }

        public static bool English => AccessibilitySettings.Instance != null
            && AccessibilitySettings.Instance.Language == "en";

        /// <summary>UI text by exact PT match; returns the PT text itself when not in English.</summary>
        public static string T(string pt)
        {
            if (!English || string.IsNullOrEmpty(pt)) return pt;
            Load();
            return ui.TryGetValue(pt, out var en) && !string.IsNullOrEmpty(en) ? en : pt;
        }

        /// <summary>Dialogue line by stable id; falls back to the UI table, then to the PT text.</summary>
        public static string Line(string id, string pt)
        {
            if (!English) return pt;
            Load();
            if (!string.IsNullOrEmpty(id) && lines.TryGetValue(id, out var en) && !string.IsNullOrEmpty(en))
                return en;
            return T(pt);
        }

        public static void NotifyLanguageChanged() => LanguageChanged?.Invoke();

        private static void Load()
        {
            if (ui != null) return;
            ui = Read("Localization/ui_en", entry => entry.pt);
            lines = Read("Localization/lines_en", entry => entry.id);
        }

        private static Dictionary<string, string> Read(string resource, Func<Entry, string> key)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            var asset = Resources.Load<TextAsset>(resource);
            if (asset == null) return result;
            Table table = null;
            try { table = JsonUtility.FromJson<Table>(asset.text); }
            catch (Exception exception) { Debug.LogWarning("Localization table unreadable: " + resource + " — " + exception.Message); }
            if (table?.entries == null) return result;
            foreach (var entry in table.entries)
            {
                var k = entry == null ? null : key(entry);
                if (!string.IsNullOrEmpty(k)) result[k] = entry.en;
            }
            return result;
        }
    }
}
