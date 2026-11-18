using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.World;
using TMPro;
using UnityEngine;

namespace MementoMori.UI
{
    /// <summary>Optional, observation-driven room guidance. Never gates progression.</summary>
    public sealed class RoomTutorialController : MonoBehaviour
    {
        private Transform player;
        private Vector3 origin;
        private CanvasGroup group;
        private TMP_Text label;
        private Canvas overlayCanvas;
        private RectTransform hintRect;
        private RectTransform dialogueBox;
        public bool ShowsDialogueControls => insideDialogue && !string.IsNullOrEmpty(CurrentHint);
        private bool insideDialogue, wasDialogue;
        private int dialogueHintsShown;
        private const int DialogueHintLimit = 3;
        private bool fontMatched;
        private bool moved, inspected, readBook, paused;
        private float pauseHintTime;
        public string CurrentHint { get; private set; }

        private void Start()
        {
            var saved = GameState.Instance;
            if (saved != null)
            {
                moved = saved.GetCounter("tutorial.moved") > 0;
                inspected = saved.GetCounter("tutorial.inspected") > 0;
                readBook = saved.GetCounter("tutorial.grimoire") > 0;
                paused = saved.GetCounter("tutorial.paused") > 0;
                dialogueHintsShown = saved.GetCounter("tutorial.dialogueHints");
            }
            var controller = FindAnyObjectByType<MementoMori.Player.PlayerController>();
            if (controller != null) { player = controller.transform; origin = player.position; }
            var root = new GameObject("V6TutorialOverlay", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 15;
            overlayCanvas = canvas;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            group = root.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            var panel = new GameObject("Hint", typeof(RectTransform));
            panel.transform.SetParent(root.transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(.22f,.12f);
            rect.anchorMax = new Vector2(.78f,.2f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            hintRect = rect;
            label = new GameObject("HintText", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(panel.transform, false);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(20,5);
            label.rectTransform.offsetMax = new Vector2(-20,-5);
            label.font = MementoMori.UI.MenuPanelStyle.BodyFont;
            label.fontSize = 27;
            label.enableAutoSizing = true;
            label.fontSizeMin = 22;
            label.fontSizeMax = 27;
            label.color = MementoMori.UI.MenuPanelStyle.TitleColor;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
        }

        private void Update()
        {
            if (group == null) return;
            var dialogue = DialogueManager.Instance != null && DialogueManager.Instance.IsOpen;
            var blocked = InputGate.Instance != null && InputGate.Instance.IsBlocked;
            var state = GameState.Instance;
            if (!moved && player != null && !blocked && Vector3.Distance(origin,player.position) > .7f)
            { moved = true; state?.SetCounter("tutorial.moved", 1); }
            if (!inspected && moved && dialogue)
            { inspected = true; state?.SetCounter("tutorial.inspected", 1); }
            if (!readBook && GrimoireScreen.AnyOpen)
            { readBook = true; state?.SetCounter("tutorial.grimoire", 1); }
            if (!paused && Time.timeScale == 0f)
            { paused = true; state?.SetCounter("tutorial.paused", 1); }
            // The controls hint inside dialogues is taught a few times per save, then stays out of the way.
            if (dialogue && !wasDialogue && dialogueHintsShown <= DialogueHintLimit)
            { dialogueHintsShown++; state?.SetCounter("tutorial.dialogueHints", dialogueHintsShown); }
            wasDialogue = dialogue;
            if (dialogue && dialogueHintsShown > DialogueHintLimit)
                CurrentHint = string.Empty;
            else if (dialogue)
                CurrentHint = string.Format(Loc.T("{0} / Enter / Espaço — revelar o texto e avançar"), InputBindings.Get(GameAction.Interact));
            else if (!moved)
                CurrentHint = Loc.T("Use as teclas de movimento ou as setas — explore o quarto");
            else if (!inspected)
                CurrentHint = string.Format(Loc.T("Aproxime-se de um objeto e pressione {0} para examinar"), InputBindings.Get(GameAction.Interact));
            else if (!readBook && (state == null || !state.HasFlag(StoryFlag.RoomGrimoireRead)))
                CurrentHint = string.Format(Loc.T("O grimório guarda pistas. Aproxime-se e pressione {0} para ler"), InputBindings.Get(GameAction.Interact));
            else if (!paused && pauseHintTime < 7f)
            {
                CurrentHint = string.Format(Loc.T("{0} — pausar e consultar as opções"), InputBindings.Get(GameAction.Pause));
                if (!blocked) pauseHintTime += Time.unscaledDeltaTime;
            }
            else CurrentHint = string.Empty;
            PlaceHint(dialogue);
            var visible = !GrimoireScreen.AnyOpen && Time.timeScale > 0f && (dialogue || !blocked);
            MatchObjectiveFont();
            var target = visible && !string.IsNullOrEmpty(CurrentHint) ? 1f : 0f;
            group.alpha = Mathf.MoveTowards(group.alpha, target, Time.unscaledDeltaTime * 4f);
            if (!string.IsNullOrEmpty(CurrentHint)) label.text = CurrentHint;
        }

        // Same serif face as the objective panel, so tutorial and HUD read as one interface.
        private void MatchObjectiveFont()
        {
            if (fontMatched || label == null) return;
            foreach (var t in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
                if (t.name == "ObjectiveText_TMP" && t.font != null)
                { label.font = t.font; label.fontSizeMax = 25; label.fontSizeMin = 18; label.characterSpacing = 0f; fontMatched = true; return; }
        }

        // Dialogue controls occupy a separate footer below the dialogue content.
        private void PlaceHint(bool dialogue)
        {
            if (hintRect == null) return;
            insideDialogue = dialogue;
            overlayCanvas.sortingOrder = dialogue ? 85 : 15;
            if (dialogue)
            {
                if (dialogueBox == null)
                    foreach (var rect in Resources.FindObjectsOfTypeAll<RectTransform>())
                        if (rect.name == "DialogueBox" && rect.gameObject.scene.IsValid()) { dialogueBox = rect; break; }
                if (dialogueBox != null)
                {
                    var face = dialogueBox.Find("PortraitFrame");
                    var left = face != null && face.gameObject.activeSelf ? .23f : .05f;
                    hintRect.anchorMin = hintRect.anchorMax = dialogueBox.anchorMin;
                    hintRect.pivot = new Vector2(.5f, 0f);
                    hintRect.sizeDelta = new Vector2(dialogueBox.rect.width * (.95f - left) - 56f, 30f);
                    hintRect.anchoredPosition = dialogueBox.anchoredPosition
                        + new Vector2(dialogueBox.rect.width * ((left + .95f) * .5f - .5f), 10f);
                }
            }
            else
            {
                hintRect.anchorMin = new Vector2(.25f, .075f);
                hintRect.anchorMax = new Vector2(.75f, .135f);
                hintRect.pivot = new Vector2(.5f, .5f);
                hintRect.offsetMin = hintRect.offsetMax = Vector2.zero;
            }
            if (label != null) label.alpha = dialogue ? .72f : 1f;
        }
    }
}
