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
            var shade = Image("Shade", root.transform, null, new Color(.015f, .008f, .03f, .78f),
                Vector2.zero, Vector2.one);
            shade.rectTransform.offsetMin = shade.rectTransform.offsetMax = Vector2.zero;
            var frame = Image("Frame", root.transform, Resources.Load<Sprite>("SettingsFrameV1"), Color.white,
                new Vector2(.23f, .24f), new Vector2(.77f, .76f));
            frame.rectTransform.offsetMin = frame.rectTransform.offsetMax = Vector2.zero;
            var paper = Image("Paper", frame.transform, Resources.Load<Sprite>("SettingsPurplePaperV1"), Color.white,
                new Vector2(.055f, .10f), new Vector2(.945f, .88f));
            paper.rectTransform.offsetMin = paper.rectTransform.offsetMax = Vector2.zero;
            Text("Heading", frame.transform, "ESCOLHA", 34, new Vector2(.14f, .78f), new Vector2(.86f, .88f));
            Text("Prompt", paper.transform, choice.prompt, 31, new Vector2(.08f, .68f), new Vector2(.92f, .90f));
            for (var i = 0; i < choice.options.Length; i++)
            {
                var index = i;
                var top = .66f - i * .27f;
                var min = new Vector2(.10f, top - .22f);
                var max = new Vector2(.90f, top);
                var fill = Image("OptionFill_" + (i + 1), paper.transform, null,
                    new Color(.09f, .045f, .16f, .86f), min, max);
                fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
                var row = Image("Option_" + (i + 1), paper.transform, Resources.Load<Sprite>("SettingsTabFrameV1"),
                    Color.white, min, max);
                row.rectTransform.offsetMin = row.rectTransform.offsetMax = Vector2.zero;
                row.type = UnityEngine.UI.Image.Type.Sliced;
                var button = row.gameObject.AddComponent<Button>();
                button.onClick.AddListener(() => Choose(index));
                Text("Label", row.transform, (i + 1) + "  " + choice.options[i].text, 27,
                    new Vector2(.07f, .12f), new Vector2(.93f, .88f));
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

        private static TMP_Text Text(string name, Transform parent, string content, float size, Vector2 min, Vector2 max)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(parent, false);
            label.rectTransform.anchorMin = min; label.rectTransform.anchorMax = max;
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
            var fontHost = Resources.Load<GameObject>("GrimoireRuntimeHost")?.GetComponent<GrimoireScreen>();
            label.text = content; label.font = name == "Heading"
                ? fontHost?.HeadingFont ?? TMP_Settings.defaultFontAsset
                : fontHost?.BodyFont ?? TMP_Settings.defaultFontAsset;
            label.fontSize = size; label.color = new Color(.95f, .88f, .98f);
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true; label.fontSizeMin = size * .70f; label.fontSizeMax = size;
            label.raycastTarget = false;
            return label;
        }
    }
}
