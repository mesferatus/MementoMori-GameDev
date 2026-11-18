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
            if (firstDialogue == null) firstDialogue = Resources.Load<DialogueData>("Dialogue/DLG_L_FALSE_DOOR_01");
            if (repeatDialogue == null) repeatDialogue = Resources.Load<DialogueData>("Dialogue/DLG_L_FALSE_DOOR_02");
        }
        private void Start() => attempts = GameState.Instance?.GetCounter("falseDoor.attempts") ?? 0;
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
            // Send Melantha herself back (the interactor is only her reach trigger, a child object).
            var player = context.Interactor != null ? context.Interactor.GetComponentInParent<MementoMori.Player.PlayerController>() : null;
            if (player != null)
            {
                // The authored return point predates the current map; if it is not on walkable floor, the door
                // gives her back a few steps in front of itself instead of dropping her into the void.
                var target = returnPosition;
                var floor = GameObject.Find("NavigationFloor")?.GetComponent<UnityEngine.Tilemaps.Tilemap>();
                if (floor != null && !floor.HasTile(floor.WorldToCell(target))) target = (Vector2)transform.position + new Vector2(0f, -4f);
                var body = player.GetComponent<Rigidbody2D>();
                if (body != null) { body.position = target; body.linearVelocity = Vector2.zero; }
                player.transform.position = new Vector3(target.x, target.y, player.transform.position.z);
                player.ClearAutomationMoveInput();
            }
        }
    }
}
