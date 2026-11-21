using MementoMori.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MementoMori.UI
{
    /// <summary>
    /// On-screen controls for phones: a joystick on the left, Interagir / Grimório on the right and
    /// Pausa at the top. Feeds the same GameActions as keyboard and controller (see InputBindings).
    /// Only created on touch devices, and only in the playable scenes.
    /// </summary>
    public sealed class TouchControls : MonoBehaviour
    {
        static TouchControls instance;
        static readonly int[] pendingFrame = new int[7];
        static readonly int[] activeFrame = { -9, -9, -9, -9, -9, -9, -9 };
        static readonly bool[] held = new bool[7];
        static Vector2 stick;

        public static bool Enabled => instance != null && instance.root != null && instance.root.activeSelf;
        public static Vector2 Movement => Enabled ? stick : Vector2.zero;
        public static bool Down(GameAction action) => Enabled && activeFrame[(int)action] == Time.frameCount;
        public static bool Held(GameAction action) => Enabled && held[(int)action];

        static readonly string[] Playable = { "Quarto", "Labirinto", "DominioLua", "FinalBeta" };
        GameObject root;
        RectTransform knob, pad;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isMobilePlatform || instance != null) return;
            var go = new GameObject("TouchControls");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<TouchControls>();
        }

        void Awake()
        {
            for (var i = 0; i < pendingFrame.Length; i++) pendingFrame[i] = -9;
            Build();
            SceneManager.activeSceneChanged += (_, scene) => Refresh(scene.name);
            Refresh(SceneManager.GetActiveScene().name);
        }

        void Refresh(string scene)
        {
            if (root != null) root.SetActive(System.Array.IndexOf(Playable, scene) >= 0);
            stick = Vector2.zero;
            for (var i = 0; i < held.Length; i++) held[i] = false;
        }

        void LateUpdate()
        {
            // A tap becomes visible to every reader during exactly one whole frame.
            for (var i = 0; i < pendingFrame.Length; i++)
                if (pendingFrame[i] >= 0) { activeFrame[i] = Time.frameCount + 1; pendingFrame[i] = -9; }
            if (EventSystem.current == null) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        void Build()
        {
            root = new GameObject("Toque", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 600; // above the HUD, below grimoire, map and dialogue choices
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;

            // Joystick: a gothic ring with a lilac knob, anchored bottom-left.
            pad = Ring("Joystick", new Vector2(0f, 0f), new Vector2(250f, 230f), 300f, .35f);
            var knobImage = MenuPanelStyle.NewImage("Botao", pad, MenuPanelStyle.Diamond(), MenuPanelStyle.Lilac);
            knob = knobImage;
            knob.anchorMin = knob.anchorMax = new Vector2(.5f, .5f);
            knob.sizeDelta = new Vector2(120f, 120f);
            pad.gameObject.AddComponent<StickArea>().owner = this;

            Button("Interagir", GameAction.Interact, new Vector2(1f, 0f), new Vector2(-230f, 220f), 190f);
            Button("Grimório", GameAction.Grimoire, new Vector2(1f, 0f), new Vector2(-420f, 120f), 140f);
            Button("Pausa", GameAction.Pause, new Vector2(1f, 1f), new Vector2(-110f, -110f), 120f);
        }

        RectTransform Ring(string name, Vector2 anchor, Vector2 position, float size, float alpha)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(root.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(size, size);
            var image = go.GetComponent<Image>();
            image.sprite = MenuPanelStyle.GothicFill();
            image.type = Image.Type.Sliced;
            image.color = new Color(.12f, .08f, .18f, alpha + .3f);
            var frame = MenuPanelStyle.NewImage("Moldura", rect, null, MenuPanelStyle.LilacSoft);
            MenuPanelStyle.Stretch(frame);
            MenuPanelStyle.FrameOnly(frame);
            return rect;
        }

        void Button(string label, GameAction action, Vector2 anchor, Vector2 position, float size)
        {
            var rect = Ring(label, anchor, position, size, .45f);
            var text = MenuPanelStyle.NewText("Texto", rect, MenuPanelStyle.HeadingFont, Loc.T(label), size * .16f, MenuPanelStyle.TitleColor);
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            MenuPanelStyle.Stretch(text.rectTransform);
            var press = rect.gameObject.AddComponent<PressArea>();
            press.action = action;
        }

        internal void SetStick(Vector2 screenPoint, bool active)
        {
            if (!active) { stick = Vector2.zero; knob.anchoredPosition = Vector2.zero; return; }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(pad, screenPoint, null, out var local);
            var radius = pad.rect.width * .4f;
            var clamped = Vector2.ClampMagnitude(local, radius);
            knob.anchoredPosition = clamped;
            // Cardinal like the keyboard: the stronger axis wins, with a small dead zone.
            if (clamped.magnitude < radius * .25f) stick = Vector2.zero;
            else stick = Mathf.Abs(clamped.x) > Mathf.Abs(clamped.y) ? new Vector2(Mathf.Sign(clamped.x), 0f) : new Vector2(0f, Mathf.Sign(clamped.y));
        }

        sealed class StickArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
        {
            public TouchControls owner;
            public void OnPointerDown(PointerEventData e) => owner.SetStick(e.position, true);
            public void OnDrag(PointerEventData e) => owner.SetStick(e.position, true);
            public void OnPointerUp(PointerEventData e) => owner.SetStick(e.position, false);
        }

        sealed class PressArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
        {
            public GameAction action;
            Image image;
            void Awake() => image = GetComponent<Image>();
            public void OnPointerDown(PointerEventData e)
            {
                pendingFrame[(int)action] = Time.frameCount;
                held[(int)action] = true;
                if (image) image.color = new Color(.35f, .22f, .5f, .9f);
            }
            public void OnPointerUp(PointerEventData e)
            {
                held[(int)action] = false;
                if (image) image.color = new Color(.12f, .08f, .18f, .75f);
            }
            void OnDisable() => held[(int)action] = false;
        }
    }
}
