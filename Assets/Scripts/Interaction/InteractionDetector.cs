using System.Collections.Generic;
using MementoMori.Core;
using MementoMori.UI;
using UnityEngine;
using MementoMori.Audio;

namespace MementoMori.Interaction
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class InteractionDetector : MonoBehaviour
    {
        [SerializeField] private InteractionPromptUI prompt;
        private readonly List<IInteractable> candidates = new();
        private IInteractable current;

        public void ConfigurePrompt(InteractionPromptUI interactionPrompt) => prompt = interactionPrompt;

        private void Update()
        {
            SelectCandidate();
            var blocked = InputGate.Instance != null && InputGate.Instance.IsBlocked;
            prompt?.SetTarget(blocked ? null : current);

            var interactPressed = Input.GetKeyDown(KeyCode.E) || AccessibilitySettings.Instance != null && AccessibilitySettings.Instance.RemapInteractToSpace && Input.GetKeyDown(KeyCode.Space);
            if (!blocked && current != null && interactPressed)
            {
                RuntimeAudio.PlayOneShot("06_interaction_click", .35f);
                current.Interact(new InteractionContext(gameObject));
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var interactable = FindInteractable(other);
            if (interactable != null && !candidates.Contains(interactable))
                candidates.Add(interactable);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var interactable = FindInteractable(other);
            if (interactable != null)
                candidates.Remove(interactable);
        }

        private void SelectCandidate()
        {
            current = null;
            var bestDistance = float.MaxValue;
            var bestPriority = int.MinValue;
            var context = new InteractionContext(gameObject);
            for (var index = candidates.Count - 1; index >= 0; index--)
            {
                var candidate = candidates[index];
                if (candidate is not Component component || component == null)
                {
                    candidates.RemoveAt(index);
                    continue;
                }
                if (!candidate.CanInteract(context))
                    continue;

                var distance = ((Vector2)component.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (candidate.InteractionPriority > bestPriority || candidate.InteractionPriority == bestPriority && distance < bestDistance)
                {
                    current = candidate;
                    bestPriority = candidate.InteractionPriority;
                    bestDistance = distance;
                }
            }
        }

        private static IInteractable FindInteractable(Collider2D collider)
        {
            foreach (var behaviour in collider.GetComponentsInParent<MonoBehaviour>())
                if (behaviour is IInteractable interactable)
                    return interactable;
            return null;
        }
    }
}
