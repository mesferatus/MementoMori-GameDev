using System.Collections;
using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.World;
using UnityEngine;

namespace MementoMori.Puzzles
{
    public sealed class BlackMirrorSequenceController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer mirrorRenderer;
        [SerializeField] private Sprite[] crackStates;
        private bool played;

        public void Configure(SpriteRenderer target, params Sprite[] cracks)
        {
            mirrorRenderer = target;
            crackStates = cracks;
        }

        public void Play()
        {
            if (played || GameState.Instance != null && GameState.Instance.GetCounter("event.A2") > 0) return;
            played = true;
            GrimoireCatalog.Discover("P12");
            GameState.Instance?.SetFlag(StoryFlag.MirrorBlackSolved);
            StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            // The selected mirror owns the current dialogue; do not overwrite it.
            yield return null;
            yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
            if (GameState.Instance == null || GameState.Instance.GetCounter("event.C_BLACK") == 0)
                DialogueManager.Instance?.StartDialogue(Resources.Load<DialogueData>("Dialogue/DLG_D_MIRROR_SOLVED"));
            yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
            var dialogue = GameState.Instance != null && GameState.Instance.GetCounter("event.C_BLACK") > 0
                ? null : V61EventCatalog.CreateDialogue("C_BLACK");
            if (dialogue != null && (DialogueManager.Instance == null
                || !DialogueManager.Instance.TryStartDialogue(dialogue)))
            {
                Destroy(dialogue);
                played = false;
                yield break;
            }
            if (crackStates != null && crackStates.Length > 0 && mirrorRenderer != null)
            {
                foreach (var crack in crackStates)
                {
                    yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
                    if (crack != null) mirrorRenderer.sprite = crack;
                }
            }
            yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
            if (dialogue != null) GameState.Instance?.SetCounter("event.C_BLACK", 1);
            if (dialogue != null) Destroy(dialogue);
            var andrealphus = V61EventCatalog.CreateDialogue("A2");
            if (andrealphus == null || DialogueManager.Instance == null
                || !DialogueManager.Instance.TryStartDialogue(andrealphus))
            {
                if (andrealphus != null) Destroy(andrealphus);
                played = false;
                yield break;
            }
            yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
            if (GameState.Instance != null && GameState.Instance.GetCounter("choice.CH_A2") == 0)
            {
                if (!V61ChoicePresenter.Show("CH_A2"))
                {
                    Destroy(andrealphus);
                    played = false;
                    yield break;
                }
                yield return new WaitUntil(() => !V61ChoicePresenter.IsOpen
                    && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen));
                if (GameState.Instance != null && GameState.Instance.GetCounter("choice.CH_A2") == 0)
                {
                    Destroy(andrealphus);
                    played = false;
                    yield break;
                }
            }
            GameState.Instance?.SetFlag(StoryFlag.AndrealphusMeeting02Complete);
            GameState.Instance?.SetCounter("event.A2", 1);
            if (andrealphus != null) Destroy(andrealphus);
        }
    }
}
