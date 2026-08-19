using System.Collections;
using MementoMori.Core;
using MementoMori.Interaction;
using UnityEngine;
using MementoMori.Audio;

namespace MementoMori.World
{
    public sealed class RitualController : MonoBehaviour, IInteractable
    {
        [SerializeField] private string nextScene = "Labirinto";
        [SerializeField, Min(0f)] private float transitionDelay = 1f;
        private bool completed;
        public void Configure(string targetScene, float delay)
        {
            nextScene = targetScene;
            transitionDelay = Mathf.Max(0f, delay);
        }
        public string InteractionVerb => "Concluir o ritual";
        public int InteractionPriority => 10;
        public bool CanInteract(InteractionContext context) => !completed;
        public void Interact(InteractionContext context)
        {
            if (completed) return;
            completed = true;
            StartCoroutine(CompleteRoutine());
        }
        private IEnumerator CompleteRoutine()
        {
            InputGate.Instance?.Block("Ritual");
            RuntimeAudio.PlayOneShot("04_ritual_circle_loop", .55f);
            yield return new WaitForSeconds(transitionDelay);
            GameState.Instance?.SetRitualCompleted();
            InputGate.Instance?.Release("Ritual");
            SceneLoader.Instance?.LoadScene(nextScene);
        }
    }
}
