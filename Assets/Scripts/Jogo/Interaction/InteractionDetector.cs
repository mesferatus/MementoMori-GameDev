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
        /// <summary>Interactable currently offered to the player (read-only, for presentation).</summary>
        public static IInteractable CurrentTarget { get; private set; }

        public void ConfigurePrompt(InteractionPromptUI interactionPrompt) => prompt = interactionPrompt;

        private void Update()
        {
            if (prompt == null) prompt = FindAnyObjectByType<InteractionPromptUI>();
            RefreshCandidates();
            SelectCandidate();
            var blocked = InputGate.Instance != null && InputGate.Instance.IsBlocked;
            prompt?.SetTarget(blocked ? null : current);
            CurrentTarget = blocked ? null : current;

            var interactPressed = InputBindings.Down(GameAction.Interact) || AccessibilitySettings.Instance != null && AccessibilitySettings.Instance.RemapInteractToSpace && Input.GetKeyDown(KeyCode.Space);
            if (!blocked && current != null && interactPressed)
            {
                RuntimeAudio.PlayOneShot("06_interaction_click", .35f);
                current.Interact(new InteractionContext(gameObject));
            }
        }

        private void OnDisable() => CurrentTarget = null;

        // Enter/exit events alone lost targets: an object with two trigger colliders dropped out when Melantha
        // left one of them, and a zone enabled while she already stood in it never entered at all (the prompt
        // only came back after leaving the room). The list is rebuilt from what really overlaps her reach.
        private Collider2D reachCollider;
        private readonly List<Collider2D> overlaps = new();
        private ContactFilter2D filter;
        private void RefreshCandidates()
        {
            if (reachCollider == null) { reachCollider = GetComponent<Collider2D>(); filter = new ContactFilter2D { useTriggers = true }; filter.NoFilter(); }
            if (reachCollider == null || !reachCollider.enabled) return;
            overlaps.Clear();
            reachCollider.Overlap(filter, overlaps);
            candidates.Clear();
            foreach (var other in overlaps)
            {
                var interactable = FindInteractable(other);
                if (interactable != null && !candidates.Contains(interactable)) candidates.Add(interactable);
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

        // Priority only breaks near-ties: a high-priority prop across the room must not steal the prompt
        // from the object Melantha is actually standing at.
        private const float PriorityTieMargin = .3f;
        // Small on purpose: adjacent props (the three sigil rings sit ~0.6 apart) must hand over the prompt
        // as soon as another one is really closer, while still absorbing frame-to-frame jitter.
        private const float SwitchMargin = .1f;

        private void SelectCandidate()
        {
            var previous = current;
            var previousDistance = float.MaxValue;
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
                if (component is Behaviour behaviour && !behaviour.isActiveAndEnabled) continue;
                if (!candidate.CanInteract(context))
                    continue;
                if (IsObstructed(component))
                    continue;
                var distance = Vector2.Distance(InteractionAnchor.Position(component), transform.position);
                if (candidate is IInteractionReach reach && reach.MaxReach > 0f && distance > reach.MaxReach)
                    continue;
                if (ReferenceEquals(candidate, previous)) previousDistance = distance;
                var clearlyCloser = distance < bestDistance - PriorityTieMargin;
                var nearTie = Mathf.Abs(distance - bestDistance) <= PriorityTieMargin;
                var pet = candidate is MementoMori.Poe.PoePettable;
                var bestPet = current is MementoMori.Poe.PoePettable;
                if (current == null || bestPet && !pet
                    || pet == bestPet && (clearlyCloser
                        || nearTie && (candidate.InteractionPriority > bestPriority
                            || candidate.InteractionPriority == bestPriority && distance < bestDistance)))
                {
                    current = candidate;
                    bestPriority = candidate.InteractionPriority;
                    bestDistance = distance;
                }
            }
            // Hysteresis: the prompt stays on the object already shown until another one is clearly
            // closer or outranks it, so walking between two near props does not flip it every frame.
            if (previous != null && current != null && !ReferenceEquals(current, previous)
                && !(previous is MementoMori.Poe.PoePettable && !(current is MementoMori.Poe.PoePettable))
                && previousDistance < float.MaxValue
                && current.InteractionPriority <= previous.InteractionPriority
                && bestDistance > previousDistance - SwitchMargin)
                current = previous;
        }

        private bool IsObstructed(Component target)
        {
            var origin = (Vector2)transform.position;
            // Standing right at the target's own zone: the wall it is set into is not "in front" of it
            // (echo passages sit inside their niches and lost their prompt).
            foreach (var own in target.GetComponentsInChildren<Collider2D>())
                if (own != null && own.enabled && own.bounds.SqrDistance(new Vector3(origin.x, origin.y, own.bounds.center.z)) < .6f * .6f)
                    return false;
            var destination = (Vector2)InteractionAnchor.Position(target);
            var player = GetComponentInParent<MementoMori.Player.PlayerController>();
            foreach (var hit in Physics2D.LinecastAll(origin, destination))
            {
                var obstacle = hit.collider;
                if (obstacle == null || obstacle.isTrigger) continue;
                if (player != null && obstacle.transform.IsChildOf(player.transform)) continue;
                if (obstacle.transform.IsChildOf(target.transform) || target.transform.IsChildOf(obstacle.transform)) continue;
                // Some room props keep their solid collider on the visual sibling of the interaction trigger.
                // A collider enclosing the target point is the target's surface, not a barrier in front of it.
                if (obstacle.OverlapPoint(destination)) continue;
                if (target is IInteractionSurface surface && surface.IsOwnSurface(obstacle)) continue;
                return true;
            }
            return false;
        }

        private static IInteractable FindInteractable(Collider2D collider)
        {
            foreach (var behaviour in collider.GetComponentsInParent<MonoBehaviour>())
                if (behaviour.isActiveAndEnabled && behaviour is IInteractable interactable)
                    return interactable;
            return null;
        }
    }
}

