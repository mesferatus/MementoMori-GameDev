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
        private bool moved, inspected, readBook, paused;
        private float pauseHintTime;
        public string CurrentHint { get; private set; }

        private void Start()
        {
            var controller = FindAnyObjectByType<MementoMori.Player.PlayerController>();
            if (controller != null) { player = controller.transform; origin = player.position; }
            var root = new GameObject("V6TutorialOverlay", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 15;
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
            rect.anchorMin = new Vector2(.22f,.035f);
            rect.anchorMax = new Vector2(.78f,.105f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            panel.GetComponent<UnityEngine.UI.Image>().color = new Color(.055f,.035f,.085f,.94f);
            label = new GameObject("HintText", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(panel.transform, false);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(18,6);
            label.rectTransform.offsetMax = new Vector2(-18,-6);
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
            if (player != null && !blocked && Vector3.Distance(origin,player.position) > .7f) moved = true;
            if (moved && dialogue) inspected = true;
            if (GrimoireScreen.AnyOpen) readBook = true;
            if (Time.timeScale == 0f) paused = true;
            if (dialogue)
                CurrentHint = "E / Enter / Espaço — revelar o texto e avançar";
            else if (!moved)
                CurrentHint = "WASD ou setas — explore o quarto";
            else if (!inspected)
                CurrentHint = "Aproxime-se de um objeto e pressione E para examinar";
            else if (!readBook && (state == null || !state.HasFlag(StoryFlag.RoomGrimoireRead)))
                CurrentHint = "O grimório guarda pistas. Aproxime-se e pressione E para ler";
            else if (!paused && pauseHintTime < 7f)
            {
                CurrentHint = "Esc — pausar e consultar as opções";
                if (!blocked) pauseHintTime += Time.unscaledDeltaTime;
            }
            else CurrentHint = string.Empty;
            var visible = !GrimoireScreen.AnyOpen && Time.timeScale > 0f && (dialogue || !blocked);
            group.alpha = visible && !string.IsNullOrEmpty(CurrentHint) ? 1 : 0;
            label.text = CurrentHint;
        }
    }
}
