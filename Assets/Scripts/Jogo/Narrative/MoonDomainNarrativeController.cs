using System.Collections.Generic;
using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.Poe;
using MementoMori.Puzzles;
using MementoMori.Player;
using UnityEngine;

namespace MementoMori.Narrative
{
    /// <summary>Maps existing Moon Domain state/events to the approved C4C dialogue beats.</summary>
    public sealed class MoonDomainNarrativeController : MonoBehaviour
    {
        private readonly Queue<DialogueData> pending = new();
        private readonly HashSet<string> queuedIds = new();
        private int observedGardenErrors;
        private int observedMirrorErrors;
        private int observedEchoErrors;
        private int observedSigilErrors;
        private bool observedCrescent;
        private bool observedFull;
        private bool observedWaning;
        private int lastMirrorProgress;
        private int lastSigilProgress;
        private PlayerController swimmer;
        private Transform galleryDoor;

        private void Start()
        {
            var state = GameState.Instance;
            observedGardenErrors = state == null ? 0 : state.GetCounter("garden.Crescente.errors") + state.GetCounter("garden.Cheia.errors") + state.GetCounter("garden.Minguante.errors");
            observedMirrorErrors = state?.GetCounter("mirror.errors") ?? 0;
            observedEchoErrors = state?.GetCounter("echo.errors") ?? 0;
            observedSigilErrors = state?.GetCounter("sigil.errors") ?? 0;
            observedCrescent = state != null && state.HasFlag(StoryFlag.GardenPetalCrescente);
            observedFull = state != null && state.HasFlag(StoryFlag.GardenPetalCheia);
            observedWaning = state != null && state.HasFlag(StoryFlag.GardenPetalMinguante);
            lastMirrorProgress = CountMirrorProgress(state);
            lastSigilProgress = FindAnyObjectByType<SigilRingPuzzle>()?.GetProgress() ?? 0;
            swimmer = FindAnyObjectByType<PlayerController>();
            galleryDoor = GameObject.Find("GalleryDoor_Cheia")?.transform;
            Queue("M01");
        }

