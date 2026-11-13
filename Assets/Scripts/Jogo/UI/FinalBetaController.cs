using System;
using System.Collections;
using MementoMori.Core;
using MementoMori.Dialogue;
using UnityEngine;
using UnityEngine.UI;
using MementoMori.World;

namespace MementoMori.UI
{
    /// <summary>Runs the short, fragment-gated narrative closure of the beta slice.</summary>
    public sealed class FinalBetaController : MonoBehaviour
    {
        [SerializeField] private DialogueData[] narrativeEntries;
        [SerializeField] private Text finalText;
        [SerializeField] private TMPro.TMP_Text finalTextTmp;
        [SerializeField] private Button returnButton;
        [SerializeField] private GameObject credits;
        [SerializeField] private GameObject circleActive;
        [SerializeField] private GameObject circleOff;
        [SerializeField] private GameObject[] wakeEvidence;
        [SerializeField] private GameObject endCardRoot;

        private const string ClosingMessage = "O que a gente lembra volta com a gente.\n\nObrigado por jogar Memento Mori.";
        private int entryIndex;
        private bool sequenceComplete;
        private Coroutine creditsSequence;
        private bool returning;
        private Text speakerLabel;
        private Text dialogueLabel;

        public bool SequenceComplete => sequenceComplete;
        public int ImplementedEntryCount => narrativeEntries == null ? 0 : narrativeEntries.Length;

        private void Start()
        {
            var roomVisual = FindAnyObjectByType<RoomRitualVisual>();
            if (roomVisual != null) roomVisual.SetReturnedRoom(true);
            RestyleEndCard();
            if (endCardRoot != null) endCardRoot.SetActive(false);
            EnsureCanonicalNarrativeEntry();
            if (returnButton != null)
            {
                returnButton.interactable = false;
                returnButton.onClick.AddListener(ReturnToMenu);
            }

            if (credits != null) credits.SetActive(false);
            if (GameState.Instance == null || !GameState.Instance.HasFlag(StoryFlag.FragmentCollected))
            {
                Debug.LogWarning("FinalBeta requires FragmentCollected; narrative closure was not started.", this);
                return;
            }

            if (finalText != null) finalText.gameObject.SetActive(false);
            if (circleActive != null) circleActive.SetActive(false);
            if (circleOff != null) circleOff.SetActive(true);
            if (wakeEvidence != null)
                foreach (var evidence in wakeEvidence)
                    if (evidence != null) evidence.SetActive(true);
            EnsureDialogueManager();
            StartNextEntry();
        }

        private void EnsureCanonicalNarrativeEntry()
        {
            // The scene still serializes the earlier draft. Runtime uses the V6.1 script.
            narrativeEntries = new[] { V61EventCatalog.CreateDialogue("FINAL") };
        }

        private void EnsureDialogueManager()
        {
            if (DialogueManager.Instance != null) return;

            var root = new GameObject("FinalBetaDialogue", typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var panel = new GameObject("Panel", typeof(Image));
            panel.transform.SetParent(root.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(.12f, .08f);
            panelRect.anchorMax = new Vector2(.88f, .32f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(.05f, .04f, .1f, .96f);

            speakerLabel = CreateLabel(panel.transform, "Speaker", 22, new Vector2(.05f, .68f), new Vector2(.95f, .92f));
            dialogueLabel = CreateLabel(panel.transform, "Body", 20, new Vector2(.05f, .12f), new Vector2(.95f, .68f));
            root.AddComponent<DialogueManager>().Configure(root.GetComponent<CanvasGroup>(), speakerLabel, dialogueLabel);
        }

        private static Text CreateLabel(Transform parent, string name, int size, Vector2 min, Vector2 max)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);
            var label = labelObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = size;
            label.color = Color.white;
            label.alignment = TextAnchor.UpperLeft;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            var rect = label.rectTransform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return label;
        }

        private void StartNextEntry()
        {
            if (narrativeEntries == null || entryIndex >= narrativeEntries.Length)
            {
                CompleteSequence();
                return;
            }

            var dialogue = narrativeEntries[entryIndex++];
            if (entryIndex == 4) MementoMori.Audio.RuntimeAudio.PlayOneShot("15_poe_soft_call", .4f);
            if (dialogue == null)
            {
                StartNextEntry();
                return;
            }

            Action completed = null;
            completed = () =>
            {
                DialogueManager.Instance.OnDialogueCompleted -= completed;
                if (dialogue.SequenceId == "FINAL")
                {
                    GameState.Instance?.SetCounter("event.FINAL", 1);
                    Destroy(dialogue);
                }
                StartNextEntry();
            };
            DialogueManager.Instance.OnDialogueCompleted += completed;
            DialogueManager.Instance.StartDialogue(dialogue);
        }

        private void CompleteSequence()
        {
            sequenceComplete = true;
            if (endCardRoot != null) endCardRoot.SetActive(true);
            ShowFinalIllustration();
            if (finalText == null && finalTextTmp == null)
            {
                var canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null) finalText = CreateLabel(canvas.transform, "EndCard", 28, new Vector2(.15f, .62f), new Vector2(.85f, .82f));
            }
            if (finalText != null)
            {
                finalText.text = Loc.T(ClosingMessage);
                finalText.gameObject.SetActive(true);
            }
            if (finalTextTmp != null) finalTextTmp.text = Loc.T(ClosingMessage);
            if (returnButton != null) returnButton.interactable = true;
            if (credits != null && creditsSequence == null) creditsSequence = StartCoroutine(PlayClosingCredits());
        }

