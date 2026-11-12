using MementoMori.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MementoMori.UI
{
    /// <summary>Builds settings from independent Unity UI elements over reusable sprite layers.</summary>
    public sealed class SettingsPanelView : MonoBehaviour
    {
        private GameObject pausePanel;
        private GameObject mainButtons;
        private GameObject slotButtons;
        private GameObject settingsPanel;
        private TMP_Text heading;
        private readonly Button[] rows = new Button[4];
        private readonly TMP_Text[] rowValues = new TMP_Text[4];
        private readonly Button[] tabs = new Button[4];
        private readonly Slider[] audioSliders = new Slider[4];
        private float rowFontSize = 30f;
        private int section;
        private int controlPage;
        private GameAction? capturingAction;
        public int CaptureConsumedFrame { get; private set; } = -1;
        public bool IsOpen => settingsPanel != null && settingsPanel.activeSelf;
        /// <summary>Raised with true on open and false on close, so the screen behind can hide its chrome.</summary>
        public event System.Action<bool> VisibilityChanged;
        public bool IsCapturing => capturingAction.HasValue;

        private void OnEnable() => Loc.LanguageChanged += RefreshLanguage;
        private void OnDisable() => Loc.LanguageChanged -= RefreshLanguage;
        private void RefreshLanguage() { if (IsOpen) Render(); }

        private void Update()
        {
            if (!IsCapturing || !Input.anyKeyDown) return;
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (!InputBindings.IsSupported(key) || !Input.GetKeyDown(key)) continue;
                var action = capturingAction.Value;
                capturingAction = null;
                CaptureConsumedFrame = Time.frameCount;
                if (key == KeyCode.Escape) { Render(); return; }
                if (!InputBindings.TryBind(action, key))
                {
                    Render();
                    heading.text = Loc.T("TECLA JÁ UTILIZADA");
                    return;
                }
                Render();
                return;
            }
        }

        public void Configure(GameObject pause, GameObject main, GameObject slots, Button template, TMP_Text titleTemplate)
        {
            pausePanel = pause; mainButtons = main; slotButtons = slots;
            var cog = Resources.Load<Sprite>("SettingsCogV1");
            if (cog == null || pause == null || template == null) return;

            var iconObject = new GameObject("Button_Configuracoes", typeof(RectTransform), typeof(Image), typeof(Button));
            iconObject.transform.SetParent(pause.transform, false);
            var iconRect = (RectTransform)iconObject.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(1f, 1f);
            iconRect.pivot = new Vector2(.5f, .5f);
            iconRect.anchoredPosition = new Vector2(-78f, -74f);
            iconRect.sizeDelta = new Vector2(70f, 70f);
            var iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = cog; iconImage.preserveAspect = true;
            iconObject.GetComponent<Button>().onClick.AddListener(Open);

            settingsPanel = new GameObject("SettingsPanel", typeof(RectTransform));
            settingsPanel.transform.SetParent(pause.transform.parent, false);
            MenuPanelStyle.Stretch((RectTransform)settingsPanel.transform);
            MenuPanelStyle.Veil(settingsPanel.transform, .82f);
            var panel = MenuPanelStyle.Panel(settingsPanel.transform, new Vector2(1240f, 780f));

            var buttonLabel = template.GetComponentInChildren<TMP_Text>();
            var font = buttonLabel != null ? buttonLabel.font : titleTemplate != null ? titleTemplate.font : null;
            heading = MenuPanelStyle.Title(panel, "CONFIGURAÇÕES", font);

            // Categories in one row under the title, in the menu's button frame.
            var navigation = new GameObject("SettingsCategories", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            navigation.transform.SetParent(panel, false);
            var navRect = (RectTransform)navigation.transform;
            navRect.anchorMin = navRect.anchorMax = new Vector2(.5f, 1f);
            navRect.anchoredPosition = new Vector2(0f, -205f);
            navRect.sizeDelta = new Vector2(1040f, 70f);
            var navLayout = navigation.GetComponent<HorizontalLayoutGroup>();
            navLayout.spacing = 20f; navLayout.childAlignment = TextAnchor.MiddleCenter;
            navLayout.childControlHeight = navLayout.childControlWidth = true;
            navLayout.childForceExpandHeight = navLayout.childForceExpandWidth = false;
            for (var i = 0; i < tabs.Length; i++)
            {
                var page = i;
                var tab = CloneButton(template, navigation.transform, "Tab_" + i);
                var tabSize = tab.GetComponent<LayoutElement>() ?? tab.gameObject.AddComponent<LayoutElement>();
                tabSize.minWidth = tabSize.preferredWidth = 250f; tabSize.minHeight = tabSize.preferredHeight = 70f;
                MenuButtonStyle.Apply(tab, new Vector2(250f, 70f), .5f);
                var tabLabel = tab.GetComponentInChildren<TMP_Text>();
                if (tabLabel != null) { tabLabel.enableAutoSizing = false; tabLabel.fontSize = 22f; }
                tab.onClick.AddListener(() => { section = page; controlPage = 0; capturingAction = null; Render(); });
                tabs[i] = tab;
            }

            var list = new GameObject("SettingsRows", typeof(RectTransform), typeof(VerticalLayoutGroup));
            list.transform.SetParent(panel, false);
            var listRect = (RectTransform)list.transform;
            listRect.anchorMin = listRect.anchorMax = new Vector2(.5f, .5f);
            listRect.anchoredPosition = new Vector2(0f, -40f);
            listRect.sizeDelta = new Vector2(900f, 330f);
            var layout = list.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 12f; layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = layout.childControlWidth = true;
            layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
            for (var i = 0; i < rows.Length; i++)
            {
                rows[i] = CloneButton(template, list.transform, "Setting_" + i);
                var rowSize = rows[i].GetComponent<LayoutElement>() ?? rows[i].gameObject.AddComponent<LayoutElement>();
                rowSize.minHeight = rowSize.preferredHeight = 70f;
                foreach (var extra in new[] { "SelectorLeft", "SelectorRight", "GlowOverlay" })
                    if (rows[i].transform.Find(extra) is Transform hidden) hidden.gameObject.SetActive(false);
                rows[i].image.sprite = null;
                rows[i].image.type = Image.Type.Simple;
                rows[i].image.color = new Color(.16f, .085f, .24f, .55f);
                rows[i].transition = Selectable.Transition.ColorTint;
                var colors = rows[i].colors;
                colors.normalColor = Color.white; colors.highlightedColor = colors.selectedColor = new Color(1.6f, 1.4f, 1.8f, 1f);
                colors.colorMultiplier = 1.4f;
                rows[i].colors = colors;
                var underline = MenuPanelStyle.NewImage("Linha", rows[i].transform, null, MenuPanelStyle.LilacSoft);
                underline.anchorMin = Vector2.zero; underline.anchorMax = new Vector2(1f, 0f);
                underline.offsetMin = Vector2.zero; underline.offsetMax = new Vector2(0f, 1f);
                var value = MenuPanelStyle.NewText("Value", rows[i].transform, rows[i].GetComponentInChildren<TMP_Text>().font, string.Empty, 24f, MenuPanelStyle.TitleColor);
                value.alignment = TextAlignmentOptions.MidlineRight;
                value.rectTransform.anchorMin = new Vector2(.73f, 0f);
                value.rectTransform.anchorMax = new Vector2(.95f, 1f);
                value.rectTransform.offsetMin = value.rectTransform.offsetMax = Vector2.zero;
                rowValues[i] = value;
            }
            rowFontSize = 24f;
            for (var i = 0; i < rows.Length; i++) audioSliders[i] = CreateSlider(rows[i], i);

            var back = CloneButton(template, panel, "Button_Voltar");
            var backRect = (RectTransform)back.transform;
            backRect.anchorMin = backRect.anchorMax = backRect.pivot = new Vector2(.5f, 0f);
            backRect.anchoredPosition = new Vector2(0f, 46f);
            back.GetComponentInChildren<TMP_Text>().text = "VOLTAR";
            LocalizedText.Bind(back.GetComponentInChildren<TMP_Text>());
            MenuButtonStyle.Apply(back, new Vector2(380f, 84f), .6f);
            back.onClick.AddListener(Close);
            settingsPanel.SetActive(false);
        }

        private static Button CloneButton(Button template, Transform parent, string name)
        {
            var result = Instantiate(template, parent);
            result.name = name;
            result.onClick = new Button.ButtonClickedEvent();
            result.GetComponent<MenuButtonFeedback>()?.Rebind();
            foreach (var label in result.GetComponentsInChildren<TMP_Text>(true)) LocalizedText.Unbind(label);
            var size = result.GetComponent<LayoutElement>();
            if (size != null) { size.minHeight = 55f; size.preferredHeight = 55f; }
            return result;
        }

        private static Slider CreateSlider(Button row, int index)
        {
            var root = new GameObject("AudioSlider_" + index, typeof(RectTransform), typeof(Slider));
            root.transform.SetParent(row.transform, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = new Vector2(.50f, .16f); rect.anchorMax = new Vector2(.76f, .84f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            // Drawn track and fill with the menu's star as the handle (no atlas sprite needed).
            var track = MenuPanelStyle.NewImage("Track", root.transform, null, new Color(.3f, .2f, .4f, .9f));
            track.anchorMin = new Vector2(0f, .5f); track.anchorMax = new Vector2(1f, .5f);
            track.offsetMin = new Vector2(0f, -2f); track.offsetMax = new Vector2(0f, 2f);
            var fillArea = new GameObject("FillArea", typeof(RectTransform));
            var fillAreaRect = (RectTransform)fillArea.transform;
            fillAreaRect.SetParent(root.transform, false);
            fillAreaRect.anchorMin = new Vector2(0f, .5f); fillAreaRect.anchorMax = new Vector2(1f, .5f);
            fillAreaRect.offsetMin = new Vector2(0f, -2f); fillAreaRect.offsetMax = new Vector2(0f, 2f);
            var fill = MenuPanelStyle.NewImage("Fill", fillAreaRect, null, MenuPanelStyle.Lilac);
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            var handleArea = new GameObject("HandleArea", typeof(RectTransform));
            var handleAreaRect = (RectTransform)handleArea.transform;
            handleAreaRect.SetParent(root.transform, false);
            MenuPanelStyle.Stretch(handleAreaRect);
            var handleRect = MenuPanelStyle.NewImage("Handle", handleAreaRect, MenuPanelStyle.Diamond(), MenuPanelStyle.TitleColor);
            handleRect.sizeDelta = new Vector2(30f, 30f);
            var handleImage = handleRect.GetComponent<Image>();
            handleImage.raycastTarget = true;
            var slider = root.GetComponent<Slider>();
            slider.minValue = 0f; slider.maxValue = 1f;
            slider.handleRect = handleRect;
            slider.fillRect = fill;
            slider.targetGraphic = handleImage;
            slider.gameObject.SetActive(false);
            return slider;
        }

        public void Open()
        {
            if (settingsPanel == null) return;
            section = 0; controlPage = 0; capturingAction = null; Render();
            if (pausePanel != null) pausePanel.SetActive(false);
            settingsPanel.SetActive(true);
            VisibilityChanged?.Invoke(true);
        }

        public void Close()
        {
            if (!IsOpen) return;
            capturingAction = null;
            PlayerPrefs.Save();
            settingsPanel.SetActive(false);
            if (slotButtons != null) slotButtons.SetActive(false);
            if (mainButtons != null) mainButtons.SetActive(true);
            if (pausePanel != null) pausePanel.SetActive(true);
            VisibilityChanged?.Invoke(false);
        }

        private void SetRow(int index, string caption, UnityEngine.Events.UnityAction action)
        {
            var button = rows[index];
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(action);
            var split = caption.IndexOf("  ", System.StringComparison.Ordinal);
            var label = button.GetComponentInChildren<TMP_Text>();
            label.text = split < 0 ? caption : caption.Substring(0, split);
            rowValues[index].text = split < 0 ? string.Empty : caption.Substring(split + 2);
            if (split >= 0)
            {
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.rectTransform.anchorMin = new Vector2(.05f, 0f);
                label.rectTransform.anchorMax = new Vector2(.72f, 1f);
            }
        }

        private void SetAudioRow(int index, string name, float value, UnityEngine.Events.UnityAction<float> change)
        {
            var label = rows[index].GetComponentInChildren<TMP_Text>();
            label.text = name;
            rowValues[index].text = Percent(value);
            label.fontSize = 21f;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(.06f, 0f);
            labelRect.anchorMax = new Vector2(.48f, 1f);
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            rowValues[index].rectTransform.anchorMin = new Vector2(.79f, 0f);
            rowValues[index].rectTransform.anchorMax = new Vector2(.96f, 1f);
            rows[index].onClick = new Button.ButtonClickedEvent();
            var slider = audioSliders[index];
            slider.gameObject.SetActive(true);
            slider.onValueChanged.RemoveAllListeners();
            slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(v => { change(v); rowValues[index].text = Percent(v); });
        }

        private void Render()
        {
            var s = AccessibilitySettings.Instance;
            if (s == null) return;
            var sectionNames = new[] { "ÁUDIO", "VÍDEO", "LEITURA", "CONTROLES" };
            heading.text = Loc.T("CONFIGURAÇÕES");
            for (var i = 0; i < tabs.Length; i++)
            {
                tabs[i].GetComponentInChildren<TMP_Text>().text = Loc.T(sectionNames[i]);
                tabs[i].image.color = i == section ? Color.white : new Color(.55f, .46f, .66f, 1f);
                var tabText = tabs[i].GetComponentInChildren<TMP_Text>();
                tabText.color = i == section ? MenuPanelStyle.TitleColor : MenuPanelStyle.CaptionColor;
            }
            for (var i = 0; i < rows.Length; i++)
            {
                rows[i].gameObject.SetActive(true);
                rows[i].image.enabled = true;
                rowValues[i].text = string.Empty;
                rowValues[i].rectTransform.anchorMin = new Vector2(.73f, 0f);
                rowValues[i].rectTransform.anchorMax = new Vector2(.95f, 1f);
                audioSliders[i].gameObject.SetActive(false);
                var label = rows[i].GetComponentInChildren<TMP_Text>();
                label.fontSize = rowFontSize;
                label.alignment = TextAlignmentOptions.Center;
                label.rectTransform.anchorMin = Vector2.zero;
                label.rectTransform.anchorMax = Vector2.one;
                label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
            }
            var yes = Loc.T("SIM"); var no = Loc.T("NÃO");
            if (section == 0)
            {
                SetAudioRow(0, Loc.T("GERAL"), s.MasterVolume, s.SetMasterVolume);
                SetAudioRow(1, Loc.T("MÚSICA"), s.MusicVolume, s.SetMusicVolume);
                SetAudioRow(2, Loc.T("EFEITOS"), s.EffectsVolume, s.SetEffectsVolume);
                SetAudioRow(3, Loc.T("VOZES"), s.VoiceVolume, s.SetVoiceVolume);
            }
            else if (section == 1)
            {
                SetRow(0, Loc.T("TELA CHEIA") + "  " + (Screen.fullScreen ? yes : no), () => { Screen.fullScreen = !Screen.fullScreen; PlayerPrefs.SetInt("MM.Settings.Fullscreen", Screen.fullScreen ? 1 : 0); Render(); });
                SetRow(1, Loc.T("RESOLUÇÃO") + "  " + Screen.width + " × " + Screen.height, () => { CycleResolution(); Render(); });
                SetRow(2, Loc.T("IDIOMA") + "  " + (s.Language == "en" ? "ENGLISH" : "PORTUGUÊS"), () => { s.SetLanguage(s.Language == "en" ? "pt" : "en"); Render(); });
                SetRow(3, Loc.T("TAMANHO DO TEXTO") + "  " + s.FontScale.ToString("0.0") + "×", () => { s.SetFontScale(s.FontScale >= 1.6f ? .8f : s.FontScale + .2f); Render(); });
            }
            else if (section == 2)
            {
                SetRow(0, Loc.T("VELOCIDADE DO TEXTO") + "  " + s.TextSpeed.ToString("0.0") + "×", () => { s.SetTextSpeed(s.TextSpeed >= 2f ? .5f : s.TextSpeed + .25f); Render(); });
                SetRow(1, Loc.T("LEGENDAS SONORAS") + "  " + (s.SoundCaptions ? s.FootstepCaptions ? Loc.T("SIM, COM PASSOS") : yes : no), () =>
                {
                    if (!s.SoundCaptions) s.SetSoundCaptions(true);
                    else if (!s.FootstepCaptions) s.SetFootstepCaptions(true);
                    else { s.SetSoundCaptions(false); s.SetFootstepCaptions(false); }
                    Render();
                });
                SetRow(2, Loc.T("ESPAÇO EXTRA") + "  " + (s.RemapInteractToSpace ? yes : no), () => { s.SetRemapInteractToSpace(!s.RemapInteractToSpace); Render(); });
                SetRow(3, Loc.T("RESTAURAR PADRÕES"), () => { s.RestoreDefaults(); Render(); });
            }
            else
            {
                var actions = new[] { GameAction.MoveLeft, GameAction.MoveRight, GameAction.MoveUp,
                    GameAction.MoveDown, GameAction.Interact, GameAction.Grimoire, GameAction.Pause };
                var names = new[] { "ESQUERDA", "DIREITA", "CIMA", "BAIXO", "INTERAGIR", "GRIMÓRIO", "PAUSA" };
                for (var i = 0; i < 3; i++)
                {
                    var index = controlPage * 3 + i;
                    if (index >= actions.Length) { rows[i].gameObject.SetActive(false); continue; }
                    var action = actions[index];
                    SetRow(i, Loc.T(names[index]) + "  " + InputBindings.Get(action), () =>
                    {
                        capturingAction = action;
                        rows[index % 3].GetComponentInChildren<TMP_Text>().text = Loc.T("PRESSIONE UMA TECLA");
                    });
                }
                SetRow(3, controlPage < 2 ? Loc.T("MAIS TECLAS") : Loc.T("INÍCIO"), () =>
                {
                    controlPage = (controlPage + 1) % 3;
                    Render();
                });
            }
        }

        private static string Percent(float value) => Mathf.RoundToInt(value * 100f) + "%";
        private static void CycleResolution()
        {
            var options = Screen.resolutions;
            if (options.Length == 0) return;
            var index = 0;
            for (var i = 0; i < options.Length; i++)
                if (options[i].width == Screen.width && options[i].height == Screen.height) { index = i; break; }
            var next = options[(index + 1) % options.Length];
            Screen.SetResolution(next.width, next.height, Screen.fullScreen);
            PlayerPrefs.SetInt("MM.Settings.Width", next.width);
            PlayerPrefs.SetInt("MM.Settings.Height", next.height);
        }
    }
}
