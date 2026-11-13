using MementoMori.Core;
using MementoMori.Interaction;
using UnityEngine;
using System;
using System.Collections;
using MementoMori.Audio;
using MementoMori.Dialogue;

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
        [SerializeField] private bool requiresConfiguredPuzzles;
        public void RequireConfiguredPuzzles() => requiresConfiguredPuzzles = true;
        private bool ConfiguredPuzzlesReady => !requiresConfiguredPuzzles || GameState.Instance != null && (sceneName == "DominioLua" ? GameState.Instance.Puzzles.LabyrinthGateOpen : GameState.Instance.Puzzles.MoonGateOpen);
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
        public bool CanInteract(InteractionContext context) => !used && ConfiguredPuzzlesReady && (!requiresRitual || GameState.Instance != null && GameState.Instance.RitualCompleted) && (!requiresPoe || GameState.Instance != null && GameState.Instance.PoeRevealed) && (string.IsNullOrEmpty(requiredStoryFlag) || HasRequiredFlag()) && HasRequiredFlags();
        private void Update()
        {
            RefreshVisualState(false);
            PreloadWhenApproached();
            if (sceneName != "DominioLua" || GameState.Instance == null
                || !GameState.Instance.HasFlag(StoryFlag.EchoTrial03Complete)) return;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null || Vector2.Distance(player.transform.position, transform.position) > 4f) return;
            GrimoireCatalog.Discover("A07");
            GrimoireCatalog.Discover("P07");
        }
        private float nextPreloadCheck;
        private void PreloadWhenApproached()
        {
            // Only the open gate to the Moon Domain: the player is a few steps from a guaranteed transition.
            if (sceneName != "DominioLua" || Time.unscaledTime < nextPreloadCheck || SceneLoader.Instance == null) return;
            nextPreloadCheck = Time.unscaledTime + .5f;
            if (SceneLoader.Instance.IsLoading || SceneLoader.Instance.IsPreloading(sceneName) || !CanInteract(default)) return;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && Vector2.Distance(player.transform.position, transform.position) < 7f)
                SceneLoader.Instance.Preload(sceneName);
        }
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
            if (sceneName == "FinalBeta" && GameState.Instance != null
                && GameState.Instance.HasFlag(StoryFlag.FragmentCollected))
            {
                StartCoroutine(FinalTraversal());
                return;
            }
            ActivateFromLinkedInteraction();
        }

        private IEnumerator FinalTraversal()
        {
            used = true;
            InputGate.Instance?.Block("FinalTraversal");
            var state = GameState.Instance;
            var manager = DialogueManager.Instance;
            if (state != null && state.GetCounter("event.M08") == 0)
            {
                var dialogue = V61EventCatalog.CreateDialogue("M08");
                if (manager == null || manager.IsOpen || dialogue == null || !manager.TryStartDialogue(dialogue))
                {
                    if (dialogue != null) Destroy(dialogue);
                    CancelFinalTraversal();
                    yield break;
                }
                yield return new WaitUntil(() => manager == null || !manager.IsOpen);
                state.SetCounter("event.M08", 1);
                Destroy(dialogue);
            }
            if (state != null && state.GetCounter("choice.CH_FINAL") == 0)
            {
                if (!V61ChoicePresenter.Show("CH_FINAL"))
                {
                    CancelFinalTraversal();
                    yield break;
                }
                yield return new WaitUntil(() => !V61ChoicePresenter.IsOpen
                    && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen));
                if (state.GetCounter("choice.CH_FINAL") == 0)
                {
                    CancelFinalTraversal();
                    yield break;
                }
            }
            InputGate.Instance?.Release("FinalTraversal");
            RuntimeAudio.PlayOneShot("12_portal_open", .8f);
            GamepadInput.Rumble(.25f, .45f, .35f);
            SceneLoader.Instance?.LoadScene(sceneName);
        }

        private void CancelFinalTraversal()
        {
            used = false;
            InputGate.Instance?.Release("FinalTraversal");
        }

        private void OnDisable() => InputGate.Instance?.Release("FinalTraversal");

        public void ActivateFromLinkedInteraction()
        {
            if (!CanInteract(default) || SceneLoader.Instance == null) return;
            used = true;
            RuntimeAudio.PlayOneShot("12_portal_open", .8f);
            GamepadInput.Rumble(.25f, .45f, .35f);
            SceneLoader.Instance?.LoadScene(sceneName);
        }
    }
}

