using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MementoMori.Core
{
    /// <summary>Loads scenes once at a time behind a reusable black screen fade.</summary>
    public sealed class SceneLoader : MonoBehaviour
    {
        private const string TransitionGate = "SceneTransition";

        public static SceneLoader Instance { get; private set; }

        [SerializeField, Min(0.05f)] private float fadeDuration = 0.35f;

        private CanvasGroup fadeCanvas;
        private bool isLoading;
        public bool IsLoading => isLoading;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFadeCanvas();
        }

        private AsyncOperation preload;
        private string preloadScene;
        public bool IsPreloading(string sceneName) => preload != null && preloadScene == sceneName;

        /// <summary>
        /// Starts reading a scene in the background while the current one keeps running (dream veil, final
        /// approach to a portal). The scene only activates when <see cref="LoadScene"/> asks for it, so the
        /// curtain then covers activation alone instead of the whole read of a 100 MB map.
        /// </summary>
        public void Preload(string sceneName)
        {
            // A pending preload blocks every other async scene load. Inside the Editor, tests and audit tools
            // load scenes directly, so preloading is a player-build feature only.
            if (Application.isEditor) return;
            if (isLoading || preload != null || string.IsNullOrWhiteSpace(sceneName)
                || sceneName == SceneManager.GetActiveScene().name || !Application.CanStreamedLevelBeLoaded(sceneName)) return;
            preloadScene = sceneName;
            preload = SceneManager.LoadSceneAsync(sceneName);
            preload.allowSceneActivation = false;
        }

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("SceneLoader received an empty scene name.", this);
                return;
            }

            if (isLoading) return;
            if (revealRoutine != null) { StopCoroutine(revealRoutine); revealRoutine = null; }
            StartCoroutine(LoadRoutine(sceneName));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            isLoading = true;
            InputGate.Instance?.Block(TransitionGate);
            yield return FadeTo(1f);
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"Scene '{sceneName}' is not enabled in Build Settings.", this);
                yield return FadeTo(0f);
                InputGate.Instance?.Release(TransitionGate);
                isLoading = false;
                yield break;
            }

            // Release builds default to BelowNormal, which caps scene integration at a few ms per frame
            // and stretches the large map scenes into a long, frozen-looking wait.
            var previousPriority = Application.backgroundLoadingPriority;
            Application.backgroundLoadingPriority = ThreadPriority.High;
            SetLoadingVisible(true);
            if (preload != null && preloadScene != sceneName)
            {
                // A pending preload blocks every later async load; let it finish before the requested scene.
                preload.allowSceneActivation = true;
                yield return preload;
                preload = null;
            }
            var wasPreloaded = preload != null;
            var started = Time.realtimeSinceStartup;
            var operation = preload ?? SceneManager.LoadSceneAsync(sceneName);
            preload = null; preloadScene = null;
            operation.allowSceneActivation = true;
            yield return operation;
            Application.backgroundLoadingPriority = previousPriority;
            Debug.Log($"[SceneLoader] {sceneName} ready in {Time.realtimeSinceStartup - started:0.00}s (preloaded={wasPreloaded})");
            // The new scene is live: hand control back at once; the reveal below is purely visual.
            InputGate.Instance?.Release(TransitionGate);
            isLoading = false;
            revealRoutine = StartCoroutine(Reveal());
        }

        private Coroutine revealRoutine;

        private IEnumerator Reveal()
        {
            // Let the new scene run its Start/first frames behind the curtain, so the reveal is smooth.
            for (var frame = 0; frame < 3; frame++) yield return null;
            SetLoadingVisible(false);
            yield return FadeTo(0f);
            revealRoutine = null;
        }

        /// <summary>Puts the curtain up at once, for a sequence that already ends on black.</summary>
        public void CoverImmediately()
        {
            if (fadeCanvas == null) CreateFadeCanvas();
            fadeCanvas.alpha = 1f;
            fadeCanvas.blocksRaycasts = true;
        }

        private IEnumerator FadeTo(float targetAlpha)
        {
            if (fadeCanvas == null)
                CreateFadeCanvas();
            float initialAlpha = fadeCanvas.alpha;
            if (Mathf.Approximately(initialAlpha, targetAlpha))
            {
                fadeCanvas.blocksRaycasts = targetAlpha > 0f;
                yield break;
            }
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvas.alpha = Mathf.Lerp(initialAlpha, targetAlpha, elapsed / fadeDuration);
                yield return null;
            }

            fadeCanvas.alpha = targetAlpha;
            fadeCanvas.blocksRaycasts = targetAlpha > 0f;
        }

        private TMPro.TextMeshProUGUI loadingLabel;
        private bool loadingVisible;

        private void SetLoadingVisible(bool visible)
        {
            loadingVisible = visible;
            if (loadingLabel == null) return;
            loadingLabel.text = Loc.T("Carregando");
            loadingLabel.gameObject.SetActive(visible);
        }

        private void Update()
        {
            if (!loadingVisible || loadingLabel == null) return;
            // Slow candle-like breathing plus three dots, so a long load never reads as a freeze.
            var t = Time.unscaledTime;
            loadingLabel.alpha = .45f + .35f * Mathf.Sin(t * 2.2f) * .5f + .175f;
            loadingLabel.text = Loc.T("Carregando") + new string('.', 1 + (int)(t * 1.6f) % 3);
        }

        private void CreateFadeCanvas()
        {
            var root = new GameObject("FadeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            var imageObject = new GameObject("Black", typeof(Image));
            imageObject.transform.SetParent(root.transform, false);
            var image = imageObject.GetComponent<Image>();
            image.color = new Color(0.090f, 0.078f, 0.141f, 1f);
            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var labelObject = new GameObject("Loading", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            labelObject.transform.SetParent(root.transform, false);
            loadingLabel = labelObject.GetComponent<TMPro.TextMeshProUGUI>();
            loadingLabel.alignment = TMPro.TextAlignmentOptions.BottomRight;
            loadingLabel.fontSize = 30;
            loadingLabel.color = new Color(.79f, .72f, .84f, 1f);
            loadingLabel.raycastTarget = false;
            var labelRect = loadingLabel.rectTransform;
            labelRect.anchorMin = new Vector2(.6f, .04f);
            labelRect.anchorMax = new Vector2(.96f, .12f);
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            labelObject.SetActive(false);
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;

            fadeCanvas = root.GetComponent<CanvasGroup>();
            fadeCanvas.alpha = 0f;
            fadeCanvas.blocksRaycasts = false;
        }
    }
}
