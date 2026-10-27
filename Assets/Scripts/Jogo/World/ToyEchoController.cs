using MementoMori.Dialogue;
using MementoMori.Interaction;
using MementoMori.Core;
using UnityEngine;

namespace MementoMori.World
{
    public sealed class ToyEchoController : MonoBehaviour, IInteractable
    {
        private DialogueData dialogue;
        private GameObject openingTarget;
        private bool touched;

        public string InteractionVerb => touched ? "Observar" : "Tocar";
        public int InteractionPriority => 12;
        public bool CanInteract(InteractionContext context) => !touched;

        public void Configure(DialogueData data) => dialogue = data;
        public void Configure(DialogueData data, GameObject target) { dialogue = data; openingTarget = target; }

        public void Interact(InteractionContext context)
        {
            if (touched) return;
            touched = true;
            GameState.Instance?.SetFlag(StoryFlag.ToyEchoTouched);
            DialogueManager.Instance?.StartDialogue(dialogue);
            if (openingTarget != null) openingTarget.SetActive(false);
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.enabled = false;
        }
    }
}
