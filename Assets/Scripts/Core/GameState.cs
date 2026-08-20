using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

namespace MementoMori.Core
{
    public enum StoryFlag
    {
        RoomBowlExamined, RoomToyExamined, RoomPhotoExamined, RoomWindowSecured, ToyEchoTouched,
        RoomCandlesDone, RoomRitualItemStored, RoomGrimoireRead, RoomSleepUnlocked,
        DreamTransitionComplete, LabyrinthAwakened, FalseDoorTriggered, PoeRevealed, AndrealphusMeeting01Complete,
        EchoTrial01Complete, EchoTrial02Complete, EchoTrial03Complete, EmptyChamberComplete, VoiceWellComplete,
        GardenPetalCrescente, GardenPetalCheia, GardenPetalMinguante, GardenComplete,
        MirrorPuzzleComplete, MirrorPresentSolved, MirrorDelayedSolved, MirrorAheadSolved,
        MirrorAbsentSolved, MirrorDoubleSolved, MirrorRoomSolved, MirrorBlackSolved, AndrealphusMeeting02Complete,
        HiddenDoorRevealed, SigilPuzzleComplete, FragmentCollected, MoonDomainUnlocked
    }

    /// <summary>Stores session-only progression flags for the beta.</summary>
    public sealed class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        public bool RitualCompleted { get; private set; }
        public bool PoeRevealed { get; private set; }
        public bool MirrorPuzzleSolved { get; private set; }
        public bool SigilPuzzleSolved { get; private set; }
        public bool FragmentCollected { get; private set; }
        private readonly System.Collections.Generic.HashSet<StoryFlag> flags = new();
        private readonly System.Collections.Generic.Dictionary<string, int> counters = new();
        private readonly System.Collections.Generic.Dictionary<string, int> puzzleProgress = new();
        private readonly System.Collections.Generic.Dictionary<string, int> checkpointCounters = new();
        public string CheckpointScene { get; private set; }
        public Vector2 CheckpointPosition { get; private set; }
        public CheckpointId LastCheckpoint { get; private set; }
        public event Action<StoryFlag, bool> OnFlagChanged;
        public event Action<CheckpointId> CheckpointChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void StartNewGame() => ResetSession();

        public void ResetSession()
        {
            flags.Clear();
            counters.Clear();
            puzzleProgress.Clear();
            checkpointCounters.Clear();
            LastCheckpoint = CheckpointId.Sleep;
            CheckpointScene = null;
            RitualCompleted = false;
            PoeRevealed = false;
            MirrorPuzzleSolved = false;
            SigilPuzzleSolved = false;
            FragmentCollected = false;
            CheckpointPosition = Vector2.zero;
            PlayerPrefs.DeleteKey("MementoMori.Checkpoint");
        }

        public bool HasFlag(StoryFlag flag) => flags.Contains(flag);
        public void SetFlag(StoryFlag flag, bool value = true)
        {
            if (value) flags.Add(flag); else flags.Remove(flag);
            OnFlagChanged?.Invoke(flag, value);
        }

        public int GetCounter(string id) => id != null && counters.TryGetValue(id, out var value) ? value : 0;
        public int IncrementCounter(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            counters[id] = GetCounter(id) + 1;
            return counters[id];
        }
        public void SetCounter(string id, int value)
        {
            if (string.IsNullOrEmpty(id)) return;
            counters[id] = Mathf.Max(0, value);
        }

        public int GetPuzzleProgress(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            return puzzleProgress.TryGetValue(id, out var value) ? value : GetCounter("puzzle." + id);
        }

        public int SetPuzzleProgress(string id, int value)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            puzzleProgress[id] = Mathf.Max(0, value);
            SetCounter("puzzle." + id, puzzleProgress[id]);
            return puzzleProgress[id];
        }

        public int IncrementPuzzleProgress(string id) => SetPuzzleProgress(id, GetPuzzleProgress(id) + 1);

        public int GetCheckpointCount(CheckpointId id) => checkpointCounters.TryGetValue(id.ToString(), out var value) ? value : 0;

        public void SaveStoryCheckpoint(CheckpointId id)
        {
            LastCheckpoint = id;
            checkpointCounters[id.ToString()] = GetCheckpointCount(id) + 1;
            SaveCheckpoint();
            CheckpointChanged?.Invoke(id);
        }
        public void SaveCheckpoint() => SaveCheckpoint(SceneManager.GetActiveScene().name, Vector2.zero);
        public void SaveCheckpoint(string sceneName) => SaveCheckpoint(sceneName, Vector2.zero);
        public void SaveCheckpoint(string sceneName, Vector2 safePosition)
        {
            CheckpointScene = sceneName;
            CheckpointPosition = safePosition;
            var snapshot = new CheckpointSnapshot
            {
                scene = sceneName,
                position = safePosition,
                ritualCompleted = RitualCompleted,
                poeRevealed = PoeRevealed,
                mirrorPuzzleSolved = MirrorPuzzleSolved,
                sigilPuzzleSolved = SigilPuzzleSolved,
                fragmentCollected = FragmentCollected,
                flags = new List<StoryFlag>(flags).ConvertAll(flag => (int)flag).ToArray(),
                counterIds = new List<string>(counters.Keys).ToArray(),
                counterValues = new List<int>(counters.Values).ToArray()
            };
            PlayerPrefs.SetString("MementoMori.Checkpoint", JsonUtility.ToJson(snapshot));
            PlayerPrefs.Save();
        }

        public bool RestoreCheckpoint()
        {
            var json = PlayerPrefs.GetString("MementoMori.Checkpoint", string.Empty);
            if (string.IsNullOrEmpty(json)) return false;
            var snapshot = JsonUtility.FromJson<CheckpointSnapshot>(json);
            if (snapshot == null || string.IsNullOrEmpty(snapshot.scene)) return false;
            flags.Clear(); counters.Clear();
            if (snapshot.flags != null) foreach (var value in snapshot.flags) flags.Add((StoryFlag)value);
            if (snapshot.counterIds != null && snapshot.counterValues != null)
                for (var i = 0; i < Mathf.Min(snapshot.counterIds.Length, snapshot.counterValues.Length); i++) counters[snapshot.counterIds[i]] = snapshot.counterValues[i];
            CheckpointScene = snapshot.scene; CheckpointPosition = snapshot.position;
            RitualCompleted = snapshot.ritualCompleted; PoeRevealed = snapshot.poeRevealed; MirrorPuzzleSolved = snapshot.mirrorPuzzleSolved;
            SigilPuzzleSolved = snapshot.sigilPuzzleSolved; FragmentCollected = snapshot.fragmentCollected;
            return true;
        }

        public void SetRitualCompleted() => RitualCompleted = true;
        public void SetPoeRevealed() => PoeRevealed = true;
        public void SetMirrorPuzzleSolved() => MirrorPuzzleSolved = true;
        public void SetSigilPuzzleSolved() => SigilPuzzleSolved = true;
        public void SetFragmentCollected() { FragmentCollected = true; SetFlag(StoryFlag.FragmentCollected); }

        [Serializable]
        private sealed class CheckpointSnapshot
        {
            public string scene;
            public Vector2 position;
            public bool ritualCompleted, poeRevealed, mirrorPuzzleSolved, sigilPuzzleSolved, fragmentCollected;
            public int[] flags;
            public string[] counterIds;
            public int[] counterValues;
        }
    }
}
