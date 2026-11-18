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
        [SerializeField] private SpriteRenderer pickupVisual;
        private bool collected;
        private int interactionStage;
        private bool busy;
        private void Start()
        {
            var state = GameState.Instance;
            collected = state != null && state.HasFlag(StoryFlag.FragmentCollected);
            if (pickupVisual != null) pickupVisual.enabled = !collected;
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
        /// <summary>
        /// The visible pickup: the Fragment lifts, flares and is drawn into Melantha's hands, so the player sees
        /// it leave the pedestal instead of only reading about it.
        /// </summary>
        private IEnumerator CollectFlourish()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            var art = new System.Collections.Generic.List<SpriteRenderer>();
            if (pickupVisual != null && pickupVisual.enabled && pickupVisual.sprite != null) art.Add(pickupVisual);
            if (art.Count == 0) foreach (var r in GetComponentsInChildren<SpriteRenderer>())
                if (r.enabled && r.sprite != null && r.bounds.size.x < 3f && r.bounds.size.y < 3f && !art.Contains(r)) art.Add(r);
            if (art.Count == 0)
                foreach (var r in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
                    if (r.enabled && r.sprite != null && r.name.Contains("Fragment") && r.bounds.size.x < 3f
                        && Vector2.Distance(r.bounds.center, transform.position) < 3f) art.Add(r);
            RuntimeAudio.PlayOneShot("13_fragment_collect", .7f);
            GamepadInput.Rumble(.2f, .35f, .3f);
            if (art.Count == 0) { yield return new WaitForSeconds(.6f); yield break; }
            var starts = new Vector3[art.Count];
            var scales = new Vector3[art.Count];
            var colors = new Color[art.Count];
            for (var i = 0; i < art.Count; i++)
            {
                starts[i] = art[i].transform.position; scales[i] = art[i].transform.localScale; colors[i] = art[i].color;
            }
            const float rise = .7f, gather = .55f;
            for (var t = 0f; t < rise + gather;)
            {
                t = Mathf.Min(t + Time.deltaTime, rise + gather);
                for (var i = 0; i < art.Count; i++)
                {
                    var r = art[i];
                    if (r == null) continue;
                    Vector3 position; float scale; float glow;
                    if (t < rise)
                    {
                        var k = Mathf.SmoothStep(0f, 1f, t / rise);
                        position = starts[i] + Vector3.up * (.6f * k);
                        scale = 1f + .25f * k; glow = k;
                    }
                    else
                    {
                        var k = Mathf.SmoothStep(0f, 1f, (t - rise) / gather);
                        var hands = player != null ? player.transform.position + Vector3.up * .5f : starts[i];
                        position = Vector3.Lerp(starts[i] + Vector3.up * .6f, hands, k);
                        scale = Mathf.Lerp(1.25f, .1f, k); glow = 1f;
                    }
                    r.transform.position = position;
                    r.transform.localScale = scales[i] * scale;
                    var c = Color.Lerp(colors[i], new Color(1f, .97f, .85f, colors[i].a), glow * .6f);
                    if (t >= rise) c.a = colors[i].a * (1f - Mathf.Clamp01((t - rise) / gather));
                    r.color = c;
                }
                yield return null;
            }
            foreach (var r in art) if (r != null) r.enabled = false;
            yield return new WaitForSeconds(.25f);
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
            yield return CollectFlourish();
            GameState.Instance?.SetFragmentCollected();
            GameState.Instance?.SetCounter("fragment.stage", 4);
            collected = true;
            if (poe == null) poe = FindAnyObjectByType<PoeFollower>();
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

