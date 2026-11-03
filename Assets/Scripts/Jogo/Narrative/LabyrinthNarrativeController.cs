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
        private DialogueData falseDoorSolved;
        private DialogueData echoSolved;
        private DialogueData voiceWellSolved;
        private DialogueData courtyardSolved;
        private DialogueData coreSolved;
        private readonly Dictionary<string, DialogueData> optionalSanctuaries = new();
        private readonly Dictionary<string, Vector2> sanctuaryCenters = new();
        private MementoMori.Player.PlayerController player;

        private readonly Queue<DialogueData> pending = new();
        private readonly HashSet<string> queuedIds = new();
        private int observedEchoErrors;

        private void Start()
        {
            Queue(V61EventCatalog.CreateDialogue("L01"));
            poeSignal = V61EventCatalog.CreateDialogue("C_POE");
            echoWrongRoute = Load("DLG_L_ECHO_WRONG");
            echoProgress = Load("DLG_L_ECHO_INTRO");
            echoConclusion = Load("DLG_L_ECHO_COMPLETE");
            andrealphusAfter = Load("DLG_L_ANDREALPHUS_01");
            poeAfterAndrealphus = Load("DLG_L_POE_REVEAL");
            portalUnlocked = V61EventCatalog.CreateDialogue("L08", "on_gate_unlocked");
            falseDoorSolved = V61EventCatalog.CreateDialogue("E02");
            echoSolved = V61EventCatalog.CreateDialogue("E01");
            voiceWellSolved = V61EventCatalog.CreateDialogue("E03");
            courtyardSolved = V61EventCatalog.CreateDialogue("E04");
            coreSolved = V61EventCatalog.CreateDialogue("E05");
            for (var number = 9; number <= 14; number++)
            {
                var id = "L" + number.ToString("00");
                var rootName = id + "_Santuário " + (number switch
                {
                    9 => "do Diabo", 10 => "da Torre", 11 => "do Enforcado",
                    12 => "do Eremita", 13 => "da Morte", _ => "do Julgamento"
                });
                var root = GameObject.Find(rootName);
                if (root == null) continue;
                var renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
                if (renderers.Length == 0) continue;
                var center = Vector2.zero;
                foreach (var renderer in renderers) center += (Vector2)renderer.bounds.center;
                sanctuaryCenters[id] = center / renderers.Length;
                optionalSanctuaries[id] = V61EventCatalog.CreateDialogue(id);
            }
            observedEchoErrors = GameState.Instance?.GetCounter("echo.errors") ?? 0;
            player = FindAnyObjectByType<MementoMori.Player.PlayerController>();
        }

        private static DialogueData Load(string id) => Resources.Load<DialogueData>("Dialogue/" + id);

        private void Update()
        {
            var state = GameState.Instance;
            if (state == null) return;

            QueueWhen(state.Puzzles.GetSolved("lab.false_door"), falseDoorSolved);
            QueueWhen(state.HasFlag(StoryFlag.EchoTrial03Complete), echoSolved);
            QueueWhen(state.HasFlag(StoryFlag.PoeRevealed), poeSignal);
            QueueWhen(state.HasFlag(StoryFlag.VoiceWellComplete), voiceWellSolved);
            QueueWhen(state.Puzzles.GetSolved("lab.skyless_courtyard"), courtyardSolved);
            QueueWhen(state.Puzzles.GetSolved("lab.core"), coreSolved);
            if (player == null) player = FindAnyObjectByType<MementoMori.Player.PlayerController>();
            if (player != null)
                foreach (var landmark in sanctuaryCenters)
                    if (Vector2.Distance(player.transform.position, landmark.Value) < 6f)
                        Queue(optionalSanctuaries[landmark.Key]);

            var echoErrors = state.GetCounter("echo.errors");
            if (echoErrors > observedEchoErrors)
                Queue(echoWrongRoute);
            observedEchoErrors = echoErrors;

            QueueWhen(state.HasFlag(StoryFlag.EchoTrial01Complete), echoProgress);
            // V6AndrealphusAppearance owns the first meeting, once.
            QueueWhen(state.HasFlag(StoryFlag.AndrealphusMeeting01Complete), poeAfterAndrealphus);

            
            QueueWhen(state.Puzzles.LabyrinthGateOpen, portalUnlocked);

            if (MementoMori.Regions.V6Challenge.AnyOpen || V61ChoicePresenter.IsOpen
                || DialogueManager.Instance == null || DialogueManager.Instance.IsOpen)
                return;

            if (state.GetCounter("event.C_POE") > 0 && state.GetCounter("choice.CH_POE") == 0)
            { V61ChoicePresenter.Show("CH_POE"); return; }
            if (state.GetCounter("event.L11") > 0 && state.GetCounter("choice.CH_SANCT") == 0)
            { V61ChoicePresenter.Show("CH_SANCT"); return; }
            if (pending.Count == 0) return;

            var next = pending.Peek();
            if (DialogueManager.Instance.TryStartDialogue(next))
            {
                pending.Dequeue();
                void Complete()
                {
                    DialogueManager.Instance.OnDialogueCompleted -= Complete;
                    GameState.Instance?.SetCounter("dialogue." + next.SequenceId, 1);
                    if (V61EventCatalog.TryGet(next.SequenceId, out _))
                        GameState.Instance?.SetCounter("event." + next.SequenceId, 1);
                    if (next.SequenceId == "C_POE") V61ChoicePresenter.Show("CH_POE");
                    if (next.SequenceId == "L11") V61ChoicePresenter.Show("CH_SANCT");
                }
                DialogueManager.Instance.OnDialogueCompleted += Complete;
            }
        }

        private void QueueWhen(bool condition, DialogueData dialogue)
        {
            if (condition) Queue(dialogue);
        }

        private void Queue(DialogueData dialogue)
        {
            if (dialogue == null || string.IsNullOrWhiteSpace(dialogue.SequenceId)
                || GameState.Instance != null && GameState.Instance.GetCounter("dialogue." + dialogue.SequenceId) > 0
                || !queuedIds.Add(dialogue.SequenceId))
                return;
            pending.Enqueue(dialogue);
        }
    }
}

