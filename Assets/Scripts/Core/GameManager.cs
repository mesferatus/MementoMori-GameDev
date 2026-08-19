using UnityEngine;

namespace MementoMori.Core
{
    /// <summary>Coordinates session-level actions without owning scene gameplay.</summary>
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

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

        public void StartNewGame()
        {
            GameState.Instance?.StartNewGame();
            SceneLoader.Instance?.LoadScene("Quarto");
        }

        public void ReturnToMenu()
        {
            GameState.Instance?.ResetSession();
            SceneLoader.Instance?.LoadScene("MainMenu");
        }
    }
}
