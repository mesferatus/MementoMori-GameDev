using System.Collections;
using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.Regions;
using UnityEngine;
namespace MementoMori.Narrative
{
    public sealed class V6AndrealphusAppearance : MonoBehaviour
    {
        [SerializeField] private bool moon;
        [SerializeField] private SpriteRenderer[] visuals;
        private bool running;
        private bool visibleTarget;
        private float appearanceBlend;
        private Color[] originalColors;
        private Vector3 originalPosition;
        public void Configure(bool inMoon)
        {
            moon=inMoon; visuals=GetComponentsInChildren<SpriteRenderer>(true);
        }
        private void Start()
        {
            foreach(var trigger in GetComponentsInChildren<DialogueTrigger>(true)) trigger.enabled=false;
            originalPosition = transform.localPosition;
            if (visuals == null || visuals.Length == 0) visuals = GetComponentsInChildren<SpriteRenderer>(true);
            originalColors = new Color[visuals.Length];
            for (var i = 0; i < visuals.Length; i++)
                if (visuals[i] != null) originalColors[i] = visuals[i].color;
            Show(false);
        }
        private void Show(bool visible)
        {
            visibleTarget = visible;
        }
        private void LateUpdate()
        {
            if (visuals == null || originalColors == null) return;
            appearanceBlend = Mathf.MoveTowards(appearanceBlend, visibleTarget ? 1f : 0f,
                Time.unscaledDeltaTime * 1.8f);
            transform.localPosition = originalPosition + Vector3.up
                * (appearanceBlend * (.12f + Mathf.Sin(Time.unscaledTime * 1.8f) * .045f));
            for (var i = 0; i < visuals.Length; i++)
            {
                var visual = visuals[i];
                if (visual == null) continue;
                var color = originalColors[i];
                color.a *= appearanceBlend;
                visual.color = color;
                visual.enabled = appearanceBlend > .001f;
            }
        }
        private void Update()
        {
            var state=GameState.Instance;
            if(state==null)return;
            if(moon)
            {
                Show(!state.HasFlag(StoryFlag.AndrealphusMeeting02Complete)
                    && DialogueManager.Instance!=null && DialogueManager.Instance.IsOpen
                    && DialogueManager.Instance.CurrentSpeaker=="Andrealphus");
                return;
            }
            if(running || state.HasFlag(StoryFlag.AndrealphusMeeting01Complete)
                || !state.Puzzles.GetSolved("lab.core") || state.GetCounter("event.E05") == 0
                || V6Challenge.AnyOpen)return;
            var player=FindAnyObjectByType<MementoMori.Player.PlayerController>();
            if(player==null || Vector2.Distance(player.transform.position,transform.position)>6f)return;
            if(DialogueManager.Instance==null || DialogueManager.Instance.IsOpen)return;
            StartCoroutine(FirstMeeting());
        }
        private IEnumerator FirstMeeting()
        {
            running=true; Show(true);
            InputGate.Instance?.Block("AndrealphusAppearance");
            yield return new WaitForSeconds(.7f);
            var dialogue = V61EventCatalog.CreateDialogue("A1");
            if (dialogue == null || DialogueManager.Instance == null
                || !DialogueManager.Instance.TryStartDialogue(dialogue))
            {
                if (dialogue != null) Destroy(dialogue);
                Show(false);
                InputGate.Instance?.Release("AndrealphusAppearance");
                running = false;
                yield break;
            }
            yield return new WaitUntil(()=>DialogueManager.Instance==null || !DialogueManager.Instance.IsOpen);
            if (GameState.Instance != null && GameState.Instance.GetCounter("choice.CH_A1") == 0)
            {
                if (!V61ChoicePresenter.Show("CH_A1"))
                {
                    Destroy(dialogue);
                    Show(false);
                    InputGate.Instance?.Release("AndrealphusAppearance");
                    running = false;
                    yield break;
                }
                yield return new WaitUntil(() => !V61ChoicePresenter.IsOpen
                    && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen));
                if (GameState.Instance != null && GameState.Instance.GetCounter("choice.CH_A1") == 0)
                {
                    Destroy(dialogue);
                    Show(false);
                    InputGate.Instance?.Release("AndrealphusAppearance");
                    running = false;
                    yield break;
                }
            }
            GameState.Instance?.SetFlag(StoryFlag.AndrealphusMeeting01Complete);
            GameState.Instance?.SetCounter("event.A1", 1);
            Destroy(dialogue);
            yield return new WaitForSeconds(.5f);
            Show(false);InputGate.Instance?.Release("AndrealphusAppearance");
        }
        private void OnDisable()
        {
            StopAllCoroutines();Show(false);
            appearanceBlend = 0f;
            transform.localPosition = originalPosition;
            if (visuals != null) foreach (var visual in visuals) if (visual != null) visual.enabled = false;
            InputGate.Instance?.Release("AndrealphusAppearance");
        }
    }
}
