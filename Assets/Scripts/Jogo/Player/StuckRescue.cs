using MementoMori.Core;
using UnityEngine;

namespace MementoMori.Player
{
    /// <summary>
    /// Safety net against being wedged inside a solid outline (a dais contour, a wall corner): when Melantha
    /// is pushing to move but has not moved for a second while overlapping something solid, she is placed on
    /// the nearest free floor point. Never runs in cutscenes, menus or while swimming.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public sealed class StuckRescue : MonoBehaviour
    {
        PlayerController player; Rigidbody2D body; CircleCollider2D feet;
        Vector2 last; float stillFor;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Hook()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= Attach; UnityEngine.SceneManagement.SceneManager.sceneLoaded += Attach;
            Attach(default, default);
        }
        static void Attach(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m)
        {
            foreach (var p in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
                if (p.GetComponent<StuckRescue>() == null) p.gameObject.AddComponent<StuckRescue>();
        }

        void Start()
        {
            player = GetComponent<PlayerController>(); body = GetComponent<Rigidbody2D>();
            foreach (var c in GetComponents<CircleCollider2D>()) if (!c.isTrigger) feet = c;
        }

        bool Blocked(Vector2 at)
        {
            var radius = feet != null ? feet.radius * Mathf.Abs(transform.lossyScale.x) : .18f;
            var offset = feet != null ? feet.offset : new Vector2(0f, -.22f);
            foreach (var hit in Physics2D.OverlapCircleAll(at + offset, radius * .95f))
                if (hit != null && !hit.isTrigger && hit.attachedRigidbody != body && !hit.transform.IsChildOf(transform)) return true;
            return false;
        }

        void FixedUpdate()
        {
            if (player == null || body == null) return;
            var gate = InputGate.Instance != null && InputGate.Instance.IsBlocked;
            var wants = InputBindings.Movement().sqrMagnitude > .01f || player.HasAutomationInput;
            Vector2 at = body.position;
            if (gate || !wants || player.IsSwimming || (at - last).sqrMagnitude > .0004f) { stillFor = 0f; last = at; return; }
            stillFor += Time.fixedDeltaTime;
            if (stillFor < 1f || !Blocked(at)) return;
            // Spiral out for the nearest spot where her feet are clear.
            for (var r = .25f; r <= 3f; r += .25f)
            for (var a = 0; a < 16; a++)
            {
                var p = at + new Vector2(Mathf.Cos(a * Mathf.PI / 8f), Mathf.Sin(a * Mathf.PI / 8f)) * r;
                if (Blocked(p)) continue;
                body.position = p; transform.position = new Vector3(p.x, p.y, transform.position.z);
                body.linearVelocity = Vector2.zero; stillFor = 0f; last = p;
                return;
            }
        }
    }
}
