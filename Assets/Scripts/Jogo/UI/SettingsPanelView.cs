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
        public bool IsCapturing => capturingAction.HasValue;

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
                    heading.text = "TECLA JÁ UTILIZADA";
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
            var frame = Resources.Load<Sprite>("SettingsFrameV1");
            if (cog == null || frame == null || pause == null || template == null) return;

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

            settingsPanel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image));
            settingsPanel.transform.SetParent(pause.transform.parent, false);
            var rootRect = (RectTransform)settingsPanel.transform;
            rootRect.anchorMin = Vector2.zero; rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
            settingsPanel.GetComponent<Image>().color = new Color(.035f, .018f, .065f, .96f);

            var frameObject = new GameObject("SettingsFrame", typeof(RectTransform), typeof(Image));
            frameObject.transform.SetParent(settingsPanel.transform, false);
            var frameRect = (RectTransform)frameObject.transform;
            frameRect.anchorMin = frameRect.anchorMax = new Vector2(.5f, .5f);
            frameRect.sizeDelta = new Vector2(1120f, 710f);
            var frameImage = frameObject.GetComponent<Image>();
            frameImage.sprite = frame; frameImage.preserveAspect = false;
            frameImage.raycastTarget = false;

            heading = new GameObject("SettingsTitle", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
            heading.transform.SetParent(settingsPanel.transform, false);
            var buttonLabel = template.GetComponentInChildren<TMP_Text>();
            if (buttonLabel != null) heading.font = buttonLabel.font;
            else if (titleTemplate != null) heading.font = titleTemplate.font;
            heading.text = "CONFIGURAÇÕES";
            heading.fontSize = 34f;
            heading.alignment = TextAlignmentOptions.Center;
            heading.color = new Color(.92f, .85f, 1f);
            var titleRect = heading.rectTransform;
            titleRect.anchorMin = titleRect.anchorMax = new Vector2(.5f, .5f);
            titleRect.anchoredPosition = new Vector2(0f, 195f);
            titleRect.sizeDelta = new Vector2(850f, 78f);

            var content = new GameObject("PurpleContentPanel", typeof(RectTransform), typeof(Image));
            content.transform.SetParent(settingsPanel.transform, false);
            var contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = contentRect.anchorMax = new Vector2(.5f, .5f);
            contentRect.anchoredPosition = new Vector2(177f, -30f);
            contentRect.sizeDelta = new Vector2(615f, 390f);
            var paperImage = content.GetComponent<Image>();
            paperImage.sprite = null;
            paperImage.color = new Color(.105f, .058f, .17f, .98f);

            var navigation = new GameObject("SettingsCategories", typeof(RectTransform), typeof(VerticalLayoutGroup));
            navigation.transform.SetParent(settingsPanel.transform, false);
            var navRect = (RectTransform)navigation.transform;
            navRect.anchorMin = navRect.anchorMax = new Vector2(.5f, .5f);
            navRect.anchoredPosition = new Vector2(-353f, -30f);
            navRect.sizeDelta = new Vector2(300f, 390f);
            var navLayout = navigation.GetComponent<VerticalLayoutGroup>();
            navLayout.spacing = 13f; navLayout.childAlignment = TextAnchor.UpperCenter;
            navLayout.childControlHeight = navLayout.childControlWidth = true;
            navLayout.childForceExpandHeight = navLayout.childForceExpandWidth = false;
            var tabSprite = Resources.Load<Sprite>("SettingsTabFrameV1");
            for (var i = 0; i < tabs.Length; i++)
            {
                var page = i;
                var tab = CloneButton(template, navigation.transform, "Tab_" + i);
                var tabSize = tab.GetComponent<LayoutElement>();
                if (tabSize != null) { tabSize.minWidth = 300f; tabSize.preferredWidth = 300f; tabSize.minHeight = 74f; tabSize.preferredHeight = 74f; }
                ((RectTransform)tab.transform).sizeDelta = new Vector2(300f, 74f);
                tab.image.sprite = tabSprite;
                tab.image.type = tabSprite == null ? Image.Type.Simple : Image.Type.Sliced;
                tab.image.color = Color.white;
                var tabLabel = tab.GetComponentInChildren<TMP_Text>();
                if (tabLabel != null) { tabLabel.fontSize = 22f; tabLabel.enableAutoSizing = false; tabLabel.margin = new Vector4(16f, 4f, 16f, 4f); }
                tab.onClick.AddListener(() => { section = page; controlPage = 0; capturingAction = null; Render(); });
                tabs[i] = tab;
            }

            var list = new GameObject("SettingsRows", typeof(RectTransform), typeof(VerticalLayoutGroup));
            list.transform.SetParent(content.transform, false);
            var listRect = (RectTransform)list.transform;
            listRect.anchorMin = listRect.anchorMax = new Vector2(.5f, .5f);
            listRect.anchoredPosition = new Vector2(0f, -4f);
            listRect.sizeDelta = new Vector2(545f, 340f);
            var layout = list.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 9f; layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = layout.childControlWidth = true;
            layout.childForceExpandHeight = layout.childForceExpandWidth = false;
            for (var i = 0; i < rows.Length; i++)
            {
                rows[i] = CloneButton(template, list.transform, "Setting_" + i);
                rows[i].image.sprite = null;
                rows[i].image.type = Image.Type.Simple;
                rows[i].image.color = new Color(.11f, .065f, .19f, .96f);
                var value = new GameObject("Value", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
                value.transform.SetParent(rows[i].transform, false);
                value.font = rows[i].GetComponentInChildren<TMP_Text>().font;
                value.fontSize = 21f;
                value.alignment = TextAlignmentOptions.MidlineRight;
                value.color = new Color(.95f, .85f, 1f);
                value.raycastTarget = false;
                value.rectTransform.anchorMin = new Vector2(.73f, 0f);
                value.rectTransform.anchorMax = new Vector2(.95f, 1f);
                value.rectTransform.offsetMin = value.rectTransform.offsetMax = Vector2.zero;
                rowValues[i] = value;
            }
            rowFontSize = 21f;
            for (var i = 0; i < rows.Length; i++) audioSliders[i] = CreateSlider(rows[i], i);
            var back = CloneButton(template, list.transform, "Button_Voltar");
            back.GetComponentInChildren<TMP_Text>().text = "VOLTAR";
            back.GetComponentInChildren<TMP_Text>().fontSize = 22f;
            MenuButtonStyle.Apply(back, null, .6f);
            back.onClick.AddListener(Close);
            settingsPanel.SetActive(false);
        }

        private static Button CloneButton(Button template, Transform parent, string name)
        {
            var result = Instantiate(template, parent);
            result.name = name;
            result.onClick = new Button.ButtonClickedEvent();
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
            var track = new GameObject("Track", typeof(RectTransform), typeof(Image));
            track.transform.SetParent(root.transform, false);
            var trackRect = (RectTransform)track.transform;
            trackRect.anchorMin = Vector2.zero; trackRect.anchorMax = Vector2.one;
            trackRect.offsetMin = trackRect.offsetMax = Vector2.zero;
            var trackImage = track.GetComponent<Image>();
            trackImage.sprite = Resources.Load<Sprite>("SettingsSliderTrackV1");
            trackImage.preserveAspect = false;
            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(root.transform, false);
            var handleRect = (RectTransform)handle.transform;
            handleRect.sizeDelta = new Vector2(38f, 38f);
            var handleImage = handle.GetComponent<Image>();
            handleImage.sprite = Resources.Load<Sprite>("SettingsSliderThumbV1");
            handleImage.preserveAspect = true;
            handleImage.raycastTarget = false;
            var slider = root.GetComponent<Slider>();
            slider.minValue = 0f; slider.maxValue = 1f;
            slider.handleRect = handleRect;
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
            heading.text = "CONFIGURAÇÕES";
            for (var i = 0; i < tabs.Length; i++)
            {
                tabs[i].GetComponentInChildren<TMP_Text>().text = sectionNames[i];
                tabs[i].image.color = i == section ? Color.white : new Color(.62f, .52f, .73f, 1f);
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
            if (section == 0)
            {
                SetAudioRow(0, "GERAL", s.MasterVolume, s.SetMasterVolume);
                SetAudioRow(1, "MÚSICA", s.MusicVolume, s.SetMusicVolume);
                SetAudioRow(2, "EFEITOS", s.EffectsVolume, s.SetEffectsVolume);
                SetAudioRow(3, "VOZES", s.VoiceVolume, s.SetVoiceVolume);
            }
            else if (section == 1)
            {
                SetRow(0, "TELA CHEIA  " + (Screen.fullScreen ? "SIM" : "NÃO"), () => { Screen.fullScreen = !Screen.fullScreen; PlayerPrefs.SetInt("MM.Settings.Fullscreen", Screen.fullScreen ? 1 : 0); Render(); });
                SetRow(1, "RESOLUÇÃO  " + Screen.width + " × " + Screen.height, () => { CycleResolution(); Render(); });
                rows[2].gameObject.SetActive(false);
                SetRow(3, "TAMANHO DO TEXTO  " + s.FontScale.ToString("0.0") + "×", () => { s.SetFontScale(s.FontScale >= 1.6f ? .8f : s.FontScale + .2f); Render(); });
            }
            else if (section == 2)
            {
                SetRow(0, "VELOCIDADE DO TEXTO  " + s.TextSpeed.ToString("0.0") + "×", () => { s.SetTextSpeed(s.TextSpeed >= 2f ? .5f : s.TextSpeed + .25f); Render(); });
                SetRow(1, "LEGENDAS SONORAS  " + (s.SoundCaptions ? "SIM" : "NÃO"), () => { s.SetSoundCaptions(!s.SoundCaptions); Render(); });
                SetRow(2, "ESPAÇO EXTRA  " + (s.RemapInteractToSpace ? "SIM" : "NÃO"), () => { s.SetRemapInteractToSpace(!s.RemapInteractToSpace); Render(); });
                SetRow(3, "RESTAURAR PADRÕES", () => { s.RestoreDefaults(); Render(); });
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
                    SetRow(i, names[index] + "  " + InputBindings.Get(action), () =>
                    {
                        capturingAction = action;
                        rows[index % 3].GetComponentInChildren<TMP_Text>().text = "PRESSIONE UMA TECLA";
                    });
                }
                SetRow(3, controlPage < 2 ? "MAIS TECLAS" : "INÍCIO", () =>
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
