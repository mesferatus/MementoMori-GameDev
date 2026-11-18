using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.Core
{
    /// <summary>Coordinates session-level actions without owning scene gameplay.</summary>
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        private SaveSlotStore saves;
        private GameState.PersistentSnapshot pendingLoad;
        public string LastSaveError { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            saves = new SaveSlotStore();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this) Instance = null;
        }

        public void StartNewGame()
        {
            pendingLoad = null;
            GameState.Instance?.StartNewGame();
            SceneLoader.Instance?.LoadScene("Quarto");
        }

        public bool SaveGame(int slot)
        {
            LastSaveError = null;
            var state = GameState.Instance;
            var scene = SceneManager.GetActiveScene().name;
            if (state == null || !SaveSlotStore.IsPlayableScene(scene) || SceneLoader.Instance == null
                || SceneLoader.Instance.IsLoading)
            { LastSaveError = "Não há partida jogável para salvar."; return false; }
            var player = FindAnyObjectByType<MementoMori.Player.PlayerController>();
            var position = player == null ? state.CheckpointPosition : player.SafeSavePosition;
            try { saves.Save(slot, state.CapturePersistentState(scene, position)); return true; }
            catch (System.Exception exception)
            { LastSaveError = "Falha ao salvar: " + exception.Message; return false; }
        }

        public bool LoadGame(int slot)
        {
            LastSaveError = null;
            if (!saves.TryLoad(slot, out var snapshot, out var error))
            { LastSaveError = error; return false; }
            if (!Application.CanStreamedLevelBeLoaded(snapshot.scene) || GameState.Instance == null
                || SceneLoader.Instance == null || SceneLoader.Instance.IsLoading)
            { LastSaveError = "A cena do salvamento não está disponível."; return false; }
            if (!GameState.Instance.RestorePersistentState(snapshot))
            { LastSaveError = "O salvamento contém dados inválidos."; return false; }
            pendingLoad = snapshot;
            Time.timeScale = 1f;
            InputGate.Instance?.ClearAll();
            SceneLoader.Instance.LoadScene(snapshot.scene);
            return true;
        }

        public bool RestartCheckpoint()
        {
            var state = GameState.Instance;
            if (state == null || SceneLoader.Instance == null || SceneLoader.Instance.IsLoading
                || !state.RestoreCheckpoint() || !SaveSlotStore.IsPlayableScene(state.CheckpointScene)
                || !Application.CanStreamedLevelBeLoaded(state.CheckpointScene)) return false;
            // Reuse the load placement path after the new scene has initialized.
            pendingLoad = state.CapturePersistentState(state.CheckpointScene, state.CheckpointPosition);
            Time.timeScale = 1f;
            InputGate.Instance?.ClearAll();
            SceneLoader.Instance.LoadScene(state.CheckpointScene);
            return true;
        }

        public bool ContinueGame()
        {
            var foundAny = false;
            foreach (var slot in saves.ExistingSlotsNewestFirst())
            {
                foundAny = true;
                if (LoadGame(slot)) return true;
            }
            LastSaveError = foundAny ? "Nenhum salvamento válido pôde ser carregado."
                : "Nenhuma partida salva foi encontrada.";
            return false;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (pendingLoad != null && scene.name == pendingLoad.scene)
            {
                StartCoroutine(PlaceLoadedPlayer(pendingLoad));
                pendingLoad = null;
            }
            else if (scene.name == "Labirinto" || scene.name == "DominioLua")
                StartCoroutine(SaveRegionEntryCheckpoint(scene));
        }

        private static IEnumerator SaveRegionEntryCheckpoint(Scene scene)
        {
            // Region geometry places the actor during Start; save the destination, not the old room's coordinates.
            for (var frame = 0; frame < 3; frame++) yield return null;
            if (SceneManager.GetActiveScene() != scene || GameState.Instance == null) yield break;
            var player = FindAnyObjectByType<MementoMori.Player.PlayerController>();
            if (player == null) yield break;
            GameState.Instance.SaveStoryCheckpoint(scene.name == "Labirinto"
                ? CheckpointId.LabyrinthStart : CheckpointId.MoonDomainEntry);
        }

        private static IEnumerator PlaceLoadedPlayer(GameState.PersistentSnapshot snapshot)
        {
            yield return null;
            var player = FindAnyObjectByType<MementoMori.Player.PlayerController>();
            if (player == null) yield break;
            player.transform.position = snapshot.safePosition;
            var body = player.GetComponent<Rigidbody2D>();
            if (body != null) { body.position = snapshot.safePosition; body.linearVelocity = Vector2.zero; }
            FindAnyObjectByType<MementoMori.Poe.PoeFollower>()?.RestoreBesidePlayer();
        }

        public void ReturnToMenu()
        {
            pendingLoad = null;
            GameState.Instance?.ResetSession();
            SceneLoader.Instance?.LoadScene("MainMenu");
        }
    }
}
