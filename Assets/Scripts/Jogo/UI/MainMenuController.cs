using MementoMori.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MementoMori.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject creditsPanel;
        private SettingsPanelView settingsView;
        private GameObject creditsReturn;
        private void Start()
        {
            // Reuse the approved menu button artwork and layout for Continue.
            var play = GameObject.Find("Button_Jogar")?.GetComponent<Button>();
            if (play == null || play.transform.parent == null || play.transform.parent.name != "MainMenuButtons") return;
            var button = Instantiate(play, play.transform.parent);
            button.name = "Button_Continuar";
            button.transform.SetSiblingIndex(play.transform.GetSiblingIndex() + 1);
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(Continue);
            var label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = "CONTINUAR";
            var saves = new SaveSlotStore();
            button.interactable = saves.Exists(0) || saves.Exists(1) || saves.Exists(2) || saves.Exists(3);
            settingsView = gameObject.AddComponent<SettingsPanelView>();
            settingsView.Configure(play.transform.parent.gameObject, play.transform.parent.gameObject,
                null, play, label);
            var shortcut = play.transform.parent.Find("Button_Configuracoes") as RectTransform;
            if (shortcut != null)
            {
                // The existing settings cog belongs to the canvas, outside the menu's vertical layout.
                shortcut.SetParent(play.transform.parent.parent, false);
                shortcut.anchorMin = shortcut.anchorMax = Vector2.one;
                shortcut.anchoredPosition = new Vector2(-78f, -74f);
            }
            ApplyCamilaComposition(play.transform.parent as RectTransform, shortcut);
        }

        /// <summary>
        /// Approved menu layout (2026-11): Camila's original illustration centred as the background, the title
        /// large at the top, JOGAR prominent under the illustration with CONTINUAR and SAIR below it, CRÉDITOS
        /// small at the bottom-left, the settings cog at the bottom-right and a thin lilac frame. Everything is a
        /// separate overlay on the existing menu objects: the art file is untouched and the callbacks are kept.
        /// </summary>
        private static void ApplyCamilaComposition(RectTransform buttons, RectTransform settingsCog)
        {
            var art = Resources.Load<Sprite>("MainMenu/CamilaIlustracao");
            var canvas = buttons != null ? buttons.parent as RectTransform : null;
            if (art == null || canvas == null) return;
            var width = canvas.rect.width > 0f ? canvas.rect.width : 1920f;
            var height = canvas.rect.height > 0f ? canvas.rect.height : 1080f;

            var placeholder = canvas.Find("MainMenuBackgroundPlaceholder");
            var insertAt = placeholder != null ? placeholder.GetSiblingIndex() + 1 : 0;

            // Centred, covering the screen with the aspect preserved.
            var artRect = NewImage("CamilaIlustracao", canvas, insertAt, art, Color.white);
            artRect.anchorMin = artRect.anchorMax = artRect.pivot = new Vector2(.5f, .5f);
            var cover = Mathf.Max(width / art.rect.width, height / art.rect.height);
            artRect.sizeDelta = new Vector2(art.rect.width * cover, art.rect.height * cover);
            artRect.anchoredPosition = Vector2.zero;

            // Reversible chromatic treatment (Codex proposal #713580 at low strength) as its own layer.
            var tint = NewImage("CamilaTratamentoCromatico", canvas, insertAt + 1, null, new Color(.2f, .07f, .26f, .22f));
            Stretch(tint);

            // Soft shade behind the title and behind the buttons, so both read over the illustration.
            var dark = new Color(.02f, .006f, .035f, 1f);
            var top = NewImage("SombraTitulo", canvas, insertAt + 2, MakeVerticalFade(false), dark);
            top.anchorMin = new Vector2(0f, .66f); top.anchorMax = Vector2.one; top.offsetMin = top.offsetMax = Vector2.zero;
            var bottom = NewImage("SombraBotoes", canvas, insertAt + 3, MakeVerticalFade(true), dark);
            bottom.anchorMin = Vector2.zero; bottom.anchorMax = new Vector2(1f, .42f); bottom.offsetMin = bottom.offsetMax = Vector2.zero;

            AddThinFrame(canvas, insertAt + 4);

            foreach (var name in new[] { "R4Corner0", "R4Corner1", "R4Corner2", "R4Corner3", "CenterSigil" })
            {
                // The illustration brings its own ornate corners; the centred sigil would sit behind the face.
                var decoration = canvas.Find(name);
                if (decoration != null) decoration.gameObject.SetActive(false);
            }

            // Small crescent mark at the bottom centre of the frame.
            Move(canvas.Find("MoonEmblem"), new Vector2(0f, -height * .5f + 30f), .32f);
            // The title, large and centred at the top.
            Move(canvas.Find("Logo_MementoMoriBeta"), new Vector2(0f, height * .5f - 165f), 1.12f);

            // Buttons: JOGAR prominent, CONTINUAR and SAIR smaller below it, centred.
            var layout = buttons.GetComponent<VerticalLayoutGroup>();
            if (layout != null) layout.enabled = false;
            buttons.localScale = Vector3.one;
            buttons.anchoredPosition = new Vector2(0f, -height * .5f + 250f);
            var frame = Resources.Load<Sprite>("MainMenu/BotaoMenu");
            Place(buttons.Find("Button_Jogar"), new Vector2(0f, 40f), 1f, frame, new Vector2(600f, 112f));
            Place(buttons.Find("Button_Continuar"), new Vector2(0f, -66f), .76f, frame, new Vector2(600f, 100f));
            Place(buttons.Find("Button_Sair"), new Vector2(0f, -142f), .76f, frame, new Vector2(600f, 100f));
            Style(buttons.Find("Button_Creditos"), frame, new Vector2(600f, 100f));

            // CRÉDITOS: small, bottom-left. The cog: bottom-right. Both stay below the credits/settings panels.
            var credits = canvas.Find("Credits");
            if (buttons.Find("Button_Creditos") is RectTransform creditsButton)
            {
                creditsButton.SetParent(canvas, false);
                creditsButton.SetSiblingIndex(credits != null ? credits.GetSiblingIndex() : canvas.childCount - 1);
                creditsButton.anchorMin = creditsButton.anchorMax = Vector2.zero;
                creditsButton.pivot = new Vector2(.5f, .5f);
                creditsButton.anchoredPosition = new Vector2(240f, 110f);
                creditsButton.localScale = Vector3.one * .5f;
            }
            if (settingsCog != null)
            {
                settingsCog.gameObject.SetActive(true);
                settingsCog.SetSiblingIndex(credits != null ? credits.GetSiblingIndex() : canvas.childCount - 1);
                settingsCog.anchorMin = settingsCog.anchorMax = new Vector2(1f, 0f);
                settingsCog.anchoredPosition = new Vector2(-190f, 140f);
                settingsCog.sizeDelta = new Vector2(104f, 104f);
                var cogImage = settingsCog.GetComponent<Image>();
                if (cogImage != null) cogImage.color = new Color(.92f, .82f, 1f, 1f);
            }
        }

        private static void Place(Transform target, Vector2 position, float scale, Sprite frame, Vector2 size)
        {
            if (target is not RectTransform rect) return;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.localScale = Vector3.one * scale;
            Style(rect, frame, size);
        }

        /// <summary>Reference button frame (9-slice) with a colour-tint hover, replacing the old bar art.</summary>
        private static void Style(Transform target, Sprite frame, Vector2 size)
        {
            if (target is not RectTransform rect || frame == null) return;
            rect.sizeDelta = size;
            var image = rect.GetComponent<Image>();
            if (image != null) { image.sprite = frame; image.type = Image.Type.Sliced; image.color = Color.white; }
            var button = rect.GetComponent<Button>();
            if (button != null)
            {
                button.transition = Selectable.Transition.ColorTint;
                var colors = button.colors;
                colors.normalColor = new Color(.86f, .82f, .9f, 1f);
                colors.highlightedColor = Color.white;
                colors.selectedColor = Color.white;
                colors.pressedColor = new Color(.7f, .62f, .78f, 1f);
                colors.disabledColor = new Color(.5f, .46f, .55f, .55f);
                colors.colorMultiplier = 1.15f;
                colors.fadeDuration = .12f;
                button.colors = colors;
            }
            foreach (var name in new[] { "SelectorLeft", "SelectorRight", "GlowOverlay" })
            {
                // The new frame carries its own stars; the old side diamonds would double them.
                var extra = rect.Find(name);
                if (extra != null) extra.gameObject.SetActive(false);
            }
            var label = rect.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.rectTransform.anchorMin = new Vector2(.2f, .14f);
                label.rectTransform.anchorMax = new Vector2(.8f, .86f);
                label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
                label.color = new Color(.95f, .9f, 1f, 1f);
            }
        }

        // Thin lilac frame a little inside the screen edge, drawn as four 2px lines.
        private static void AddThinFrame(RectTransform canvas, int index)
        {
            const float inset = 26f, thickness = 2f;
            var color = new Color(.72f, .55f, .86f, .8f);
            var root = new GameObject("MolduraFina", typeof(RectTransform));
            var rootRect = (RectTransform)root.transform;
            rootRect.SetParent(canvas, false);
            rootRect.SetSiblingIndex(Mathf.Min(index, canvas.childCount - 1));
            Stretch(rootRect);
            rootRect.offsetMin = Vector2.one * inset; rootRect.offsetMax = -Vector2.one * inset;
            Line(rootRect, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -thickness), Vector2.zero, color);
            Line(rootRect, Vector2.zero, new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, thickness), color);
            Line(rootRect, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(thickness, 0f), color);
            Line(rootRect, new Vector2(1f, 0f), Vector2.one, new Vector2(-thickness, 0f), Vector2.zero, color);
        }

        private static void Line(RectTransform parent, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject("Linha", typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            var image = go.GetComponent<Image>(); image.color = color; image.raycastTarget = false;
        }

        private static Sprite MakeVerticalFade(bool fromBottom)
        {
            const int size = 128;
            var texture = new Texture2D(1, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (var y = 0; y < size; y++)
            {
                var t = y / (size - 1f);
                var edge = fromBottom ? 1f - t : t; // 1 at the screen edge, 0 towards the centre
                texture.SetPixel(0, y, new Color(1f, 1f, 1f, .88f * edge * edge));
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, size), new Vector2(.5f, .5f));
        }

        private static RectTransform NewImage(string name, RectTransform parent, int index, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.SetSiblingIndex(Mathf.Min(index, parent.childCount - 1));
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void Move(Transform target, Vector2 position, float scale)
        {
            if (target is not RectTransform rect) return;
            rect.anchoredPosition = position;
            rect.localScale = Vector3.one * scale;
        }

        private void Update()
        {
            if (creditsPanel != null && creditsPanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape))
            {
                HideCredits();
                return;
            }
            if (settingsView != null && settingsView.IsOpen && !settingsView.IsCapturing
                && settingsView.CaptureConsumedFrame != Time.frameCount && Input.GetKeyDown(KeyCode.Escape))
                settingsView.Close();
        }
        public void ConfigureCredits(GameObject panel) => creditsPanel = panel;
        public void Play() => GameManager.Instance?.StartNewGame();
        public void Continue()
        {
            var manager = GameManager.Instance;
            if (manager != null && !manager.ContinueGame())
                Debug.LogWarning(manager.LastSaveError, this);
        }
        public void ShowCredits()
        {
            if (creditsPanel == null) return;
            var events = UnityEngine.EventSystems.EventSystem.current;
            creditsReturn = events != null ? events.currentSelectedGameObject : null;
            creditsPanel.SetActive(true);
            // Keyboard focus must move into the panel; otherwise Enter re-submits the hidden menu button.
            var back = creditsPanel.GetComponentInChildren<Button>();
            if (events != null && back != null) events.SetSelectedGameObject(back.gameObject);
        }
        public void HideCredits()
        {
            if (creditsPanel == null) return;
            creditsPanel.SetActive(false);
            var events = UnityEngine.EventSystems.EventSystem.current;
            if (events != null && creditsReturn != null && creditsReturn.activeInHierarchy)
                events.SetSelectedGameObject(creditsReturn);
        }
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
