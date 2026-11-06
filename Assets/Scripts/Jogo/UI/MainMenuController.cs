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
            AddSettingsButton(play, button);
            ApplyCamilaComposition(play.transform.parent as RectTransform, shortcut);
        }

        // CONFIGURAÇÕES as a real menu entry (same approved button art), right after CONTINUAR.
        private void AddSettingsButton(Button play, Button continueButton)
        {
            var settings = Instantiate(play, play.transform.parent);
            settings.name = "Button_ConfiguracoesMenu";
            settings.transform.SetSiblingIndex(continueButton.transform.GetSiblingIndex() + 1);
            settings.onClick = new Button.ButtonClickedEvent();
            settings.onClick.AddListener(() => settingsView?.Open());
            settings.interactable = true;
            var label = settings.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = "CONFIGURAÇÕES";
        }

        /// <summary>
        /// Camila's original illustration as the menu background, figure centre-right, the existing logo
        /// and buttons moved to a left column. Everything is a separate overlay: the art file is untouched
        /// and removing this call restores the previous menu.
        /// </summary>
        private static void ApplyCamilaComposition(RectTransform buttons, RectTransform settingsCog)
        {
            var art = Resources.Load<Sprite>("MainMenu/CamilaIlustracao");
            var canvas = buttons != null ? buttons.parent as RectTransform : null;
            if (art == null || canvas == null) return;

            var placeholder = canvas.Find("MainMenuBackgroundPlaceholder");
            var insertAt = placeholder != null ? placeholder.GetSiblingIndex() + 1 : 0;

            // Full height, aspect preserved, right-aligned: the figure sits centre-right with no crop.
            var artRect = NewImage("CamilaIlustracao", canvas, insertAt, art, Color.white);
            artRect.anchorMin = artRect.anchorMax = artRect.pivot = new Vector2(1f, .5f);
            var height = canvas.rect.height > 0f ? canvas.rect.height : 1080f;
            artRect.sizeDelta = new Vector2(height * art.rect.width / art.rect.height, height);
            artRect.anchoredPosition = Vector2.zero;

            // Reversible chromatic treatment (Codex proposal #713580 at low strength) as its own layer.
            var tint = NewImage("CamilaTratamentoCromatico", canvas, insertAt + 1, null, new Color(.443f, .208f, .502f, .12f));
            Stretch(tint);

            // Left shade so the controls read over the roses; fades out before the figure.
            var shade = NewImage("CamilaSombraEsquerda", canvas, insertAt + 2, MakeHorizontalFade(), new Color(.02f, .008f, .04f, 1f));
            shade.anchorMin = Vector2.zero; shade.anchorMax = new Vector2(.5f, 1f);
            shade.offsetMin = shade.offsetMax = Vector2.zero;

            foreach (var name in new[] { "R4Corner0", "R4Corner1", "R4Corner2", "R4Corner3" })
            {
                // The illustration brings its own silver corners.
                var decoration = canvas.Find(name);
                if (decoration != null) decoration.gameObject.SetActive(false);
            }
            if (settingsCog != null) settingsCog.gameObject.SetActive(false);

            const float column = -575f;
            // The previous menu's sigil oval returns as the backdrop of the left column, behind logo and buttons.
            var sigil = canvas.Find("CenterSigil");
            if (sigil is RectTransform sigilRect)
            {
                sigilRect.SetSiblingIndex(shade.GetSiblingIndex() + 1);
                Move(sigilRect, new Vector2(column, 20f), .98f);
                var sigilImage = sigilRect.GetComponent<Image>();
                if (sigilImage != null) sigilImage.color = new Color(.62f, .5f, .74f, .42f);
            }
            Move(canvas.Find("MoonEmblem"), new Vector2(column, 440f), .9f);
            // Larger title: the logo is the identity of the screen.
            Move(canvas.Find("Logo_MementoMoriBeta"), new Vector2(column, 255f), .95f);
            var layout = buttons.GetComponent<VerticalLayoutGroup>();
            if (layout != null) layout.spacing = 14f;
            // Five entries now: size the column to its buttons so the layout stops spreading them apart.
            buttons.sizeDelta = new Vector2(buttons.sizeDelta.x, 5 * 96f + 4 * 14f);
            buttons.anchoredPosition = new Vector2(column, -210f);
            buttons.localScale = Vector3.one * .78f;
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

        private static Sprite MakeHorizontalFade()
        {
            const int width = 256;
            var texture = new Texture2D(width, 1, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (var x = 0; x < width; x++)
            {
                var t = x / (width - 1f);
                texture.SetPixel(x, 0, new Color(1f, 1f, 1f, .94f * (1f - t * t)));
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, 1), new Vector2(.5f, .5f));
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
