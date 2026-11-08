using System;
using System.Collections.Generic;
using System.Text;
using MementoMori.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.World
{
    /// <summary>Verbatim entries supplied for the vertical slice; progress stays in GameState.</summary>
    public static class GrimoireCatalog
    {
        public sealed class Entry
        {
            public string Id;
            public string Title;
            public string Body;
            public string Note;
            public string[] Hints = Array.Empty<string>();
            public string Record;
            public int Category => Id[0] switch { 'A' => 0, 'R' => 1, 'P' => 2, _ => 3 };
        }

        private static readonly List<Entry> entries = new();
        private static bool loaded;
        public static IReadOnlyList<Entry> Entries { get { Load(); return entries; } }
        public static int Count { get { Load(); return entries.Count; } }

        private static void Load()
        {
            if (loaded) return;
            loaded = true;
            var source = Resources.Load<TextAsset>("Grimoire/CanonicalEntries");
            if (source == null) { Debug.LogError("Grimoire canonical entries resource is missing."); return; }
            Entry current = null;
            string section = null;
            var buffer = new StringBuilder();
            void Flush()
            {
                if (current == null || section == null) return;
                var value = buffer.ToString().TrimEnd('\r', '\n');
                switch (section)
                {
                    case "TEXT": current.Body = value; break;
                    case "NOTE": current.Note = value; break;
                    case "RECORD": current.Record = value; break;
                    case "HINT1": case "HINT2": case "HINT3":
                        var level = section[4] - '1';
                        var hints = current.Hints.Length == 3 ? current.Hints : new string[3];
                        hints[level] = value;
                        current.Hints = hints;
                        break;
                }
                buffer.Clear();
            }
            var seen = new HashSet<string>();
            foreach (var raw in source.text.Replace("\r\n", "\n").Split('\n'))
            {
                var line = raw.TrimEnd('\r');
                if (line.StartsWith("@", StringComparison.Ordinal))
                {
                    Flush();
                    var header = line.Substring(1).Split(new[] { '|' }, 2);
                    if (header.Length != 2 || !seen.Add(header[0]))
                    { Debug.LogError("Invalid or duplicate Grimoire entry: " + line); current = null; continue; }
                    current = new Entry { Id = header[0], Title = header[1] };
                    entries.Add(current);
                    section = null;
                }
                else if (line.StartsWith(">", StringComparison.Ordinal)) { Flush(); section = line.Substring(1); }
                else if (section != null) buffer.Append(line).Append('\n');
            }
            Flush();
            var counts = new int[4];
            foreach (var entry in entries)
            {
                if (entry.Hints.Length > 0)
                {
                    var hintCount = entry.Hints.Length;
                    while (hintCount > 0 && string.IsNullOrEmpty(entry.Hints[hintCount - 1])) hintCount--;
                    Array.Resize(ref entry.Hints, hintCount);
                }
                counts[entry.Category]++;
                if (string.IsNullOrEmpty(entry.Body) && (entry.Hints.Length == 0 || string.IsNullOrEmpty(entry.Hints[0])))
                    Debug.LogError("Grimoire entry has no canonical body: " + entry.Id);
            }
            if (entries.Count != 40 || counts[0] != 12 || counts[1] != 7 || counts[2] != 13 || counts[3] != 8)
                Debug.LogError($"Grimoire count mismatch: total={entries.Count}, A={counts[0]}, R={counts[1]}, P={counts[2]}, M={counts[3]}");
        }

        private static bool Flag(StoryFlag flag) => GameState.Instance != null && GameState.Instance.HasFlag(flag);
        private static bool Seen(string id) => GameState.Instance != null && GameState.Instance.GetCounter("grimoire.found." + id) > 0;
        public static void Discover(string id)
        {
            var state = GameState.Instance;
            if (state == null || state.GetCounter("grimoire.found." + id) > 0) return;
            state.SetCounter("grimoire.found." + id, 1);
            GrimoireScreen.NotifyNewEntry(id);
        }
        public static bool IsNew(string id) => IsUnlocked(id) && GameState.Instance != null && GameState.Instance.GetCounter("grimoire.read." + id) == 0;
        public static void MarkRead(string id) => GameState.Instance?.SetCounter("grimoire.read." + id, 1);

        public static void OnSceneEntered(string scene)
        {
            if (!Flag(StoryFlag.RoomGrimoireRead)) return;
            if (scene == "Labirinto") { Discover("A05"); Discover("R04"); }
            if (scene == "DominioLua") Discover("A08");
            if (scene == "FinalBeta") Discover("A12");
        }

        public static bool IsUnlocked(string id)
        {
            if (!Flag(StoryFlag.RoomGrimoireRead)) return false;
            return id switch
            {
                "A01" or "R01" => true,
                "A02" => Flag(StoryFlag.RoomPhotoExamined),
                "A03" => Flag(StoryFlag.PoeRevealed),
                "A04" => Seen("A04"),
                "A05" or "R04" => Seen("A05") || Flag(StoryFlag.LabyrinthAwakened),
                "A06" => Flag(StoryFlag.AndrealphusMeeting01Complete),
                "A07" or "P07" => Seen("A07"),
                "A08" => Seen("A08"),
                "A09" => Seen("A09"),
                "A10" or "R06" => Seen("A10"),
                "A11" or "M06" => Flag(StoryFlag.FragmentCollected),
                "A12" => Seen("A12"),
                "R02" => Seen("R02"),
                "R03" => Flag(StoryFlag.RoomWindowSecured),
                "R05" => Flag(StoryFlag.VoiceWellComplete) || Seen("R05"),
                "R07" or "P13" => Seen("R07"),
                "P01" => Seen("P01"),
                "P02" => Flag(StoryFlag.FalseDoorTriggered),
                "P03" => Seen("P03"),
                "P04" => Seen("P04"),
                "P05" => Seen("P05"),
                "P06" => Seen("P06"),
                "P08" => Seen("P08"),
                "P09" => Seen("P09"),
                "P10" => Seen("P10"),
                "P11" => Seen("P11"),
                "P12" => Seen("P12"),
                "M01" => GameState.Instance != null && GameState.Instance.RitualCompleted,
                "M02" => Flag(StoryFlag.VoiceWellComplete),
                "M03" => Flag(StoryFlag.EchoTrial03Complete),
                "M04" => Flag(StoryFlag.MirrorRoomSolved),
                "M05" => Flag(StoryFlag.MirrorBlackSolved),
                "M07" => Seen("M07"),
                "M08" => Seen("M08"),
                _ => false
            };
        }
    }
}
