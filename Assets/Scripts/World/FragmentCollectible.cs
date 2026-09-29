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
        public bool CanInteract(InteractionContext context) => !collected && (GameState.Instance == null || GameState.Instance.HasFlag(StoryFlag.SigilPuzzleComplete));
        public void Interact(InteractionContext context)
        {
            if (!CanInteract(context)) return;
            switch (interactionStage)
            {
                case 0: interactionStage++; StartDialogue(approachDialogue ?? Load("DLG_D_FRAGMENT_APPROACH")); break;
                case 1: interactionStage++; StartDialogue(touchDialogue ?? Load("DLG_D_FRAGMENT_TOUCH")); break;
                case 2: interactionStage++; StartCoroutine(MemoryRoutine()); break;
                default: collected = true; StartCoroutine(CollectRoutine()); break;
            }
        }
        private static DialogueData Load(string id) => Resources.Load<DialogueData>("Dialogue/" + id);
        private static void StartDialogue(DialogueData dialogue) => DialogueManager.Instance?.StartDialogue(dialogue);
        private IEnumerator MemoryRoutine()
        {
            InputGate.Instance?.Block("FragmentMemory");
            StartDialogue(memoryDialogue ?? Load("DLG_D_FRAGMENT_MEMORY"));
            yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
            InputGate.Instance?.Release("FragmentMemory");
        }
        private IEnumerator CollectRoutine()
        {
            InputGate.Instance?.Block("Fragment");
            StartDialogue(collectDialogue ?? Load("DLG_D_FRAGMENT_COLLECT"));
            yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
            GameState.Instance?.SetFragmentCollected();
            poe ??= FindAnyObjectByType<PoeFollower>();
            if (poe != null)
            {
                GrimoireCatalog.Discover("M07");
                yield return poe.NarrativeVanishAndReappear(poeReappearPoint);
                GrimoireCatalog.Discover("M08");
            }
            StartDialogue(Load("DLG_D_POE_VANISH"));
            yield return new WaitForSeconds(delayBeforeFinal);
            if (linkedFinalPortal != null) linkedFinalPortal.ActivateFromLinkedInteraction();
            else Debug.LogError($"Fragment '{name}' has no linked final portal for scene '{finalScene}'.", this);
        }
    }
}
