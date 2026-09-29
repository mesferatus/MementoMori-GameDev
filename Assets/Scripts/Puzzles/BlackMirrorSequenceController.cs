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
            if (played) return;
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
            DialogueManager.Instance?.StartDialogue(Resources.Load<DialogueData>("Dialogue/DLG_D_MIRROR_SOLVED"));
            yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
            var dialogue = Resources.Load<DialogueData>("Dialogue/DLG_D_BLACK_MIRROR_MEMORY");
            DialogueManager.Instance?.StartDialogue(dialogue);
            if (crackStates != null && crackStates.Length > 0 && mirrorRenderer != null)
            {
                foreach (var crack in crackStates)
                {
                    yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
                    if (crack != null) mirrorRenderer.sprite = crack;
                }
            }
            yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
            DialogueManager.Instance?.StartDialogue(Resources.Load<DialogueData>("Dialogue/DLG_D_ANDREALPHUS_02"));
            yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
            GameState.Instance?.SetFlag(StoryFlag.AndrealphusMeeting02Complete);
        }
    }
}
