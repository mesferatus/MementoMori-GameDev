using System.Collections;
using MementoMori.Core;
using MementoMori.Interaction;
using UnityEngine;
using MementoMori.Audio;

namespace MementoMori.World
{
    public sealed class FragmentCollectible : MonoBehaviour, IInteractable
    {
        [SerializeField] private string finalScene = "FinalBeta";
        [SerializeField, Min(0f)] private float delayBeforeFinal = 1f;
        [SerializeField] private Portal linkedFinalPortal;
        private bool collected;
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
        public string InteractionVerb => "Aproximar-se";
        public int InteractionPriority => 10;
        public bool CanInteract(InteractionContext context) => !collected && (GameState.Instance == null || GameState.Instance.HasFlag(StoryFlag.SigilPuzzleComplete));
        public void Interact(InteractionContext context)
        {
            if (!CanInteract(context)) return;
            collected = true;
            RuntimeAudio.PlayOneShot("13_fragment_collect");
            StartCoroutine(CollectRoutine());
        }
        private IEnumerator CollectRoutine()
        {
            InputGate.Instance?.Block("Fragment");
            GameState.Instance?.SetFragmentCollected();
            yield return new WaitForSeconds(delayBeforeFinal);
            if (linkedFinalPortal != null) linkedFinalPortal.ActivateFromLinkedInteraction();
            else Debug.LogError($"Fragment '{name}' has no linked final portal for scene '{finalScene}'.", this);
        }
    }
}
