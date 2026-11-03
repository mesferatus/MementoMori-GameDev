using System.Collections;
using MementoMori.Core;
using MementoMori.Interaction;
using UnityEngine;
using MementoMori.Audio;
using MementoMori.Dialogue;
using MementoMori.Poe;

namespace MementoMori.World
{
    public sealed class FragmentCollectible : MonoBehaviour, IInteractable
    {
        [SerializeField] private string finalScene = "FinalBeta";
        [SerializeField, Min(0f)] private float delayBeforeFinal = 1f;
        [SerializeField] private Portal linkedFinalPortal;
        [SerializeField] private PoeFollower poe;
        [SerializeField] private Transform poeReappearPoint;
        [SerializeField] private DialogueData approachDialogue;
        [SerializeField] private DialogueData touchDialogue;
        [SerializeField] private DialogueData memoryDialogue;
        [SerializeField] private DialogueData collectDialogue;
        private bool collected;
        private int interactionStage;
        private bool busy;
        private void Start()
        {
            var state = GameState.Instance;
            collected = state != null && state.HasFlag(StoryFlag.FragmentCollected);
            interactionStage = collected ? 4 : Mathf.Clamp(state?.GetCounter("fragment.stage") ?? 0, 0, 3);
        }
        public void Configure(string targetScene, float delay)
        {
            finalScene = targetScene;
            delayBeforeFinal = Mathf.Max(0f, delay);
        }
        public void Configure(string targetScene, float delay, Portal finalPortal)
        {
            Configure(targetScene, delay);
            linkedFinalPortal = finalPortal;
        }
        public string InteractionVerb => interactionStage < 3 ? "Aproximar-se" : "Coletar";
        public int InteractionPriority => 10;
        public bool CanInteract(InteractionContext context) => !collected && !busy
            && DialogueManager.Instance != null && !DialogueManager.Instance.IsOpen
            && (GameState.Instance == null || GameState.Instance.HasFlag(StoryFlag.SigilPuzzleComplete));
        public void Interact(InteractionContext context)
        {
            if (!CanInteract(context)) return;
            if (GameState.Instance != null && GameState.Instance.GetCounter("event.M07") > 0
                && GameState.Instance.GetCounter("choice.CH_FRAGMENT") == 0)
            {
                V61ChoicePresenter.Show("CH_FRAGMENT");
                return;
            }
            switch (interactionStage)
            {
                case 0: AdvanceAfterDialogue(approachDialogue ?? Load("DLG_D_FRAGMENT_APPROACH")); break;
                case 1: AdvanceAfterDialogue(touchDialogue ?? Load("DLG_D_FRAGMENT_TOUCH")); break;
                case 2: StartCoroutine(MemoryRoutine()); break;
                default: StartCoroutine(CollectRoutine()); break;
            }
        }
        private void AdvanceAfterDialogue(DialogueData dialogue)
        {
            if (DialogueManager.Instance == null || !DialogueManager.Instance.TryStartDialogue(dialogue)) return;
            interactionStage++;
            GameState.Instance?.SetCounter("fragment.stage", interactionStage);
        }
        private static DialogueData Load(string id) => Resources.Load<DialogueData>("Dialogue/" + id);
        private static void StartDialogue(DialogueData dialogue) => DialogueManager.Instance?.StartDialogue(dialogue);
        private void OnDisable()
        {
            StopAllCoroutines();
            busy = false;
            InputGate.Instance?.Release("Fragment");
            InputGate.Instance?.Release("FragmentMemory");
        }
        private IEnumerator MemoryRoutine()
        {
            busy = true;
            InputGate.Instance?.Block("FragmentMemory");
            if (DialogueManager.Instance == null || !DialogueManager.Instance.TryStartDialogue(memoryDialogue ?? Load("DLG_D_FRAGMENT_MEMORY")))
            {
                InputGate.Instance?.Release("FragmentMemory");
                busy = false;
                yield break;
            }
            yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
            if (GameState.Instance != null && GameState.Instance.GetCounter("event.M07") == 0)
            {
                var v61 = V61EventCatalog.CreateDialogue("M07");
                if (v61 != null && DialogueManager.Instance != null && DialogueManager.Instance.TryStartDialogue(v61))
                {
                    yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
                    GameState.Instance?.SetCounter("event.M07", 1);
                }
                if (v61 != null) Destroy(v61);
            }
            if (GameState.Instance == null || GameState.Instance.GetCounter("event.M07") == 0)
            {
                InputGate.Instance?.Release("FragmentMemory");
                busy = false;
                yield break;
            }
            interactionStage = 3;
            GameState.Instance.SetCounter("fragment.stage", interactionStage);
            if (GameState.Instance != null && GameState.Instance.GetCounter("event.M07") > 0
                && GameState.Instance.GetCounter("choice.CH_FRAGMENT") == 0)
            {
                V61ChoicePresenter.Show("CH_FRAGMENT");
                yield return new WaitUntil(() => !V61ChoicePresenter.IsOpen
                    && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen));
            }
            InputGate.Instance?.Release("FragmentMemory");
            busy = false;
        }
        private IEnumerator CollectRoutine()
        {
            if (GameState.Instance != null && GameState.Instance.GetCounter("choice.CH_FRAGMENT") == 0)
            {
                V61ChoicePresenter.Show("CH_FRAGMENT");
                yield break;
            }
            busy = true;
            InputGate.Instance?.Block("Fragment");
            if (DialogueManager.Instance == null || !DialogueManager.Instance.TryStartDialogue(collectDialogue ?? Load("DLG_D_FRAGMENT_COLLECT")))
            {
                InputGate.Instance?.Release("Fragment");
                busy = false;
                yield break;
            }
            yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
            GameState.Instance?.SetFragmentCollected();
            GameState.Instance?.SetCounter("fragment.stage", 4);
            collected = true;
            poe ??= FindAnyObjectByType<PoeFollower>();
            if (poe != null)
            {
                GrimoireCatalog.Discover("M07");
                yield return poe.NarrativeVanishAndReappear(poeReappearPoint);
                GrimoireCatalog.Discover("M08");
            }
            StartDialogue(Load("DLG_D_POE_VANISH"));
            yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
            InputGate.Instance?.Release("Fragment");
            busy = false;
            // The final portal remains an explicit player action after the memory.
        }
    }
}

