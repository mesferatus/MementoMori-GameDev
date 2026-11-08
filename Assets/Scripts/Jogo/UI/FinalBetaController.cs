using System;
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

        private int entryIndex;
        private bool sequenceComplete;
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
                finalText.text = "Algumas coisas não terminam.\nElas apenas mudam de lugar.\n\nObrigado por carregar esta memória até aqui.";
                finalText.gameObject.SetActive(true);
            }
            if (finalTextTmp != null) finalTextTmp.text = "Algumas coisas não terminam.\nElas apenas mudam de lugar.\n\nObrigado por carregar esta memória até aqui.";
            if (returnButton != null) returnButton.interactable = true;
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

            var backdrop = NewLayer(root.transform, "Backdrop", null, new Color(.02f, .008f, .04f, 1f));
            backdrop.anchorMin = Vector2.zero; backdrop.anchorMax = Vector2.one;
            backdrop.offsetMin = backdrop.offsetMax = Vector2.zero;

            var image = NewLayer(root.transform, "CamilaIlustracao", art, new Color(1f, 1f, 1f, .78f));
            image.anchorMin = Vector2.zero; image.anchorMax = Vector2.one;
            image.offsetMin = image.offsetMax = Vector2.zero;
            image.GetComponent<Image>().preserveAspect = true;

            // Darker band where the closing message sits, so it stays legible over the art.
            var band = NewLayer(root.transform, "TextShade", null, new Color(.02f, .008f, .04f, .55f));
            band.anchorMin = new Vector2(.12f, .58f); band.anchorMax = new Vector2(.88f, .86f);
            band.offsetMin = band.offsetMax = Vector2.zero;
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
            if (ui.Find("Backdrop")?.GetComponent<Image>() is Image backdrop) backdrop.color = new Color(.02f, .008f, .04f, .62f);
            // The emblem sat over the illustration's face; the logo above already closes the card.
            ui.Find("CentralOrnament")?.gameObject.SetActive(false);
            foreach (var name in new[] { "CreditsButton", "ReturnButton", "QuitButton" })
                if (ui.Find(name)?.GetComponent<Button>() is Button button) MenuButtonStyle.Apply(button, new Vector2(380f, 84f), .6f);
            var credits = ui.Find("CreditsPanel") as RectTransform;
            if (credits == null || credits.Find("Painel") != null) return;
            var bodyFont = credits.Find("CreditsText")?.GetComponent<TMPro.TMP_Text>()?.font;
            var titleFont = ui.Find("CreditsButton")?.GetComponentInChildren<TMPro.TMP_Text>(true)?.font;
            if (credits.GetComponent<Image>() is Image panelImage) panelImage.color = Color.clear;
            credits.Find("CreditsText")?.gameObject.SetActive(false);
            credits.anchorMin = Vector2.zero; credits.anchorMax = Vector2.one;
            credits.offsetMin = credits.offsetMax = Vector2.zero;
            MenuPanelStyle.Veil(credits, .8f).SetAsFirstSibling();
            var panel = MenuPanelStyle.Panel(credits, new Vector2(1000f, 820f));
            panel.SetSiblingIndex(1);
            MenuPanelStyle.Title(panel, "CRÉDITOS", titleFont);
            MenuPanelStyle.CreditsBody(panel, bodyFont);
            if (credits.Find("CloseCredits") is RectTransform back)
            {
                back.SetParent(panel, false);
                back.anchorMin = back.anchorMax = back.pivot = new Vector2(.5f, 0f);
                back.anchoredPosition = new Vector2(0f, 46f);
                MenuButtonStyle.Apply(back.GetComponent<Button>(), new Vector2(380f, 84f), .6f);
            }
        }

        public void ShowCredits() { if (sequenceComplete && credits != null) credits.SetActive(true); }
        public void HideCredits() { if (credits != null) credits.SetActive(false); }
        public void Quit() { if (sequenceComplete) Application.Quit(); }

        public void ReturnToMenu()
        {
            if (!sequenceComplete) return;
            GameManager.Instance?.ReturnToMenu();
        }
    }
}
