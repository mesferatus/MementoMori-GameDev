using MementoMori.Core;
using UnityEngine;

namespace MementoMori.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject creditsPanel;
        public void ConfigureCredits(GameObject panel) => creditsPanel = panel;
        public void Play() => GameManager.Instance?.StartNewGame();
        public void ShowCredits() { if (creditsPanel != null) creditsPanel.SetActive(true); }
        public void HideCredits() { if (creditsPanel != null) creditsPanel.SetActive(false); }
        public void Quit()
        {
#if UNITY_EDITOR
            Debug.Log("Quit requested; ignored in the Unity Editor.");
#else
            Application.Quit();
#endif
        }
    }
}
