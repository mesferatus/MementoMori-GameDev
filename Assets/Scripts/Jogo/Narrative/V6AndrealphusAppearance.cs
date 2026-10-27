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
        public void Configure(bool inMoon)
        {
            moon=inMoon; visuals=GetComponentsInChildren<SpriteRenderer>(true);
        }
        private void Start()
        {
            foreach(var trigger in GetComponentsInChildren<DialogueTrigger>(true)) trigger.enabled=false;
            Show(false);
        }
        private void Show(bool visible)
        {
            if(visuals==null)return;
            foreach(var visual in visuals) if(visual!=null)visual.enabled=visible;
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
                || !state.Puzzles.GetSolved("lab.core") || V6Challenge.AnyOpen)return;
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
            DialogueManager.Instance.StartDialogue(Resources.Load<DialogueData>("Dialogue/DLG_L_ANDREALPHUS_01"));
            yield return new WaitUntil(()=>DialogueManager.Instance==null || !DialogueManager.Instance.IsOpen);
            GameState.Instance?.SetFlag(StoryFlag.AndrealphusMeeting01Complete);
            yield return new WaitForSeconds(.5f);
            Show(false);InputGate.Instance?.Release("AndrealphusAppearance");
        }
        private void OnDisable()
        {
            StopAllCoroutines();Show(false);
            InputGate.Instance?.Release("AndrealphusAppearance");
        }
    }
}
