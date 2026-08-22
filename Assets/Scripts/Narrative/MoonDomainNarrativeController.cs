using System.Collections.Generic;
using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.Poe;
using MementoMori.Puzzles;
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
        private int lastGardenProgress;
        private int lastMirrorProgress;
        private int lastSigilProgress;

        private void Start()
        {
            var state = GameState.Instance;
            observedGardenErrors = state == null ? 0 : state.GetCounter("garden.Crescente.errors") + state.GetCounter("garden.Cheia.errors") + state.GetCounter("garden.Minguante.errors");
            observedMirrorErrors = state?.GetCounter("mirror.errors") ?? 0;
            observedEchoErrors = state?.GetCounter("echo.errors") ?? 0;
            observedSigilErrors = state?.GetCounter("sigil.errors") ?? 0;
            lastGardenProgress = CountGardenProgress(state);
            lastMirrorProgress = CountMirrorProgress(state);
            lastSigilProgress = FindAnyObjectByType<SigilRingPuzzle>()?.GetProgress() ?? 0;
            Queue("DLG_C4C_D01_MOON_ARRIVAL");
        }

        private void Update()
        {
            var state = GameState.Instance;
            if (state == null) return;

            var gardenProgress = CountGardenProgress(state);
            if (gardenProgress > lastGardenProgress)
            {
                if (state.HasFlag(StoryFlag.GardenPetalCheia)) Queue("DLG_C4C_D07_FULL_PROGRESS");
                else Queue("DLG_C4C_D04_CRESCENT_PROGRESS");
            }
            lastGardenProgress = gardenProgress;

            var gardenErrors = state.GetCounter("garden.Crescente.errors") + state.GetCounter("garden.Cheia.errors") + state.GetCounter("garden.Minguante.errors");
            if (gardenErrors > observedGardenErrors)
            {
                FindAnyObjectByType<PoeFollower>()?.ReactToError(false);
                if (!state.HasFlag(StoryFlag.GardenPetalMinguante)) Queue("DLG_C4C_D08_WANING_HINT");
            }
            observedGardenErrors = gardenErrors;
            QueueWhen(state.HasFlag(StoryFlag.GardenComplete), "DLG_C4C_D10_GARDEN_COMPLETE");
            QueueWhen(state.HasFlag(StoryFlag.GardenComplete), "DLG_C4C_D11_MIRROR_INTRO");
            if (gardenProgress > 0) Queue("DLG_C4C_D06_FULL_HINT");

            var mirrorErrors = state.GetCounter("mirror.errors");
            if (mirrorErrors > observedMirrorErrors) Queue("DLG_C4C_D12_MIRROR_ERROR");
            observedMirrorErrors = mirrorErrors;
            var mirrorProgress = CountMirrorProgress(state);
            if (mirrorProgress > lastMirrorProgress && !state.HasFlag(StoryFlag.MirrorPuzzleComplete)) Queue("DLG_C4C_D13_MIRROR_PROGRESS");
            lastMirrorProgress = mirrorProgress;

            var echoErrors = state.GetCounter("echo.errors");
            if (echoErrors > observedEchoErrors)
            {
                if (echoErrors == 1) Queue("DLG_C4C_D15_CORRIDOR_REPEAT");
                else if (echoErrors == 2) Queue("DLG_C4C_D17_CORRIDOR_HINT");
            }
            observedEchoErrors = echoErrors;
            QueueWhen(state.HasFlag(StoryFlag.EchoTrial03Complete), "DLG_C4C_D18_CORRIDOR_SUCCESS");

            var sigil = FindAnyObjectByType<SigilRingPuzzle>();
            var sigilProgress = sigil?.GetProgress() ?? 0;
            if (sigilProgress > lastSigilProgress && sigilProgress < 3) Queue("DLG_C4C_D21_SIGIL_PROGRESS");
            lastSigilProgress = sigilProgress;
            var sigilErrors = state.GetCounter("sigil.errors");
            if (sigilErrors > observedSigilErrors) Queue("DLG_C4C_D20_SIGIL_ERROR");
            observedSigilErrors = sigilErrors;
            QueueWhen(state.HasFlag(StoryFlag.SigilPuzzleComplete), "DLG_C4C_D22_SIGIL_SUCCESS");

            if (pending.Count > 0 && DialogueManager.Instance != null && !DialogueManager.Instance.IsOpen)
                DialogueManager.Instance.StartDialogue(pending.Dequeue());
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
            if (state.HasFlag(StoryFlag.MirrorAbsentSolved)) count++;
            return count;
        }

        private void QueueWhen(bool condition, string id) { if (condition) Queue(id); }

        private void Queue(string id)
        {
            if (!queuedIds.Add(id)) return;
            var dialogue = Resources.Load<DialogueData>("Dialogue/" + id);
            if (dialogue != null) pending.Enqueue(dialogue);
        }
    }
}
