using MementoMori.Core;
using MementoMori.Interaction;
using UnityEngine;
using System;
using MementoMori.Audio;

namespace MementoMori.World
{
    public sealed class Portal : MonoBehaviour, IInteractable
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string interactionVerb = "Entrar";
        [SerializeField] private bool requiresRitual;
        [SerializeField] private bool requiresPoe;
        [SerializeField] private string requiredStoryFlag;
        [SerializeField] private string[] requiredStoryFlags = Array.Empty<string>();
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private Sprite lockedVisual;
        [SerializeField] private Sprite activeVisual;
        private bool lastVisualAvailability;
        private bool visualStateInitialized;
        private bool used;
        public void Configure(string targetScene, string verb, bool ritualRequired)
        {
            sceneName = targetScene;
            interactionVerb = verb;
            requiresRitual = ritualRequired;
        }
        public void Configure(string targetScene, string verb, bool ritualRequired, bool poeRequired)
        {
            Configure(targetScene, verb, ritualRequired);
            requiresPoe = poeRequired;
        }
        public void RequireFlag(string flagName) => requiredStoryFlag = flagName;
        public void RequireFlags(params string[] flagNames) => requiredStoryFlags = flagNames ?? Array.Empty<string>();
        public void ConfigureVisualStates(SpriteRenderer target, Sprite locked, Sprite active)
        {
            visual = target;
            lockedVisual = locked;
            activeVisual = active;
            RefreshVisualState(true);
        }
        public string InteractionVerb => interactionVerb;
        public int InteractionPriority => 5;
        public bool CanInteract(InteractionContext context) => !used && (!requiresRitual || GameState.Instance != null && GameState.Instance.RitualCompleted) && (!requiresPoe || GameState.Instance != null && GameState.Instance.PoeRevealed) && (string.IsNullOrEmpty(requiredStoryFlag) || HasRequiredFlag()) && HasRequiredFlags();
        private void Update() => RefreshVisualState(false);
        private void RefreshVisualState(bool force)
        {
            var available = CanInteract(default(InteractionContext));
            if (!force && visualStateInitialized && available == lastVisualAvailability) return;
            visualStateInitialized = true;
            lastVisualAvailability = available;
            if (visual != null && lockedVisual != null && activeVisual != null) visual.sprite = available ? activeVisual : lockedVisual;
        }
        private bool HasRequiredFlag() => System.Enum.TryParse(requiredStoryFlag, out StoryFlag flag) && GameState.Instance != null && GameState.Instance.HasFlag(flag);
        private bool HasRequiredFlags()
        {
            if (requiredStoryFlags == null || requiredStoryFlags.Length == 0) return true;
            if (GameState.Instance == null) return false;
            foreach (var name in requiredStoryFlags)
                if (!Enum.TryParse(name, out StoryFlag flag) || !GameState.Instance.HasFlag(flag)) return false;
            return true;
        }
        public void Interact(InteractionContext context)
        {
            if (!CanInteract(context)) return;
            ActivateFromLinkedInteraction();
        }

        public void ActivateFromLinkedInteraction()
        {
            if (used) return;
            used = true;
            RuntimeAudio.PlayOneShot("12_portal_open", .8f);
            SceneLoader.Instance?.LoadScene(sceneName);
        }
    }
}
