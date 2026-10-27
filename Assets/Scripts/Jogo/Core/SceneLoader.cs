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

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("SceneLoader received an empty scene name.", this);
                return;
            }

            if (!isLoading)
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

            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return FadeTo(0f);
            InputGate.Instance?.Release(TransitionGate);
            isLoading = false;
        }

        private IEnumerator FadeTo(float targetAlpha)
        {
            if (fadeCanvas == null)
                CreateFadeCanvas();
            float initialAlpha = fadeCanvas.alpha;
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

            fadeCanvas = root.GetComponent<CanvasGroup>();
            fadeCanvas.alpha = 0f;
            fadeCanvas.blocksRaycasts = false;
        }
    }
}
