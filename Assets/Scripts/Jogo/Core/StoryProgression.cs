using UnityEngine;

namespace MementoMori.Core
{
    public enum CheckpointId { Sleep, AndrealphusI, Echoes, Garden, Mirrors, BeforeSigil, MoonDomain, LabyrinthStart, MoonDomainEntry, FinalPortal }

    /// <summary>Compatibility facade over GameState's single progression store.</summary>
    public sealed class StoryProgression : MonoBehaviour
    {
        public static StoryProgression Instance { get; private set; }
        public CheckpointId LastCheckpoint => GameState.Instance == null ? CheckpointId.Sleep : GameState.Instance.LastCheckpoint;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
        }

        public int GetPuzzleProgress(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            return GameState.Instance?.GetPuzzleProgress(id) ?? 0;
        }
        public int SetPuzzleProgress(string id, int value)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            return GameState.Instance?.SetPuzzleProgress(id, value) ?? 0;
        }
        public int IncrementPuzzleProgress(string id) => SetPuzzleProgress(id, GetPuzzleProgress(id) + 1);
        public int GetCheckpointCount(string id)
        {
            if (string.IsNullOrEmpty(id) || GameState.Instance == null || !System.Enum.TryParse(id, out CheckpointId checkpoint)) return 0;
            return GameState.Instance.GetCheckpointCount(checkpoint);
        }

        public void SaveCheckpoint(CheckpointId id)
        {
            GameState.Instance?.SaveStoryCheckpoint(id);
        }
    }
}