        private IEnumerator PlayClosingCredits()
        {
            // Leave the closing words readable before the animated night and real credits.
            yield return new WaitForSecondsRealtime(7f);
            if (returning) yield break;
            ShowCredits();
            yield return new WaitForSecondsRealtime(30f);
            ReturnToMenu();
        }

        private void Update()
        {
            if (sequenceComplete && credits != null && credits.activeInHierarchy
                && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)
                    || GamepadInput.ConfirmDown() || GamepadInput.CancelDown()))
                ReturnToMenu();
        }

        /// <summary>
        /// Closing card: Camila's original illustration behind the real closing message, credits and the
        /// return button. It only appears after the FINAL sequence; the playable FinalBeta room stays as is.
        /// </summary>
        private void ShowFinalIllustration()
        {
            var art = Resources.Load<Sprite>("MainMenu/CamilaIlustracao");
            if (art == null || GameObject.Find("FinalIllustration") != null) return;

            // Draw just under whichever overlay holds the end text / button, above the world and HUD frame.
            var hostCanvas = returnButton != null ? returnButton.GetComponentInParent<Canvas>()
                : finalText != null ? finalText.GetComponentInParent<Canvas>()
                : finalTextTmp != null ? finalTextTmp.GetComponentInParent<Canvas>() : null;
            var order = hostCanvas != null ? hostCanvas.rootCanvas.sortingOrder - 1 : 60;

            var root = new GameObject("FinalIllustration", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = .5f;

            var rootRect = (RectTransform)root.transform;
            var backdrop = NewLayer(root.transform, "Backdrop", null, new Color(.02f, .008f, .04f, 1f));
            backdrop.anchorMin = Vector2.zero; backdrop.anchorMax = Vector2.one;
            backdrop.offsetMin = backdrop.offsetMax = Vector2.zero;

            // Same treatment as the main menu: art covering the whole screen, violet wash, soft shades,
            // dark corners and the thin lilac frame.
            var image = NewLayer(root.transform, "CamilaIlustracao", art, Color.white);
            image.anchorMin = image.anchorMax = image.pivot = new Vector2(.5f, .5f);
            var cover = Mathf.Max(1920f / art.rect.width, 1080f / art.rect.height);
            image.sizeDelta = new Vector2(art.rect.width * cover, art.rect.height * cover);
            image.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            image.GetComponent<AspectRatioFitter>().aspectRatio = art.rect.width / art.rect.height;

            var wash = NewLayer(root.transform, "TratamentoRoxo", null, new Color(.2f, .07f, .26f, .3f));
            wash.anchorMin = Vector2.zero; wash.anchorMax = Vector2.one; wash.offsetMin = wash.offsetMax = Vector2.zero;
            var dark = new Color(.02f, .006f, .035f, 1f);
            var top = NewLayer(root.transform, "SombraTitulo", MainMenuController.MakeVerticalFade(false), dark);
            top.anchorMin = new Vector2(0f, .62f); top.anchorMax = Vector2.one; top.offsetMin = top.offsetMax = Vector2.zero;
            var bottom = NewLayer(root.transform, "SombraTexto", MainMenuController.MakeVerticalFade(true), dark);
            bottom.anchorMin = Vector2.zero; bottom.anchorMax = new Vector2(1f, .5f); bottom.offsetMin = bottom.offsetMax = Vector2.zero;
            var vignette = NewLayer(root.transform, "VinhetaCantos", Resources.Load<Sprite>("MainMenu/VinhetaMenu"), Color.white);
            vignette.anchorMin = Vector2.zero; vignette.anchorMax = Vector2.one; vignette.offsetMin = vignette.offsetMax = Vector2.zero;
            MainMenuController.AddThinFrame(rootRect, rootRect.childCount);
        }

        private static RectTransform NewLayer(Transform parent, string name, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return (RectTransform)go.transform;
        }

        /// <summary>
        /// Closing card in the menu's current look: Camila's illustration shows through a light veil, the
        /// buttons use the menu frame and the credits are the same panel as the main menu.
        /// </summary>
        private static void RestyleEndCard()
        {
            Transform ui = null;
            foreach (var t in FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name == "FinalBetaUI") { ui = t; break; }
            if (ui == null) return;
            if (ui.Find("Backdrop")?.GetComponent<Image>() is Image backdrop) backdrop.color = Color.clear;
            // The emblem sat over the illustration's face; the logo above already closes the card.
            ui.Find("CentralOrnament")?.gameObject.SetActive(false);
            foreach (var name in new[] { "CreditsButton", "ReturnButton", "QuitButton" })
                if (ui.Find(name)?.GetComponent<Button>() is Button button)
                {
                    MenuButtonStyle.Apply(button, new Vector2(380f, 84f), .6f);
                    LocalizedText.Bind(button.GetComponentInChildren<TMPro.TMP_Text>(true));
                }
            var credits = ui.Find("CreditsPanel") as RectTransform;
            if (credits == null || credits.Find("Painel") != null) return;
            var bodyFont = credits.Find("CreditsText")?.GetComponent<TMPro.TMP_Text>()?.font;
            var titleFont = ui.Find("CreditsButton")?.GetComponentInChildren<TMPro.TMP_Text>(true)?.font;
            if (credits.GetComponent<Image>() is Image panelImage) panelImage.color = Color.clear;
            credits.Find("CreditsText")?.gameObject.SetActive(false);
            credits.anchorMin = Vector2.zero; credits.anchorMax = Vector2.one;
            credits.offsetMin = credits.offsetMax = Vector2.zero;
            MenuPanelStyle.Veil(credits, .8f).SetAsFirstSibling();
            var ascii = AsciiCreditsView.Attach(credits, bodyFont);
            ascii.transform.SetSiblingIndex(1);
            var panel = MenuPanelStyle.Panel(credits, new Vector2(1000f, 820f));
            panel.SetSiblingIndex(2);
            MenuPanelStyle.Title(panel, "CRÉDITOS", titleFont);
            MenuPanelStyle.CreditsBody(panel, bodyFont);
            if (credits.Find("CloseCredits") is RectTransform back)
            {
                back.SetParent(panel, false);
                back.anchorMin = back.anchorMax = back.pivot = new Vector2(.5f, 0f);
                back.anchoredPosition = new Vector2(0f, 46f);
                MenuButtonStyle.Apply(back.GetComponent<Button>(), new Vector2(380f, 84f), .6f);
                LocalizedText.Bind(back.GetComponentInChildren<TMPro.TMP_Text>(true));
            }
        }

        public void ShowCredits() { if (sequenceComplete && credits != null) credits.SetActive(true); }
        public void HideCredits() { if (sequenceComplete) ReturnToMenu(); }
        public void Quit() { if (sequenceComplete) Application.Quit(); }

        public void ReturnToMenu()
        {
            if (!sequenceComplete || returning) return;
            returning = true;
            GameManager.Instance?.ReturnToMenu();
        }
    }
}
