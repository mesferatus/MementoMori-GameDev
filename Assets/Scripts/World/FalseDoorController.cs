using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.Interaction;
using UnityEngine;

namespace MementoMori.World
{
    /// <summary>Fair false-door loop: it returns the player to the courtyard and never removes progress.</summary>
    public sealed class FalseDoorController : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueData firstDialogue;
        [SerializeField] private DialogueData repeatDialogue;
        [SerializeField] private Vector2 returnPosition = new(-8f, -2.3f);
        private int attempts;
        private void Awake()
        {
            firstDialogue ??= Resources.Load<DialogueData>("Dialogue/DLG_L_FALSE_DOOR_01");
            repeatDialogue ??= Resources.Load<DialogueData>("Dialogue/DLG_L_FALSE_DOOR_02");
        }
        public string InteractionVerb => "Abrir porta ornamentada";
        public int InteractionPriority => 7;
        public bool CanInteract(InteractionContext context) => true;
        public void Configure(DialogueData first, DialogueData repeat, Vector2 courtyardPosition)
        {
            firstDialogue = first; repeatDialogue = repeat; returnPosition = courtyardPosition;
        }
        public void Interact(InteractionContext context)
        {
            attempts++;
            GameState.Instance?.SetFlag(StoryFlag.FalseDoorTriggered);
            GameState.Instance?.IncrementCounter("falseDoor.attempts");
            var dialogue = attempts switch
            {
                1 => firstDialogue,
                2 => repeatDialogue,
                _ => Resources.Load<DialogueData>("Dialogue/DLG_L_FALSE_DOOR_03") ?? repeatDialogue ?? firstDialogue
            };
            DialogueManager.Instance?.StartDialogue(dialogue);
            if (context.Interactor != null) context.Interactor.transform.position = returnPosition;
        }
    }
}
