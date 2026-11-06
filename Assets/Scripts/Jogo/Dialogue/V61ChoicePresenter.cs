using System;
using MementoMori.Core;
using MementoMori.World;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MementoMori.Dialogue
{
    /// <summary>Reusable, data-driven choice panel. The choice and its effects are committed only once.</summary>
    public sealed class V61ChoicePresenter : MonoBehaviour
    {
        private static V61ChoicePresenter active;
        private V61EventCatalog.Choice choice;
        private Canvas canvas;
        private Action onClosed;
        public static bool IsOpen => active != null;

        public static bool Show(string choiceId, Action completed = null)
        {
            var state = GameState.Instance;
            if (active != null || state == null || state.GetCounter("choice." + choiceId) != 0
                || DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return false;
            V61EventCatalog.Choice found = null;
            foreach (var candidate in V61EventCatalog.Choices)
                if (candidate.id == choiceId) { found = candidate; break; }
            if (found == null || found.options == null || found.options.Length < 2) return false;
            var host = new GameObject("V61Choice_" + choiceId);
            active = host.AddComponent<V61ChoicePresenter>();
            active.choice = found;
            active.onClosed = completed;
            active.Build();
            InputGate.Instance?.Block("V61Choice");
            return true;
        }

        private void Build()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("V61ChoiceEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            var root = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1200;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            var panel = Image("Panel", root.transform, null, new Color(.11f, .065f, .17f, .98f),
                new Vector2(.31f, .35f), new Vector2(.69f, .65f));
            panel.rectTransform.offsetMin = panel.rectTransform.offsetMax = Vector2.zero;
            Border(panel.transform, "UpperBorder", new Vector2(.03f, .965f), new Vector2(.97f, .975f));
            Border(panel.transform, "LowerBorder", new Vector2(.03f, .025f), new Vector2(.97f, .035f));
            Border(panel.transform, "LeftBorder", new Vector2(.02f, .04f), new Vector2(.025f, .96f));
            Border(panel.transform, "RightBorder", new Vector2(.975f, .04f), new Vector2(.98f, .96f));
            var prompt = Text("Prompt", panel.transform, choice.prompt, 30,
                new Vector2(.07f, .70f), new Vector2(.93f, .94f));
            prompt.fontStyle = FontStyles.SmallCaps;
            for (var i = 0; i < choice.options.Length; i++)
            {
                var index = i;
                var height = .56f / choice.options.Length;
                var top = .65f - i * height;
                var min = new Vector2(.09f, top - height + .02f);
                var max = new Vector2(.91f, top);
                var row = Image("Option_" + (i + 1), panel.transform, null,
                    new Color(.20f, .12f, .29f, 1f), min, max);
                row.rectTransform.offsetMin = row.rectTransform.offsetMax = Vector2.zero;
                Border(row.transform, "TopLine", new Vector2(0, .97f), new Vector2(1, 1));
                Border(row.transform, "BottomLine", new Vector2(0, 0), new Vector2(1, .03f));
                var button = row.gameObject.AddComponent<Button>();
                button.colors = new ColorBlock
                {
                    normalColor = Color.white,
                    highlightedColor = new Color(1.25f, 1.15f, 1.35f),
                    pressedColor = new Color(.8f, .7f, .95f),
                    selectedColor = new Color(1.25f, 1.15f, 1.35f),
                    disabledColor = new Color(.5f, .5f, .5f),
                    colorMultiplier = 1f,
                    fadeDuration = .12f
                };
                button.onClick.AddListener(() => Choose(index));
                Text("Label", row.transform, (i + 1) + "  " + choice.options[i].text, 25,
                    new Vector2(.04f, .05f), new Vector2(.96f, .95f));
            }
        }

        private void Update()
        {
            for (var i = 0; i < choice.options.Length && i < 9; i++)
                if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i))) { Choose(i); return; }
        }

        private void Choose(int index)
        {
            if (index < 0 || index >= choice.options.Length) return;
            if (!V61EventCatalog.TryApplyChoice(GameState.Instance, choice.id, choice.options[index].id, out var reply)) return;
            GameState.Instance?.SaveCheckpoint();
            InputGate.Instance?.Release("V61Choice");
            active = null;
            var afterReply = onClosed;
            if (!string.IsNullOrWhiteSpace(reply) && DialogueManager.Instance != null)
            {
                var data = ScriptableObject.CreateInstance<DialogueData>();
                data.Configure("reply." + choice.id, new[] { new DialogueLine { Speaker = "Melantha", Text = reply, LockMovement = true } });
                var manager = DialogueManager.Instance;
                void Clear() { manager.OnDialogueCompleted -= Clear; Destroy(data); afterReply?.Invoke(); }
                if (manager.TryStartDialogue(data)) manager.OnDialogueCompleted += Clear;
                else { Destroy(data); afterReply?.Invoke(); }
            }
            else afterReply?.Invoke();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (active == this) active = null;
            InputGate.Instance?.Release("V61Choice");
        }

        private static Image Image(string name, Transform parent, Sprite sprite, Color color, Vector2 min, Vector2 max)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(parent, false);
            image.rectTransform.anchorMin = min; image.rectTransform.anchorMax = max;
            image.sprite = sprite; image.color = color;
            return image;
        }

        private static void Border(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var line = Image(name, parent, null, new Color(.69f, .50f, .69f, .85f), min, max);
            line.rectTransform.offsetMin = line.rectTransform.offsetMax = Vector2.zero;
            line.raycastTarget = false;
        }

        private static TMP_Text Text(string name, Transform parent, string content, float size, Vector2 min, Vector2 max)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(parent, false);
            label.rectTransform.anchorMin = min; label.rectTransform.anchorMax = max;
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
            label.text = content;
            var fontHost = Resources.Load<GameObject>("GrimoireRuntimeHost")?.GetComponent<GrimoireScreen>();
            label.font = fontHost?.BodyFont ?? TMP_Settings.defaultFontAsset;
            label.fontSize = size; label.color = new Color(.96f, .86f, .96f);
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true; label.fontSizeMin = size - 3f; label.fontSizeMax = size;
            label.raycastTarget = false;
            return label;
        }
    }
}
