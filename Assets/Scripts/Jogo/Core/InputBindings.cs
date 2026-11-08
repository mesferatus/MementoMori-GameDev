using System;
using UnityEngine;

namespace MementoMori.Core
{
    public enum GameAction { MoveLeft, MoveRight, MoveUp, MoveDown, Interact, Grimoire, Pause }

    /// <summary>One persistent keyboard binding per gameplay action; rejects duplicate and reserved keys.</summary>
    public static class InputBindings
    {
        private static readonly KeyCode[] Defaults =
        {
            KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S,
            KeyCode.E, KeyCode.G, KeyCode.Escape
        };
        private static readonly KeyCode[] Keys = new KeyCode[Defaults.Length];
        private static bool loaded;
        private static readonly CardinalMovement keyboardMovement = new();

        public static KeyCode Get(GameAction action)
        {
            Load();
            return Keys[(int)action];
        }

        public static bool Down(GameAction action) => Input.GetKeyDown(Get(action));
        public static bool Held(GameAction action) => Input.GetKey(Get(action));

        public static Vector2 Movement()
        {
            var left = Held(GameAction.MoveLeft) || Input.GetKey(KeyCode.LeftArrow);
            var right = Held(GameAction.MoveRight) || Input.GetKey(KeyCode.RightArrow);
            var up = Held(GameAction.MoveUp) || Input.GetKey(KeyCode.UpArrow);
            var down = Held(GameAction.MoveDown) || Input.GetKey(KeyCode.DownArrow);
            var keyboard = keyboardMovement.Resolve(left, right, up, down);
            if (left || right || up || down) return keyboard;
            // Keep existing controller axes without letting the old WASD mapping bypass a rebound key.
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)
                || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) return Vector2.zero;
            foreach (var joystick in Input.GetJoystickNames())
                if (!string.IsNullOrEmpty(joystick))
                    return CardinalMovement.Snap(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
            return Vector2.zero;
        }

        public static bool TryBind(GameAction action, KeyCode key)
        {
            Load();
            if (!CanBind(action, key)) return false;
            Keys[(int)action] = key;
            PlayerPrefs.SetInt("MM.Input." + action, (int)key);
            PlayerPrefs.Save();
            return true;
        }

        public static bool CanBind(GameAction action, KeyCode key)
        {
            Load();
            if (!IsSupported(key) || key == KeyCode.Escape && action != GameAction.Pause
                || key == KeyCode.Return || key == KeyCode.Space && action != GameAction.Interact
                || key == KeyCode.LeftArrow || key == KeyCode.RightArrow
                || key == KeyCode.UpArrow || key == KeyCode.DownArrow) return false;
            for (var i = 0; i < Keys.Length; i++)
                if (i != (int)action && Keys[i] == key) return false;
            return true;
        }

        public static void RestoreDefaults()
        {
            loaded = true;
            Array.Copy(Defaults, Keys, Defaults.Length);
            for (var i = 0; i < Keys.Length; i++) PlayerPrefs.DeleteKey("MM.Input." + (GameAction)i);
            PlayerPrefs.Save();
        }

        public static bool IsSupported(KeyCode key) =>
            key >= KeyCode.A && key <= KeyCode.Z
            || key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9
            || key == KeyCode.Space || key == KeyCode.Return || key == KeyCode.Escape
            || key == KeyCode.LeftArrow || key == KeyCode.RightArrow
            || key == KeyCode.UpArrow || key == KeyCode.DownArrow;

        private static void Load()
        {
            if (loaded) return;
            loaded = true;
            Array.Copy(Defaults, Keys, Defaults.Length);
            for (var i = 0; i < Keys.Length; i++)
            {
                var key = (KeyCode)PlayerPrefs.GetInt("MM.Input." + (GameAction)i, (int)Defaults[i]);
                if (key == Defaults[i]) continue;
                if (IsSupported(key) && !(key == KeyCode.Escape && i != (int)GameAction.Pause))
                {
                    var available = true;
                    for (var j = 0; j < Keys.Length; j++) if (i != j && Keys[j] == key) available = false;
                    if (available) Keys[i] = key;
                }
            }
        }
    }

    /// <summary>Most recently pressed held direction wins. Same-frame ties use left, right, up, down order.</summary>
    public sealed class CardinalMovement
    {
        private readonly bool[] previous = new bool[4];
        private readonly long[] order = new long[4];
        private long serial;
        private static readonly Vector2[] Directions = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

        public Vector2 Resolve(bool left, bool right, bool up, bool down)
        {
            var held = new[] { left, right, up, down };
            var winner = -1;
            for (var i = 0; i < held.Length; i++)
            {
                if (held[i] && !previous[i]) order[i] = ++serial;
                previous[i] = held[i];
                if (held[i] && (winner < 0 || order[i] > order[winner])) winner = i;
            }
            return winner < 0 ? Vector2.zero : Directions[winner];
        }

        public static Vector2 Snap(Vector2 value)
        {
            if (value.sqrMagnitude < .01f) return Vector2.zero;
            return Mathf.Abs(value.x) > Mathf.Abs(value.y)
                ? new Vector2(Mathf.Sign(value.x), 0f) : new Vector2(0f, Mathf.Sign(value.y));
        }
    }
}
