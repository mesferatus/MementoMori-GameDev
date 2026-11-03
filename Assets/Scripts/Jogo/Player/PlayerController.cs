using MementoMori.Core;
using UnityEngine;
using MementoMori.Audio;

namespace MementoMori.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 3.5f;
        [SerializeField] private Animator animator;
        [SerializeField] private RuntimeAnimatorController preferredController;
        [SerializeField] private RuntimeAnimatorController swimController;

        private Rigidbody2D body;
        private GardenWaterZone waterZone;
        private Vector2 moveInput;
        private float nextFootstepAt;
        private Vector2 lastDryPosition;
        private Vector3 baseVisualPosition;
        private SpriteRenderer splashVisual;
        private AudioSource splashAudio;
        private static AudioClip splashClip;
        private float splashStartedAt;
        private float nextSplashAt;
        private bool isSwimming;
        public bool IsSwimming => isSwimming;
        public Vector2 SafeSavePosition => isSwimming ? lastDryPosition : (Vector2)transform.position;

        private bool automationInputActive;
        public void SetAutomationMoveInput(Vector2 direction) { automationInputActive = true; moveInput = CardinalMovement.Snap(direction); }
        public void ClearAutomationMoveInput() { automationInputActive = false; moveInput = Vector2.zero; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.None;
            lastDryPosition = body.position;
            waterZone = GardenWaterZone.Ensure();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (gameObject.scene.name != "DominioLua")
            {
                preferredController = Resources.Load<RuntimeAnimatorController>("Costumes/MelanthaOriginal") ?? preferredController;
                swimController = null;
            }
            if (animator != null && preferredController != null && animator.runtimeAnimatorController != preferredController)
                animator.runtimeAnimatorController = preferredController;
            if (animator != null) baseVisualPosition = animator.transform.localPosition;
            var splashSprite = Resources.Load<Sprite>("SwimSplashV1");
            if (splashSprite != null)
            {
                var splashObject = new GameObject("SwimSplash", typeof(SpriteRenderer));
                splashObject.transform.SetParent(transform, false);
                splashVisual = splashObject.GetComponent<SpriteRenderer>();
                splashVisual.sprite = splashSprite;
                var character = animator == null ? null : animator.GetComponent<SpriteRenderer>();
                if (character != null)
                {
                    splashVisual.sortingLayerID = character.sortingLayerID;
                    splashVisual.sortingOrder = character.sortingOrder + 1;
                }
                splashVisual.enabled = false;
            }
            splashAudio = gameObject.AddComponent<AudioSource>();
            splashAudio.playOnAwake = false;
            splashAudio.spatialBlend = 0f;
        }

        private void Update()
        {
            if (waterZone == null) waterZone = GardenWaterZone.Ensure();
            var inWater = waterZone != null && GardenWaterZone.Contains(body.position);
            if (!inWater) lastDryPosition = body.position;
            if (inWater != isSwimming) StartSplash();
            isSwimming = inWater;
            if (animator != null)
            {
                var targetController = isSwimming && swimController != null ? swimController : preferredController;
                if (targetController != null && animator.runtimeAnimatorController != targetController)
                    animator.runtimeAnimatorController = targetController;
            }
            if (InputGate.Instance != null && InputGate.Instance.IsBlocked)
                moveInput = Vector2.zero;
            else if (!automationInputActive)
                moveInput = InputBindings.Movement();

            if (isSwimming && moveInput.sqrMagnitude > .01f && Time.time >= nextSplashAt)
                StartSplash();
            UpdateSplash();

            if (!isSwimming && moveInput.sqrMagnitude > 0f && Time.time >= nextFootstepAt)
            {
                RuntimeAudio.PlayOneShot("17_footstep_stone", .2f);
                nextFootstepAt = Time.time + .42f;
            }

            if (animator != null)
            {
                animator.speed = isSwimming ? .72f : 1f;
                animator.transform.localPosition = baseVisualPosition + (isSwimming && moveInput.sqrMagnitude > .01f
                    ? Vector3.up * (Mathf.Sin(Time.time * 7f) * .045f) : Vector3.zero);
                animator.SetFloat("Speed", moveInput.sqrMagnitude);
                animator.SetFloat("MoveX", moveInput.x);
                animator.SetFloat("MoveY", moveInput.y);
                if (moveInput.sqrMagnitude > 0f)
                {
                    animator.SetFloat("LastMoveX", moveInput.x);
                    animator.SetFloat("LastMoveY", moveInput.y);
                    var visual = animator.GetComponent<SpriteRenderer>();
                    if (visual != null)
                        visual.flipX = Mathf.Abs(moveInput.x) > .01f && moveInput.x < 0f;
                }
            }
        }

        private void FixedUpdate()
        {
            if (InputGate.Instance != null && InputGate.Instance.IsBlocked) moveInput = Vector2.zero;
            moveInput = CardinalMovement.Snap(moveInput);
            if (moveInput.sqrMagnitude < .001f)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }
            body.MovePosition(body.position + moveInput * moveSpeed
                * (isSwimming ? .65f : 1f) * Time.fixedDeltaTime);
        }

        private void OnDisable()
        {
            moveInput = Vector2.zero;
            automationInputActive = false;
            if (body != null) body.linearVelocity = Vector2.zero;
        }

        private void StartSplash()
        {
            if (splashVisual == null) return;
            splashStartedAt = Time.time;
            nextSplashAt = Time.time + .58f;
            splashVisual.enabled = true;
            if (splashAudio != null)
            {
                if (splashClip == null) splashClip = CreateSplashClip();
                var effects = AccessibilitySettings.Instance == null ? 1f : AccessibilitySettings.Instance.EffectsVolume;
                splashAudio.PlayOneShot(splashClip, .18f * effects);
            }
        }

        private void UpdateSplash()
        {
            if (splashVisual == null || !splashVisual.enabled) return;
            var progress = (Time.time - splashStartedAt) / .48f;
            if (progress >= 1f) { splashVisual.enabled = false; return; }
            splashVisual.color = new Color(1f, 1f, 1f, 1f - progress);
            splashVisual.transform.localScale = Vector3.one * Mathf.Lerp(.65f, 1f, progress);
        }

        private static AudioClip CreateSplashClip()
        {
            const int rate = 11025;
            const int count = 2205;
            var samples = new float[count];
            var seed = 10731u;
            var smooth = 0f;
            for (var i = 0; i < count; i++)
            {
                var t = i / (float)count;
                seed ^= seed << 13; seed ^= seed >> 17; seed ^= seed << 5;
                var noise = (seed / (float)uint.MaxValue) * 2f - 1f;
                smooth = Mathf.Lerp(smooth, noise, .22f);
                var envelope = (1f - t) * (1f - t);
                samples[i] = (smooth * .68f + Mathf.Sin(2f * Mathf.PI * (210f - 90f * t) * i / rate) * .32f) * envelope;
            }
            var clip = AudioClip.Create("SwimSplash", count, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
