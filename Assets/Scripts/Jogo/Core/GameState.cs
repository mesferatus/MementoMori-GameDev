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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureForDirectScene()
        {
            var scene = SceneManager.GetActiveScene().name;
            if (scene != "Quarto" && scene != "Labirinto" && scene != "DominioLua" && scene != "FinalBeta") return;
            if (Instance == null) new GameObject("GameState (Direct Scene)").AddComponent<GameState>();
            // A technical scene bootstrap must never grant narrative progress.
            MementoMori.World.GrimoireScreen.EnsureRuntimeForScene(SceneManager.GetActiveScene());
        }

        private PuzzleRegistry puzzles;
        public PuzzleRegistry Puzzles => puzzles ??= new PuzzleRegistry();
        public bool RitualCompleted { get; private set; }
        public bool PoeRevealed { get; private set; }
        public bool MirrorPuzzleSolved { get; private set; }
        public bool SigilPuzzleSolved { get; private set; }
        public bool FragmentCollected { get; private set; }
        private readonly System.Collections.Generic.HashSet<StoryFlag> flags = new();
        private readonly System.Collections.Generic.Dictionary<string, int> counters = new();
        private readonly System.Collections.Generic.Dictionary<string, int> narrativeValues = new();
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
            Puzzles.Reset();
            flags.Clear();
            counters.Clear();
            narrativeValues.Clear();
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
        public int GetNarrativeValue(string id) => id != null && narrativeValues.TryGetValue(id, out var value) ? value : 0;
        public void AddNarrativeValue(string id, int delta)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            narrativeValues[id] = GetNarrativeValue(id) + delta;
        }
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
        public void SaveCheckpoint() => SaveCheckpoint(SceneManager.GetActiveScene().name, CurrentPlayerPosition());
        public void SaveCheckpoint(string sceneName) => SaveCheckpoint(sceneName, CurrentPlayerPosition());
        private static Vector2 CurrentPlayerPosition()
        {
            var player = FindAnyObjectByType<MementoMori.Player.PlayerController>();
            return player == null ? Vector2.zero : player.SafeSavePosition;
        }
        public void SaveCheckpoint(string sceneName, Vector2 safePosition)
        {
            CheckpointScene = sceneName;
            CheckpointPosition = safePosition;
            WriteCheckpointPrefs(sceneName, safePosition);
            if (Application.isPlaying && SaveSlotStore.IsPlayableScene(sceneName))
            {
                try { new SaveSlotStore().Save(0, CapturePersistentState(sceneName, safePosition)); }
                catch (Exception exception) { Debug.LogWarning("Autosave failed: " + exception.Message, this); }
            }
        }

        private void WriteCheckpointPrefs(string sceneName, Vector2 safePosition)
        {
            var snapshot = new CheckpointSnapshot
            {
                scene = sceneName,
                solvedPuzzleIds = Puzzles.Snapshot(),
                position = safePosition,
                ritualCompleted = RitualCompleted,
                poeRevealed = PoeRevealed,
                mirrorPuzzleSolved = MirrorPuzzleSolved,
                sigilPuzzleSolved = SigilPuzzleSolved,
                fragmentCollected = FragmentCollected,
                flags = new List<StoryFlag>(flags).ConvertAll(flag => (int)flag).ToArray(),
                counterIds = new List<string>(counters.Keys).ToArray(),
                counterValues = new List<int>(counters.Values).ToArray(),
                narrativeIds = new List<string>(narrativeValues.Keys).ToArray(),
                narrativeValues = new List<int>(narrativeValues.Values).ToArray(),
                progressIds = new List<string>(puzzleProgress.Keys).ToArray(),
                progressValues = new List<int>(puzzleProgress.Values).ToArray(),
                checkpointIds = new List<string>(checkpointCounters.Keys).ToArray(),
                checkpointValues = new List<int>(checkpointCounters.Values).ToArray(),
                lastCheckpoint = (int)LastCheckpoint
            };
            PlayerPrefs.SetString("MementoMori.Checkpoint", JsonUtility.ToJson(snapshot));
            PlayerPrefs.Save();
        }

        public bool RestoreCheckpoint()
        {
            var json = PlayerPrefs.GetString("MementoMori.Checkpoint", string.Empty);
            if (string.IsNullOrEmpty(json)) return false;
            CheckpointSnapshot snapshot;
            try { snapshot = JsonUtility.FromJson<CheckpointSnapshot>(json); }
            catch (Exception) { return false; }
            if (snapshot == null || !SaveSlotStore.IsPlayableScene(snapshot.scene)) return false;
            if (!ValidPairs(snapshot.counterIds, snapshot.counterValues)
                || !ValidPairs(snapshot.progressIds, snapshot.progressValues)
                || !ValidSignedPairs(snapshot.narrativeIds, snapshot.narrativeValues)
                || !ValidPairs(snapshot.checkpointIds, snapshot.checkpointValues)
                || !ValidPosition(snapshot.position)) return false;
            if (snapshot.solvedPuzzleIds != null)
                foreach (var id in snapshot.solvedPuzzleIds) if (!Puzzles.IsKnown(id)) return false;
            if (snapshot.flags != null)
                foreach (var value in snapshot.flags) if (!Enum.IsDefined(typeof(StoryFlag), value)) return false;
            flags.Clear(); counters.Clear(); narrativeValues.Clear(); puzzleProgress.Clear(); checkpointCounters.Clear();
            Puzzles.Restore(snapshot.solvedPuzzleIds);
            if (snapshot.flags != null) foreach (var value in snapshot.flags) flags.Add((StoryFlag)value);
            if (snapshot.counterIds != null && snapshot.counterValues != null)
                for (var i = 0; i < Mathf.Min(snapshot.counterIds.Length, snapshot.counterValues.Length); i++) counters[snapshot.counterIds[i]] = snapshot.counterValues[i];
            if (snapshot.narrativeIds != null)
                for (var i = 0; i < snapshot.narrativeIds.Length; i++) narrativeValues[snapshot.narrativeIds[i]] = snapshot.narrativeValues[i];
            if (snapshot.progressIds != null)
                for (var i = 0; i < snapshot.progressIds.Length; i++) puzzleProgress[snapshot.progressIds[i]] = snapshot.progressValues[i];
            if (snapshot.checkpointIds != null)
                for (var i = 0; i < snapshot.checkpointIds.Length; i++) checkpointCounters[snapshot.checkpointIds[i]] = snapshot.checkpointValues[i];
            LastCheckpoint = Enum.IsDefined(typeof(CheckpointId), snapshot.lastCheckpoint)
                ? (CheckpointId)snapshot.lastCheckpoint : CheckpointId.Sleep;
            CheckpointScene = snapshot.scene; CheckpointPosition = snapshot.position;
            RitualCompleted = snapshot.ritualCompleted; PoeRevealed = snapshot.poeRevealed; MirrorPuzzleSolved = snapshot.mirrorPuzzleSolved;
            SigilPuzzleSolved = snapshot.sigilPuzzleSolved; FragmentCollected = snapshot.fragmentCollected;
            return true;
        }

        public void SetRitualCompleted() => RitualCompleted = true;
        public void SetPoeRevealed() { PoeRevealed = true; SetFlag(StoryFlag.PoeRevealed); }
        public void SetMirrorPuzzleSolved() => MirrorPuzzleSolved = true;
        public void SetSigilPuzzleSolved() => SigilPuzzleSolved = true;
        public void SetFragmentCollected() { FragmentCollected = true; SetFlag(StoryFlag.FragmentCollected); }

        [Serializable]
        public sealed class PersistentSnapshot
        {
            public int schemaVersion = 1;
            public string scene;
            public Vector2 safePosition;
            public string[] solvedPuzzleIds;
            public int[] flags;
            public string[] counterIds;
            public int[] counterValues;
            public string[] narrativeIds;
            public int[] narrativeValues;
            public string[] progressIds;
            public int[] progressValues;
            public string[] checkpointIds;
            public int[] checkpointValues;
            public int lastCheckpoint;
            public bool ritualCompleted, poeRevealed, mirrorPuzzleSolved, sigilPuzzleSolved, fragmentCollected;
        }

        public PersistentSnapshot CapturePersistentState(string scene, Vector2 safePosition)
        {
            var counterIds = new List<string>(counters.Keys);
            var progressIds = new List<string>(puzzleProgress.Keys);
            var checkpointIds = new List<string>(checkpointCounters.Keys);
            return new PersistentSnapshot
            {
                scene = scene,
                safePosition = safePosition,
                solvedPuzzleIds = Puzzles.Snapshot(),
                flags = new List<StoryFlag>(flags).ConvertAll(flag => (int)flag).ToArray(),
                counterIds = counterIds.ToArray(),
                counterValues = counterIds.ConvertAll(id => counters[id]).ToArray(),
                narrativeIds = new List<string>(narrativeValues.Keys).ToArray(),
                narrativeValues = new List<int>(narrativeValues.Values).ToArray(),
                progressIds = progressIds.ToArray(),
                progressValues = progressIds.ConvertAll(id => puzzleProgress[id]).ToArray(),
                checkpointIds = checkpointIds.ToArray(),
                checkpointValues = checkpointIds.ConvertAll(id => checkpointCounters[id]).ToArray(),
                lastCheckpoint = (int)LastCheckpoint,
                ritualCompleted = RitualCompleted,
                poeRevealed = PoeRevealed,
                mirrorPuzzleSolved = MirrorPuzzleSolved,
                sigilPuzzleSolved = SigilPuzzleSolved,
                fragmentCollected = FragmentCollected
            };
        }

        public bool RestorePersistentState(PersistentSnapshot snapshot)
        {
            if (snapshot == null || snapshot.schemaVersion != 1 || string.IsNullOrWhiteSpace(snapshot.scene)
                || snapshot.counterIds == null || snapshot.counterValues == null
                || snapshot.progressIds == null || snapshot.progressValues == null
                || snapshot.checkpointIds == null || snapshot.checkpointValues == null
                || snapshot.counterIds.Length != snapshot.counterValues.Length
                || !ValidSignedPairs(snapshot.narrativeIds, snapshot.narrativeValues)
                || snapshot.progressIds.Length != snapshot.progressValues.Length
                || snapshot.checkpointIds.Length != snapshot.checkpointValues.Length
                || !ValidPosition(snapshot.safePosition)
                || !Enum.IsDefined(typeof(CheckpointId), snapshot.lastCheckpoint)) return false;

            // Validate the whole payload before changing the live session.
            var counterNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in snapshot.counterIds)
                if (string.IsNullOrWhiteSpace(id) || !counterNames.Add(id)) return false;
            var progressNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in snapshot.progressIds)
                if (string.IsNullOrWhiteSpace(id) || !progressNames.Add(id)) return false;
            var checkpointNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in snapshot.checkpointIds)
                if (!Enum.TryParse(id, out CheckpointId parsed) || !Enum.IsDefined(typeof(CheckpointId), parsed)
                    || !checkpointNames.Add(id)) return false;
            foreach (var value in snapshot.counterValues) if (value < 0) return false;
            foreach (var value in snapshot.progressValues) if (value < 0) return false;
            foreach (var value in snapshot.checkpointValues) if (value < 0) return false;
            if (snapshot.solvedPuzzleIds != null)
                foreach (var id in snapshot.solvedPuzzleIds) if (!Puzzles.IsKnown(id)) return false;
            if (snapshot.flags != null)
                foreach (var value in snapshot.flags) if (!Enum.IsDefined(typeof(StoryFlag), value)) return false;

            Puzzles.Restore(snapshot.solvedPuzzleIds);
            flags.Clear(); counters.Clear(); narrativeValues.Clear(); puzzleProgress.Clear(); checkpointCounters.Clear();
            if (snapshot.flags != null) foreach (var value in snapshot.flags) flags.Add((StoryFlag)value);
            for (var i = 0; i < snapshot.counterIds.Length; i++) counters.Add(snapshot.counterIds[i], snapshot.counterValues[i]);
            if (snapshot.narrativeIds != null)
                for (var i = 0; i < snapshot.narrativeIds.Length; i++) narrativeValues.Add(snapshot.narrativeIds[i], snapshot.narrativeValues[i]);
            for (var i = 0; i < snapshot.progressIds.Length; i++) puzzleProgress.Add(snapshot.progressIds[i], snapshot.progressValues[i]);
            for (var i = 0; i < snapshot.checkpointIds.Length; i++) checkpointCounters.Add(snapshot.checkpointIds[i], snapshot.checkpointValues[i]);
            LastCheckpoint = (CheckpointId)snapshot.lastCheckpoint;
            RitualCompleted = snapshot.ritualCompleted;
            PoeRevealed = snapshot.poeRevealed;
            MirrorPuzzleSolved = snapshot.mirrorPuzzleSolved;
            SigilPuzzleSolved = snapshot.sigilPuzzleSolved;
            FragmentCollected = snapshot.fragmentCollected;
            CheckpointScene = snapshot.scene;
            CheckpointPosition = snapshot.safePosition;
            WriteCheckpointPrefs(snapshot.scene, snapshot.safePosition);
            return true;
        }

        private static bool ValidPairs(string[] ids, int[] values)
        {
            if (ids == null && values == null) return true;
            if (ids == null || values == null || ids.Length != values.Length) return false;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < ids.Length; i++)
                if (string.IsNullOrWhiteSpace(ids[i]) || values[i] < 0 || !seen.Add(ids[i])) return false;
            return true;
        }

        private static bool ValidSignedPairs(string[] ids, int[] values)
        {
            if (ids == null && values == null) return true;
            if (ids == null || values == null || ids.Length != values.Length) return false;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in ids) if (string.IsNullOrWhiteSpace(id) || !seen.Add(id)) return false;
            return true;
        }

        private static bool ValidPosition(Vector2 value) => !float.IsNaN(value.x) && !float.IsInfinity(value.x)
            && !float.IsNaN(value.y) && !float.IsInfinity(value.y);

        [Serializable]
        private sealed class CheckpointSnapshot
        {
            public string[] solvedPuzzleIds;
            public string scene;
            public Vector2 position;
            public bool ritualCompleted, poeRevealed, mirrorPuzzleSolved, sigilPuzzleSolved, fragmentCollected;
            public int[] flags;
            public string[] counterIds;
            public int[] counterValues;
            public string[] narrativeIds;
            public int[] narrativeValues;
            public string[] progressIds;
            public int[] progressValues;
            public string[] checkpointIds;
            public int[] checkpointValues;
            public int lastCheckpoint;
        }
    }
}




