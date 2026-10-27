using System;
using UnityEngine;
using UnityEngine.Events;

namespace MementoMori.World
{
    public enum DoorState { Locked, Opening, Open }

    public sealed class DoorController : MonoBehaviour
    {
        [SerializeField] private Collider2D blockingCollider;
        [SerializeField] private Animator animator;
        [SerializeField] private UnityEvent onOpened;
        public DoorState State { get; private set; } = DoorState.Locked;
        public event Action OnOpened;

        public void Configure(Collider2D blocker)
        {
            blockingCollider = blocker;
            if (State == DoorState.Open && blockingCollider != null)
                blockingCollider.enabled = false;
        }

        public void Open()
        {
            if (State != DoorState.Locked)
                return;
            State = DoorState.Opening;
            if (animator != null)
                animator.SetTrigger("Open");
            if (blockingCollider != null)
                blockingCollider.enabled = false;
            State = DoorState.Open;
            onOpened?.Invoke();
            OnOpened?.Invoke();
        }
    }
}
