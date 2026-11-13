using MementoMori.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MementoMori.UI
{
    /// <summary>
    /// Small "progress saved" mark in the lower-right corner after each autosave, so the player knows it is
    /// safe to quit. Purely visual: it never takes input and stays hidden while dialogue holds the input gate.
    /// </summary>
    public sealed class AutosaveIndicator : MonoBehaviour
    {
        const float Visible = 2.2f, Fade = .8f, Cooldown = 12f;
        CanvasGroup group;
        TMP_Text label;
        RectTransform sigil;
        float shownAt = -100f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            SceneManager.sceneLoaded -= Attach;
            SceneManager.sceneLoaded += Attach;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AttachOpenScene() => Attach(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        static void Attach(Scene scene, LoadSceneMode mode)
        {
            if (!SaveSlotStore.IsPlayableScene(scene.name)) return;
            if (FindFirstObjectByType<AutosaveIndicator>() != null) return;
            var host = new GameObject("AutosaveIndicator");
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<AutosaveIndicator>();
        }

        void OnEnable() => GameState.Autosaved += OnAutosaved;
        void OnDisable() => GameState.Autosaved -= OnAutosaved;

        void OnAutosaved()
        {
            // Checkpoints can fire in quick bursts (module entry + puzzle state); one mark is enough.
            if (Time.unscaledTime - shownAt < Cooldown) return;
            shownAt = Time.unscaledTime;
        }

        void Start()
        {
            var root = new GameObject("AutosaveCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 91;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            group = root.GetComponent<CanvasGroup>(); group.alpha = 0; group.interactable = group.blocksRaycasts = false;

            var mark = new GameObject("Sigil", typeof(RectTransform), typeof(Image));
            mark.transform.SetParent(root.transform, false);
            var diamond = mark.GetComponent<Image>();
            diamond.color = new Color(.79f, .72f, .84f); diamond.raycastTarget = false;
            sigil = diamond.rectTransform;
            sigil.anchorMin = sigil.anchorMax = new Vector2(1f, 0f); sigil.pivot = new Vector2(.5f, .5f);
            sigil.sizeDelta = new Vector2(12, 12); sigil.anchoredPosition = new Vector2(-62, 52);

            var text = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            text.transform.SetParent(root.transform, false);
            label = text.GetComponent<TMP_Text>();
            var book = Resources.Load<GameObject>("GrimoireRuntimeHost")?.GetComponent<MementoMori.World.GrimoireScreen>();
            label.font = book != null && book.HeadingFont != null ? book.HeadingFont : TMP_Settings.defaultFontAsset;
            label.fontSize = 20; label.alignment = TextAlignmentOptions.MidlineRight;
            label.color = new Color(.79f, .72f, .84f, .9f); label.raycastTarget = false;
            var rect = label.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f); rect.pivot = new Vector2(1f, .5f);
            rect.sizeDelta = new Vector2(360, 40); rect.anchoredPosition = new Vector2(-84, 52);
        }

        void Update()
        {
            if (group == null) return;
            var age = Time.unscaledTime - shownAt;
            var alpha = age < .35f ? age / .35f : age < Visible ? 1f : Mathf.Clamp01(1f - (age - Visible) / Fade);
            var blocked = InputGate.Instance != null && InputGate.Instance.IsBlocked;
            group.alpha = blocked ? 0f : alpha * .85f;
            if (group.alpha <= 0f) return;
            label.text = Loc.T("Progresso salvo");
            var calm = AccessibilitySettings.Instance != null && AccessibilitySettings.Instance.ReduceFlashes;
            sigil.localEulerAngles = new Vector3(0, 0, 45f + (calm ? 0f : age * 90f));
        }
    }
}
