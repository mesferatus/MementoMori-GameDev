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
            // Same family as the pause, map and puzzle panels: soft veil, framed panel, spaced prompt and divider,
            // menu-frame buttons with hover feedback. It used to be a flat box with flat bars.
            MementoMori.UI.MenuPanelStyle.Veil(root.transform, .55f).GetComponent<Image>().raycastTarget = false;
            var height = 270f + choice.options.Length * 100f;
            var panel = MementoMori.UI.MenuPanelStyle.Panel(root.transform, new Vector2(1000f, height));
            var prompt = MementoMori.UI.MenuPanelStyle.Title(panel, Loc.T(choice.prompt), MementoMori.UI.MenuPanelStyle.HeadingFont, 46f, 32f);
            prompt.characterSpacing = 3f;
            prompt.enableAutoSizing = true; prompt.fontSizeMin = 26f; prompt.fontSizeMax = 38f;
            for (var i = 0; i < choice.options.Length; i++)
            {
                var index = i;
                var go = new GameObject("Option_" + (i + 1), typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(panel, false);
                var rect = (RectTransform)go.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
                rect.sizeDelta = new Vector2(780f, 84f);
                rect.anchoredPosition = new Vector2(0f, -180f - i * 100f);
                var button = go.GetComponent<Button>();
                MementoMori.UI.MenuButtonStyle.Apply(button, null, .6f);
                button.onClick.AddListener(() => Choose(index));
                Text("Label", go.transform, (i + 1) + "  ·  " + Loc.T(choice.options[i].text), 31,
                    new Vector2(.06f, .08f), new Vector2(.94f, .92f));
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
            label.font = MementoMori.UI.MenuPanelStyle.BodyFont;
            label.fontSize = size; label.color = new Color(.96f, .86f, .96f);
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true; label.fontSizeMin = size - 3f; label.fontSizeMax = size;
            label.raycastTarget = false;
            return label;
        }
    }
}
