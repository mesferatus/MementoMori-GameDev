using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.Interaction;
using UnityEngine;

namespace MementoMori.World
{
    /// <summary>Optional well: each interaction advances one altered memory without blocking progress.</summary>
    public sealed class VoiceWellController : MonoBehaviour, IInteractable
    {
        [SerializeField] DialogueData[] voices;
        int heard;
        public string InteractionVerb => "Escutar o poço";
        public int InteractionPriority => 4;
        public void Configure(params DialogueData[] lines) => voices = lines;
        public bool CanInteract(InteractionContext context) => voices != null && voices.Length > 0;
        public void Interact(InteractionContext context)
        {
            if (!CanInteract(context)) return;
            var index = Mathf.Min(heard, voices.Length - 1);
            heard++;
            DialogueManager.Instance?.StartDialogue(voices[index]);
            if (heard >= voices.Length)
                GameState.Instance?.SetFlag(StoryFlag.VoiceWellComplete);
        }
    }
}
