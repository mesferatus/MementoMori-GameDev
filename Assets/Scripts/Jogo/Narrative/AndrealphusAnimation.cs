using MementoMori.Dialogue;
using UnityEngine;

namespace MementoMori.Narrative
{
    [RequireComponent(typeof(Animator))]
    public sealed class AndrealphusAnimation : MonoBehaviour
    {
        private Animator animator;
        private bool talking;
        private void Awake() => animator = GetComponent<Animator>();
        private void Update()
        {
            if (animator.runtimeAnimatorController == null) return;
            var speaking = DialogueManager.Instance != null
                && DialogueManager.Instance.CurrentSpeaker == "Andrealphus";
            if (speaking == talking) return;
            talking = speaking;
            animator.Play(talking ? "andrealphus_speak" : "andrealphus_idle_float", 0, 0);
        }
    }
}
