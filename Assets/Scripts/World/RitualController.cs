using System.Collections;
using MementoMori.Core;
using MementoMori.Interaction;
using MementoMori.Dialogue;
using UnityEngine;
using MementoMori.Audio;
using MementoMori.UI;

namespace MementoMori.World
{
    public sealed class RitualController : MonoBehaviour, IInteractable
    {
        [SerializeField, Min(0f)] private float transitionDelay = 1f;
        private bool completed;
        public void Configure(string targetScene, float delay)
        {
            transitionDelay = Mathf.Max(0f, delay);
        }
        public string InteractionVerb => "Concluir o ritual";
        public int InteractionPriority => 10;
        public bool CanInteract(InteractionContext context) => !completed
            && GameState.Instance != null && GameState.Instance.HasFlag(StoryFlag.RoomCandlesDone);
        public void Interact(InteractionContext context)
        {
            if (completed) return;
            GrimoireCatalog.Discover("A04");
            if (!HasEssentialRoomSteps())
            {
                DialogueManager.Instance?.StartDialogue(Resources.Load<DialogueData>("Dialogue/" + MissingStepDialogue()));
                return;
            }
            completed = true;
            StartCoroutine(CompleteRoutine());
        }
        private static bool HasEssentialRoomSteps()
        {
            var state = GameState.Instance;
            return state != null && state.HasFlag(StoryFlag.RoomPhotoExamined)
                && state.HasFlag(StoryFlag.RoomGrimoireRead)
                && state.HasFlag(StoryFlag.RoomWindowSecured)
                && state.HasFlag(StoryFlag.RoomRitualItemStored)
                && state.HasFlag(StoryFlag.RoomCandlesDone);
        }
        private static string MissingStepDialogue()
        {
            var state = GameState.Instance;
            return "DLG_Q_CIRCLE_NEEDS_ANCHOR";
        }
        private IEnumerator CompleteRoutine()
        {
            InputGate.Instance?.Block("Ritual");
            RuntimeAudio.PlayOneShot("04_ritual_circle_loop", .55f);
            DialogueManager.Instance?.StartDialogue(Resources.Load<DialogueData>("Dialogue/DLG_Q_RITUAL_PROGRESS"));
            yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
            yield return new WaitForSeconds(transitionDelay);
            GameState.Instance?.SetRitualCompleted();
            ObjectiveToastController.Instance?.EvaluateForScene("Quarto");
            DialogueManager.Instance?.StartDialogue(Resources.Load<DialogueData>("Dialogue/DLG_Q_RITUAL_COMPLETE"));
            yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
            InputGate.Instance?.Release("Ritual");
        }
    }
}