        private void Update()
        {
            var state = GameState.Instance;
            if (state == null) return;

            if (swimmer == null) swimmer = FindAnyObjectByType<PlayerController>();
            if (swimmer != null && swimmer.IsSwimming) Queue("M02_WATER");
            if (galleryDoor != null && swimmer != null
                && Vector2.Distance(swimmer.transform.position, galleryDoor.position) < 5f)
                Queue("M05_OPEN");
            QueueWhen(state.HasFlag(StoryFlag.GardenComplete), "P01");
            QueueWhen(state.GetCounter("event.A2") > 0, "P02");
            QueueWhen(state.Puzzles.GetSolved("moon.illusory_corridor"), "P03");
            QueueWhen(state.Puzzles.GetSolved("moon.cycles"), "P04");
            QueueWhen(state.HasFlag(StoryFlag.SigilPuzzleComplete), "P05");

            var gardenProgress = CountGardenProgress(state);
            if (!observedCrescent && state.HasFlag(StoryFlag.GardenPetalCrescente)) Queue("DLG_D_GARDEN_CRESCENT_SUCCESS");
            if (!observedFull && state.HasFlag(StoryFlag.GardenPetalCheia)) Queue("DLG_D_GARDEN_FULL_SUCCESS");
            if (!observedWaning && state.HasFlag(StoryFlag.GardenPetalMinguante)) Queue("DLG_D_GARDEN_WANING_SUCCESS");
            observedCrescent = state.HasFlag(StoryFlag.GardenPetalCrescente);
            observedFull = state.HasFlag(StoryFlag.GardenPetalCheia);
            observedWaning = state.HasFlag(StoryFlag.GardenPetalMinguante);

            var gardenErrors = state.GetCounter("garden.Crescente.errors") + state.GetCounter("garden.Cheia.errors") + state.GetCounter("garden.Minguante.errors");
            if (gardenErrors > observedGardenErrors)
            {
                FindAnyObjectByType<PoeFollower>()?.ReactToError(false);
                if (!state.HasFlag(StoryFlag.GardenPetalMinguante)) Queue("DLG_D_GARDEN_WANING_INTRO");
            }
            observedGardenErrors = gardenErrors;
            QueueWhen(state.HasFlag(StoryFlag.GardenComplete), "DLG_D_MIRROR_PRESENT");
            if (gardenProgress > 0) Queue("DLG_D_GARDEN_FULL_NEAR");

            var mirrorErrors = state.GetCounter("mirror.errors");
            if (mirrorErrors > observedMirrorErrors) Queue("DLG_D_MIRROR_WRONG");
            observedMirrorErrors = mirrorErrors;
            var mirrorProgress = CountMirrorProgress(state);
            if (mirrorProgress > lastMirrorProgress && !state.HasFlag(StoryFlag.MirrorPuzzleComplete)) Queue("DLG_D_MIRROR_HINT_1");
            lastMirrorProgress = mirrorProgress;

            var echoErrors = state.GetCounter("echo.errors");
            if (echoErrors > observedEchoErrors)
            {
                if (echoErrors == 1) Queue("DLG_L_ECHO_WRONG");
                else if (echoErrors == 2) Queue("DLG_L_ECHO_HINT_POE");
            }
            observedEchoErrors = echoErrors;

            var sigil = FindAnyObjectByType<SigilRingPuzzle>();
            var sigilProgress = sigil?.GetProgress() ?? 0;
            if (sigilProgress > lastSigilProgress && sigilProgress < 3) Queue("DLG_D_SIGIL_PHASE_OK");
            lastSigilProgress = sigilProgress;
            var sigilErrors = state.GetCounter("sigil.errors");
            if (sigilErrors > observedSigilErrors) Queue("DLG_D_SIGIL_PHASE_WRONG");
            observedSigilErrors = sigilErrors;

            if (swimmer != null && swimmer.IsSwimming && state.GetCounter("event.M02_WATER") > 0
                && state.GetCounter("choice.CH_WATER") == 0 && !V61ChoicePresenter.IsOpen
                && DialogueManager.Instance != null && !DialogueManager.Instance.IsOpen)
            { V61ChoicePresenter.Show("CH_WATER"); return; }
            if (state.GetCounter("event.M05_OPEN") > 0 && state.GetCounter("choice.CH_GALLERY") == 0
                && !V61ChoicePresenter.IsOpen && DialogueManager.Instance != null && !DialogueManager.Instance.IsOpen)
            { V61ChoicePresenter.Show("CH_GALLERY"); return; }

            if (!MementoMori.Regions.V6Challenge.AnyOpen && !V61ChoicePresenter.IsOpen
                && pending.Count > 0 && DialogueManager.Instance != null && !DialogueManager.Instance.IsOpen)
            {
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
                        if (next.SequenceId == "M02_WATER") V61ChoicePresenter.Show("CH_WATER");
                        if (next.SequenceId == "M05_OPEN") V61ChoicePresenter.Show("CH_GALLERY");
                    }
                    DialogueManager.Instance.OnDialogueCompleted += Complete;
                }
            }
        }

        private static int CountGardenProgress(GameState state)
        {
            if (state == null) return 0;
            var count = 0;
            if (state.HasFlag(StoryFlag.GardenPetalCrescente)) count++;
            if (state.HasFlag(StoryFlag.GardenPetalCheia)) count++;
            if (state.HasFlag(StoryFlag.GardenPetalMinguante)) count++;
            return count;
        }

        private static int CountMirrorProgress(GameState state)
        {
            if (state == null) return 0;
            var count = 0;
            if (state.HasFlag(StoryFlag.MirrorDelayedSolved)) count++;
            if (state.HasFlag(StoryFlag.MirrorAheadSolved)) count++;
            if (state.HasFlag(StoryFlag.MirrorRoomSolved)) count++;
            return count;
        }

        private void QueueWhen(bool condition, string id) { if (condition) Queue(id); }

        private void Queue(string id)
        {
            if (GameState.Instance != null && GameState.Instance.GetCounter("dialogue." + id) > 0) return;
            if (!queuedIds.Add(id)) return;
            var dialogue = V61EventCatalog.TryGet(id, out _) ? V61EventCatalog.CreateDialogue(id)
                : Resources.Load<DialogueData>("Dialogue/" + id);
            if (dialogue != null) pending.Enqueue(dialogue);
        }
    }
}

