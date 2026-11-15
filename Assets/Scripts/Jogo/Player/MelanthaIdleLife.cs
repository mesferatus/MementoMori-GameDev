using System;
using System.Linq;
using MementoMori.Core;
using MementoMori.Poe;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.Player
{
    // Cosmetic only: no input locks, companion movement, story flags or save writes.
    [DefaultExecutionOrder(200)]
    public sealed class MelanthaIdleLife : MonoBehaviour
    {
        private PlayerController player;
        private Animator animator;
        private SpriteRenderer visual;
        private Rigidbody2D body;
        private Vector3 originalScale;
        private PoeFollower companion;
        private Sprite[] petFrames;
        private float idleSince;
        private float petSince = -1f;
        private float nextPetAt;
        private float nextLookupAt;
        public bool IsPetting => petSince >= 0f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneLoaded += SceneLoaded;
            Attach();
        }

        private static void SceneLoaded(Scene scene, LoadSceneMode mode) => Attach();

        private static void Attach()
        {
            foreach (var p in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
                if (p.GetComponent<MelanthaIdleLife>() == null)
                    p.gameObject.AddComponent<MelanthaIdleLife>();
        }

        private void Start()
        {
            player = GetComponent<PlayerController>();
            body = GetComponent<Rigidbody2D>();
            animator = GetComponentInChildren<Animator>();
            visual = animator == null ? null : animator.GetComponent<SpriteRenderer>();
            if (visual != null) originalScale = visual.transform.localScale;
            var costume = gameObject.scene.name == "DominioLua" ? "Lunar" : "Normal";
            petFrames = Resources.LoadAll<Sprite>("IdleLife/MelanthaPet" + costume)
                .OrderBy(s => s.name, StringComparer.Ordinal).ToArray();
            idleSince = Time.time;
            nextPetAt = Time.time + 10f;
        }

        private void LateUpdate()
        {
            if (player == null || animator == null || visual == null) return;
            var moving = Mathf.Abs(animator.GetFloat("MoveX")) > .01f
                || Mathf.Abs(animator.GetFloat("MoveY")) > .01f
                || body != null && body.linearVelocity.sqrMagnitude > .0025f;
            var blocked = InputGate.Instance != null && InputGate.Instance.IsBlocked;
            if (moving || blocked || player.IsSwimming || !visual.enabled)
            {
                CancelPet();
                visual.transform.localScale = originalScale;
                idleSince = Time.time;
                return;
            }

            if (Time.time >= nextLookupAt)
            {
                nextLookupAt = Time.time + .5f;
                if (companion == null)
                    companion = FindFirstObjectByType<PoeFollower>();
            }

            if (IsPetting)
            {
                if (!CompanionInReach() || Time.time - petSince >= 2.1f)
                    CancelPet();
                else
                {
                    visual.transform.localScale = originalScale;
                    visual.flipX = companion.transform.position.x < transform.position.x;
                    var frame = Mathf.Min(petFrames.Length - 1,
                        Mathf.FloorToInt((Time.time - petSince) / .35f));
                    visual.sprite = petFrames[frame];
                    return;
                }
            }

            // Less than one logical pixel at the shoulders; sprite pivot keeps the feet rooted.
            var breath = Mathf.Sin((Time.time - idleSince) * (Mathf.PI * 2f / 3.8f));
            visual.transform.localScale = Vector3.Scale(originalScale,
                new Vector3(1f - breath * .0015f, 1f + breath * .006f, 1f));

            if (petFrames.Length == 6 && Time.time >= nextPetAt
                && Time.time - idleSince >= 8f && CompanionInReach())
            {
                petSince = Time.time;
                visual.transform.localScale = originalScale;
                animator.SetFloat("LastMoveX", companion.transform.position.x < transform.position.x ? -1f : 1f);
                animator.SetFloat("LastMoveY", 0f);
            }
        }

        private static bool CanReach(Vector2 delta, PoeState state, bool revealed, bool visible)
        {
            return revealed && visible
                && (state == PoeState.Following || state == PoeState.Waiting)
                && Mathf.Abs(delta.y) <= .18f && Mathf.Abs(delta.x) >= .45f
                && Mathf.Abs(delta.x) <= .70f;
        }

        private bool CompanionInReach()
        {
            if (companion == null || companion.gameObject.scene != gameObject.scene) return false;
            var companionAnimator = companion.GetComponentInChildren<Animator>();
            var sr = companionAnimator == null ? null : companionAnimator.GetComponent<SpriteRenderer>();
            var visible = sr != null && sr.enabled && sr.gameObject.activeInHierarchy && sr.color.a > .1f;
            var delta = (Vector2)(companion.transform.position - transform.position);
            if (!CanReach(delta, companion.State, GameState.Instance != null && GameState.Instance.PoeRevealed, visible))
                return false;
            foreach (var hit in Physics2D.LinecastAll(transform.position, companion.transform.position))
            {
                if (hit.collider == null || hit.collider.isTrigger) continue;
                if (hit.transform.IsChildOf(transform) || hit.transform.IsChildOf(companion.transform)) continue;
                return false;
            }
            return true;
        }

        private void CancelPet()
        {
            if (!IsPetting) return;
            petSince = -1f;
            nextPetAt = Time.time + 24f;
            if (animator != null && animator.isActiveAndEnabled) animator.Update(0f);
        }

        private void OnDisable()
        {
            CancelPet();
            if (visual != null) visual.transform.localScale = originalScale;
        }
    }
}
