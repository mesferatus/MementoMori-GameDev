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
        private UnityEngine.UI.Image hintPanel;
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
            var panel = new GameObject("Hint", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            panel.transform.SetParent(root.transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(.25f,.075f);
            rect.anchorMax = new Vector2(.75f,.135f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            hintRect = rect;
            hintPanel = panel.GetComponent<UnityEngine.UI.Image>();
            hintPanel.color = new Color(.075f,.045f,.115f,.94f);
            hintPanel.raycastTarget = false;
            var outline = panel.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(.64f,.47f,.76f,.9f);
            outline.effectDistance = new Vector2(2f, -2f);
            label = new GameObject("HintText", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(panel.transform, false);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(20,5);
            label.rectTransform.offsetMax = new Vector2(-20,-5);
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = 27;
            label.enableAutoSizing = true;
            label.fontSizeMin = 22;
            label.fontSizeMax = 27;
            label.color = new Color(.95f,.9f,.8f);
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

        // While a dialogue is open the hint becomes the controls line inside the dialogue box;
        // drawn under it (order 15 < 80) it was hidden behind the box and showed through faintly.
        private void PlaceHint(bool dialogue)
        {
            if (hintRect == null || dialogue == insideDialogue) return;
            insideDialogue = dialogue;
            overlayCanvas.sortingOrder = dialogue ? 85 : 15;
            hintRect.anchorMin = dialogue ? new Vector2(.3f, .112f) : new Vector2(.25f, .075f);
            hintRect.anchorMax = dialogue ? new Vector2(.74f, .148f) : new Vector2(.75f, .135f);
            if (hintPanel != null) hintPanel.enabled = !dialogue;
            if (label != null) label.alpha = dialogue ? .72f : 1f;
        }
    }
}
