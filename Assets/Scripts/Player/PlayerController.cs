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

        private Rigidbody2D body;
        private Vector2 moveInput;
        private float nextFootstepAt;

#if UNITY_EDITOR
        private bool automationInputActive;
        public void SetAutomationMoveInput(Vector2 direction) { automationInputActive = true; moveInput = direction.normalized; }
        public void ClearAutomationMoveInput() { automationInputActive = false; moveInput = Vector2.zero; }
#endif

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.freezeRotation = true;
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (automationInputActive) return;
#endif
            if (InputGate.Instance != null && InputGate.Instance.IsBlocked)
                moveInput = Vector2.zero;
            else
                moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

            if (moveInput.sqrMagnitude > 0f && Time.time >= nextFootstepAt)
            {
                RuntimeAudio.PlayOneShot("17_footstep_stone", .2f);
                nextFootstepAt = Time.time + .42f;
            }

            if (animator == null)
                return;

            animator.SetFloat("Speed", moveInput.sqrMagnitude);
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
            if (moveInput.sqrMagnitude > 0f)
            {
                animator.SetFloat("LastMoveX", moveInput.x);
                animator.SetFloat("LastMoveY", moveInput.y);
            }
        }

        private void FixedUpdate() => body.MovePosition(body.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}
