using System.Collections.Generic;
using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.World;
using UnityEngine;

namespace MementoMori.Narrative
{
    /// <summary>Queues the C4B narrative beats from existing Labirinto progression state.</summary>
    public sealed class LabyrinthNarrativeController : MonoBehaviour
    {
        [SerializeField] private DialogueData poeSignal;
        [SerializeField] private DialogueData echoWrongRoute;
        [SerializeField] private DialogueData echoProgress;
        [SerializeField] private DialogueData echoConclusion;
        [SerializeField] private DialogueData andrealphusAfter;
        [SerializeField] private DialogueData poeAfterAndrealphus;
        [SerializeField] private DialogueData portalUnlocked;

        private readonly Queue<DialogueData> pending = new();
        private readonly HashSet<string> queuedIds = new();
        private int observedEchoErrors;

        private void Start()
        {
            poeSignal = Load("DLG_L_POE_SIGNAL_01");
            echoWrongRoute = Load("DLG_L_ECHO_WRONG");
            echoProgress = Load("DLG_L_ECHO_INTRO");
            echoConclusion = Load("DLG_L_ECHO_COMPLETE");
            andrealphusAfter = Load("DLG_L_ANDREALPHUS_01");
            poeAfterAndrealphus = Load("DLG_L_POE_REVEAL");
            portalUnlocked = Load("DLG_L_MOON_GATE_OPEN");
            observedEchoErrors = GameState.Instance?.GetCounter("echo.errors") ?? 0;
        }

        private static DialogueData Load(string id) => Resources.Load<DialogueData>("Dialogue/" + id);

        private void Update()
        {
            var state = GameState.Instance;
            if (state == null) return;

            QueueWhen(state.HasFlag(StoryFlag.PoeRevealed), poeSignal);

            var echoErrors = state.GetCounter("echo.errors");
            if (echoErrors > observedEchoErrors)
                Queue(echoWrongRoute);
            observedEchoErrors = echoErrors;

            QueueWhen(state.HasFlag(StoryFlag.EchoTrial01Complete), echoProgress);
            QueueWhen(state.HasFlag(StoryFlag.EchoTrial03Complete), echoConclusion);
            QueueWhen(state.HasFlag(StoryFlag.AndrealphusMeeting01Complete), andrealphusAfter);
            QueueWhen(state.HasFlag(StoryFlag.AndrealphusMeeting01Complete), poeAfterAndrealphus);

            var portal = FindFirstObjectByType<Portal>();
            QueueWhen(state.HasFlag(StoryFlag.EchoTrial03Complete) && portal != null && portal.CanInteract(default), portalUnlocked);

            if (pending.Count == 0 || DialogueManager.Instance == null || DialogueManager.Instance.IsOpen)
                return;

            DialogueManager.Instance.StartDialogue(pending.Dequeue());
        }

        private void QueueWhen(bool condition, DialogueData dialogue)
        {
            if (condition) Queue(dialogue);
        }

        private void Queue(DialogueData dialogue)
        {
            if (dialogue == null || string.IsNullOrWhiteSpace(dialogue.SequenceId) || !queuedIds.Add(dialogue.SequenceId))
                return;
            pending.Enqueue(dialogue);
        }
    }
}
