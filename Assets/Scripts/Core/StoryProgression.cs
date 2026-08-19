using System;
using System.Collections.Generic;
using UnityEngine;

namespace MementoMori.Core
{
    public enum CheckpointId { Sleep, AndrealphusI, Echoes, Garden, Mirrors, BeforeSigil, MoonDomain, LabyrinthStart, MoonDomainEntry, FinalPortal }

    /// <summary>Small, explicit session service used by graybox scenes and tests.</summary>
    public sealed class StoryProgression : MonoBehaviour
    {
        public static StoryProgression Instance { get; private set; }
        readonly Dictionary<string, int> puzzleProgress = new();
        readonly Dictionary<string, int> checkpointCounters = new();
        public CheckpointId LastCheckpoint { get; private set; }
        public event Action<CheckpointId> CheckpointChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
        }

        public int GetPuzzleProgress(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            return puzzleProgress.TryGetValue(id, out var value) ? value : GameState.Instance?.GetCounter("puzzle." + id) ?? 0;
        }
        public int SetPuzzleProgress(string id, int value)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            puzzleProgress[id] = Mathf.Max(0, value);
            GameState.Instance?.SetCounter("puzzle." + id, puzzleProgress[id]);
            return puzzleProgress[id];
        }
        public int IncrementPuzzleProgress(string id) => SetPuzzleProgress(id, GetPuzzleProgress(id) + 1);
        public int GetCheckpointCount(string id) => id != null && checkpointCounters.TryGetValue(id, out var value) ? value : 0;
        public void SaveCheckpoint(CheckpointId id)
        {
            LastCheckpoint = id;
            checkpointCounters[id.ToString()] = GetCheckpointCount(id.ToString()) + 1;
            GameState.Instance?.SaveCheckpoint();
            CheckpointChanged?.Invoke(id);
        }
        public void ResetSession()
        {
            puzzleProgress.Clear(); checkpointCounters.Clear(); LastCheckpoint = CheckpointId.Sleep;
        }
    }
}
