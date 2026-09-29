using System.Collections;
using MementoMori.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MementoMori.UI
{
    /// <summary>Keeps the current non-interactive objective visible during exploration.</summary>
    public sealed class ObjectiveToastController : MonoBehaviour
    {
        public static ObjectiveToastController Instance { get; private set; }

        [SerializeField, Min(.5f)] private float visibleDuration = 3f;
        [SerializeField] private bool persistentObjective = true;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text objectiveText;
        [SerializeField] private TMP_Text objectiveTextTmp;

        private Coroutine hideRoutine;
        private string currentObjective = string.Empty;
        private string lastShownObjective = string.Empty;
        private int showCount;

        public bool IsVisible => canvasGroup != null && canvasGroup.alpha > 0f;
        public string CurrentObjective => currentObjective;
        public int ShowCount => showCount;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (canvasGroup != null && (objectiveText != null || objectiveTextTmp != null))
                {
                    Instance.Hide();
                    Instance.canvasGroup = canvasGroup;
                    Instance.objectiveText = objectiveText;
                    Instance.objectiveTextTmp = objectiveTextTmp;
                    Instance.lastShownObjective = string.Empty;
                    Instance.currentObjective = string.Empty;
                }
                Destroy(this);
                return;
            }

            Instance = this;
            if (Application.isPlaying && transform.parent == null)
                DontDestroyOnLoad(gameObject);
            EnsureFallbackUi();
            Hide();
        }

        private void Start()
        {
            if (GameState.Instance != null)
                GameState.Instance.OnFlagChanged += OnFlagChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EvaluateForScene(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            if (GameState.Instance != null)
                GameState.Instance.OnFlagChanged -= OnFlagChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this) Instance = null;
        }

        public void EvaluateForScene(string sceneName)
        {
            var objective = ObjectiveFor(sceneName, GameState.Instance);
            if (string.IsNullOrEmpty(objective))
            {
                currentObjective = string.Empty;
                Hide();
                return;
            }

            if (objective == currentObjective && IsVisible)
                return;

            currentObjective = objective;
            ShowObjective(objective);
        }

        public void ShowObjective(string objective)
        {
            if (string.IsNullOrWhiteSpace(objective)) return;
            if (objective == lastShownObjective && IsVisible) return;
            EnsureFallbackUi();

            currentObjective = objective;
            lastShownObjective = objective;
            showCount++;
            SetObjective(objective);
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            if (hideRoutine != null) StopCoroutine(hideRoutine);
            if (Application.isPlaying && !persistentObjective)
                hideRoutine = StartCoroutine(HideAfterDelay());
        }

        public void Hide()
        {
            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
                hideRoutine = null;
            }
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        public void SetObjective(string text)
        {
            currentObjective = text ?? string.Empty;
            if (objectiveText != null) objectiveText.text = currentObjective;
            if (objectiveTextTmp != null) objectiveTextTmp.text = currentObjective;
        }

        public static string ObjectiveFor(string sceneName, GameState state)
        {
            if (state == null) return null;
            switch (sceneName)
            {
                case "Quarto":
                    if (state.RitualCompleted) return "Descanse.";
                    var sawAnyClue = state.HasFlag(StoryFlag.RoomWindowSecured)
                        || state.HasFlag(StoryFlag.RoomPhotoExamined)
                        || state.HasFlag(StoryFlag.RoomBowlExamined)
                        || state.HasFlag(StoryFlag.RoomToyExamined);
                    if (!sawAnyClue) return "Examine o quarto.";
                    if (!state.HasFlag(StoryFlag.RoomWindowSecured) || !state.HasFlag(StoryFlag.RoomPhotoExamined))
                        return "Reúna pistas sobre o ritual.";
                    if (!state.HasFlag(StoryFlag.RoomGrimoireRead)) return "Consulte o grimório.";
                    return "Recomponha o ritual.";
                case "Labirinto":
                    return state.HasFlag(StoryFlag.PoeRevealed) ? "Encontre uma saída / portal." : "Siga Poe.";
                case "DominioLua":
                    if (state.HasFlag(StoryFlag.FragmentCollected)) return null;
                    if (state.HasFlag(StoryFlag.SigilPuzzleComplete)) return "Colete o fragmento.";
                    if (state.HasFlag(StoryFlag.MirrorPuzzleComplete)) return "Complete o sigilo.";
                    if (state.HasFlag(StoryFlag.GardenComplete)) return "Resolva os desafios.";
                    return "Explore o domínio.";
                default:
                    return null;
            }
        }

        private static bool HasRoomExploration(GameState state)
        {
            return state.HasFlag(StoryFlag.RoomPhotoExamined)
                && state.HasFlag(StoryFlag.RoomGrimoireRead)
                && state.HasFlag(StoryFlag.RoomWindowSecured)
                && state.HasFlag(StoryFlag.RoomRitualItemStored)
                && state.HasFlag(StoryFlag.RoomCandlesDone);
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSecondsRealtime(visibleDuration);
            Hide();
        }

        private void OnFlagChanged(StoryFlag flag, bool value)
        {
            EvaluateForScene(SceneManager.GetActiveScene().name);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureFallbackUi();
            EvaluateForScene(scene.name);
        }

        private void EnsureFallbackUi()
        {
            if (canvasGroup != null && (objectiveText != null || objectiveTextTmp != null)) return;

            var canvasObject = new GameObject("ObjectiveToast", typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            canvasGroup = canvasObject.GetComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(.25f, .82f);
            panelRect.anchorMax = new Vector2(.75f, .93f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(.05f, .04f, .1f, .92f);

            var textObject = new GameObject("ObjectiveText", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(panel.transform, false);
            objectiveText = textObject.GetComponent<Text>();
            objectiveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            objectiveText.fontSize = 18;
            objectiveText.color = Color.white;
            objectiveText.alignment = TextAnchor.MiddleCenter;
            objectiveText.horizontalOverflow = HorizontalWrapMode.Wrap;
            objectiveText.verticalOverflow = VerticalWrapMode.Overflow;
            var textRect = objectiveText.rectTransform;
            textRect.anchorMin = new Vector2(.04f, .08f);
            textRect.anchorMax = new Vector2(.96f, .92f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }
}
