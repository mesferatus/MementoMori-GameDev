using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MementoMori.Core
{
    [Serializable]
    public sealed class PuzzleManifest
    {
        public string[] labyrinthRequired;
        public string[] moonRequired;
        public string[] optional;
        public static PuzzleManifest Load()
        {
            var asset = Resources.Load<TextAsset>("Progression/V6Puzzles");
            if (asset == null) throw new InvalidOperationException("Missing V6 puzzle manifest.");
            return JsonUtility.FromJson<PuzzleManifest>(asset.text);
        }
    }

    /// <summary>Completion belongs to configured puzzle IDs, never to room counts.</summary>
    public sealed class PuzzleRegistry
    {
        public IReadOnlyList<string> LabyrinthIds { get; }
        public IReadOnlyList<string> MoonIds { get; }
        private readonly HashSet<string> known;
        private readonly HashSet<string> solved = new HashSet<string>(StringComparer.Ordinal);
        public event Action<string> PuzzleSolved;
        public PuzzleRegistry() : this(PuzzleManifest.Load()) {}
        public PuzzleRegistry(PuzzleManifest manifest)
        {
            if (manifest == null) throw new ArgumentNullException(nameof(manifest));
            var lab = manifest.labyrinthRequired ?? Array.Empty<string>();
            var moon = manifest.moonRequired ?? Array.Empty<string>();
            var all = lab.Concat(moon).Concat(manifest.optional ?? Array.Empty<string>()).ToArray();
            if (all.Any(string.IsNullOrWhiteSpace) || all.Distinct(StringComparer.Ordinal).Count() != all.Length)
                throw new ArgumentException("Puzzle IDs must be nonempty and globally unique.");
            known = new HashSet<string>(all, StringComparer.Ordinal);
            LabyrinthIds = Array.AsReadOnly((string[])lab.Clone());
            MoonIds = Array.AsReadOnly((string[])moon.Clone());
        }
        public bool IsKnown(string id) => id != null && known.Contains(id);
        public bool GetSolved(string id) => id != null && solved.Contains(id);
        public bool MarkSolved(string id)
        {
            if (id == null || !known.Contains(id)) throw new ArgumentException("Unknown puzzle ID", nameof(id));
            if (!solved.Add(id)) return false;
            PuzzleSolved?.Invoke(id);
            return true;
        }
        public bool AllSolved(IEnumerable<string> ids)
        {
            if (ids == null) return false;
            var required = ids.ToArray();
            return required.Length > 0 && required.All(id => id != null && known.Contains(id) && solved.Contains(id));
        }
        public bool LabyrinthGateOpen => AllSolved(LabyrinthIds);
        public bool MoonGateOpen => AllSolved(MoonIds);
        public int LabyrinthSolvedCount => LabyrinthIds.Count(GetSolved);
        public string[] Snapshot() => solved.OrderBy(id => id, StringComparer.Ordinal).ToArray();
        public void Restore(IEnumerable<string> ids)
        {
            solved.Clear();
            if (ids != null) foreach (var id in ids) if (id != null && known.Contains(id)) solved.Add(id);
        }
        public void Reset() => solved.Clear();
    }
}

