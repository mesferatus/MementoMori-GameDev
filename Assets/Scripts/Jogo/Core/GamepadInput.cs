using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MementoMori.Core
{
    /// <summary>
    /// Controller layer on top of the keyboard bindings: left stick / d-pad move, A interacts and confirms,
    /// B goes back or skips, Y opens the grimoire, Start pauses, Select (View) opens the map. Menus get a
    /// selected button as soon as the pad is touched, so uGUI navigation works without a mouse.
    /// </summary>
    public static class GamepadInput
    {
        const float Deadzone = .5f;
        static int lastPadFrame = -1000;
        static Vector3 lastMouse;

        /// <summary>True while the controller is the device the player used last (prompts show pad glyphs).</summary>
        public static bool UsingGamepad { get; private set; }

        static Gamepad Pad => Gamepad.current;

        public static bool Down(GameAction action)
        {
            var pad = Pad;
            if (pad == null) return false;
            var pressed = action switch
            {
                GameAction.Interact => pad.buttonSouth.wasPressedThisFrame,
                GameAction.Grimoire => pad.buttonNorth.wasPressedThisFrame,
                GameAction.Pause => pad.startButton.wasPressedThisFrame,
                GameAction.MoveLeft => pad.dpad.left.wasPressedThisFrame,
                GameAction.MoveRight => pad.dpad.right.wasPressedThisFrame,
                GameAction.MoveUp => pad.dpad.up.wasPressedThisFrame,
                GameAction.MoveDown => pad.dpad.down.wasPressedThisFrame,
                _ => false
            };
            if (pressed) MarkPad();
            return pressed;
        }

        public static bool Held(GameAction action)
        {
            var move = Movement();
            return action switch
            {
                GameAction.MoveLeft => move.x < 0f,
                GameAction.MoveRight => move.x > 0f,
                GameAction.MoveUp => move.y > 0f,
                GameAction.MoveDown => move.y < 0f,
                GameAction.Interact => Pad != null && Pad.buttonSouth.isPressed,
                _ => false
            };
        }

        /// <summary>B: back out of a panel, skip a cutscene, close the grimoire or the map.</summary>
        public static bool CancelDown() { var pad = Pad; var d = pad != null && pad.buttonEast.wasPressedThisFrame; if (d) MarkPad(); return d; }
        public static bool ConfirmDown() => Down(GameAction.Interact);
        public static bool MapDown() { var pad = Pad; var d = pad != null && pad.selectButton.wasPressedThisFrame; if (d) MarkPad(); return d; }
        public static bool HistoryDown() { var pad = Pad; var d = pad != null && pad.leftShoulder.wasPressedThisFrame; if (d) MarkPad(); return d; }
        public static bool FastForwardDown() { var pad = Pad; var d = pad != null && pad.rightShoulder.wasPressedThisFrame; if (d) MarkPad(); return d; }

        /// <summary>Cardinal movement from the left stick or the d-pad, matching the keyboard's four directions.</summary>
        public static Vector2 Movement()
        {
            var pad = Pad;
            if (pad == null) return Vector2.zero;
            var raw = pad.leftStick.ReadValue() + pad.dpad.ReadValue();
            if (raw.sqrMagnitude < Deadzone * Deadzone) return Vector2.zero;
            MarkPad();
            return Mathf.Abs(raw.x) > Mathf.Abs(raw.y) ? new Vector2(Mathf.Sign(raw.x), 0f) : new Vector2(0f, Mathf.Sign(raw.y));
        }

        static void MarkPad() { lastPadFrame = Time.frameCount; UsingGamepad = true; }

        static float rumbleUntil = -1f;

        /// <summary>
        /// A short, soft pulse on the controller (portal, solved trial, falling asleep). Silent when the player
        /// is on keyboard, when vibration is off in the settings, or when no pad is connected.
        /// </summary>
        public static void Rumble(float low, float high, float seconds)
        {
            var pad = Pad;
            if (pad == null || !UsingGamepad) return;
            if (AccessibilitySettings.Instance != null && !AccessibilitySettings.Instance.Vibration) return;
            pad.SetMotorSpeeds(Mathf.Clamp01(low), Mathf.Clamp01(high));
            rumbleUntil = Time.unscaledTime + Mathf.Max(.02f, seconds);
        }

        public static void StopRumble()
        {
            rumbleUntil = -1f;
            Pad?.ResetHaptics();
        }

        internal static void TickRumble()
        {
            if (rumbleUntil > 0f && Time.unscaledTime >= rumbleUntil) StopRumble();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (Object.FindAnyObjectByType<GamepadMenuNavigator>() != null) return;
            var host = new GameObject("GamepadMenuNavigator");
            Object.DontDestroyOnLoad(host);
            host.AddComponent<GamepadMenuNavigator>();
        }

        internal static void Tick()
        {
            var mouse = Input.mousePosition;
            var mouseMoved = (mouse - lastMouse).sqrMagnitude > 16f; lastMouse = mouse;
            if ((Input.anyKeyDown || Input.GetMouseButtonDown(0) || mouseMoved) && lastPadFrame != Time.frameCount) UsingGamepad = false;
            var pad = Pad;
            if (pad != null && (pad.wasUpdatedThisFrame && (pad.leftStick.ReadValue().sqrMagnitude > .25f
                || pad.dpad.ReadValue().sqrMagnitude > .25f || pad.buttonSouth.wasPressedThisFrame
                || pad.buttonEast.wasPressedThisFrame || pad.startButton.wasPressedThisFrame))) MarkPad();
        }
    }

    /// <summary>Keeps a uGUI selection alive while the controller drives the menus.</summary>
    public sealed class GamepadMenuNavigator : MonoBehaviour
    {
        void OnEnable() => MementoMori.UI.RegionTitlePresenter.RegionEntered += OnRegionEntered;
        void OnDisable() { MementoMori.UI.RegionTitlePresenter.RegionEntered -= OnRegionEntered; GamepadInput.StopRumble(); }
        void OnApplicationFocus(bool focus) { if (!focus) GamepadInput.StopRumble(); }
        void OnApplicationQuit() => GamepadInput.StopRumble();
        static void OnRegionEntered(string id, string title) => GamepadInput.Rumble(.12f, 0f, .12f);

        void Update()
        {
            GamepadInput.TickRumble();
            GamepadInput.Tick();
            if (!GamepadInput.UsingGamepad) return;
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var menuScene = scene == "MainMenu" || scene == "FinalBeta";
            var events = EventSystem.current;
            // During free exploration A must only interact, never click a stray HUD button.
            if (!menuScene && InputGate.Instance != null && !InputGate.Instance.IsBlocked)
            {
                if (events != null && events.currentSelectedGameObject != null) events.SetSelectedGameObject(null);
                return;
            }
            if (events == null)
            {
                var host = new GameObject("GamepadEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                events = host.GetComponent<EventSystem>();
            }
            var selected = events.currentSelectedGameObject;
            if (selected != null && selected.activeInHierarchy && selected.GetComponent<Selectable>() is Selectable s && s.IsInteractable())
            {
                // The legacy UI module reads the stick; the d-pad is routed here.
                var pad = Gamepad.current;
                if (pad == null) return;
                Selectable next = null;
                if (pad.dpad.up.wasPressedThisFrame) next = s.FindSelectableOnUp();
                else if (pad.dpad.down.wasPressedThisFrame) next = s.FindSelectableOnDown();
                else if (pad.dpad.left.wasPressedThisFrame) next = s.FindSelectableOnLeft();
                else if (pad.dpad.right.wasPressedThisFrame) next = s.FindSelectableOnRight();
                if (next != null) events.SetSelectedGameObject(next.gameObject);
                return;
            }
            // Pick the top-most visible, interactable button (pause, settings, choices, menus).
            Selectable best = null; var bestOrder = int.MinValue;
            foreach (var candidate in Selectable.allSelectablesArray)
            {
                if (candidate == null || !candidate.IsInteractable() || !candidate.gameObject.activeInHierarchy) continue;
                var canvas = candidate.GetComponentInParent<Canvas>();
                if (canvas == null || !canvas.enabled) continue;
                var group = candidate.GetComponentInParent<CanvasGroup>();
                if (group != null && (group.alpha <= .01f || !group.interactable)) continue;
                // Buttons stacked in a menu list (Continue, choices...) win over loose icons such as the settings cog.
                var listed = candidate.GetComponentInParent<LayoutGroup>() != null ? 1 : 0;
                var order = canvas.rootCanvas.sortingOrder * 100000 + listed * 50000 + (int)candidate.transform.position.y;
                if (best == null || order > bestOrder) { best = candidate; bestOrder = order; }
            }
            if (best != null) events.SetSelectedGameObject(best.gameObject);
        }
    }
}
