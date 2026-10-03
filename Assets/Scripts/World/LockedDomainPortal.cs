using System.Collections;
using MementoMori.Audio;
using MementoMori.Interaction;
using MementoMori.World;
using TMPro;
using UnityEngine;

namespace MementoMori.World
{
    /// <summary>Feedback for a scenic domain that has no playable destination yet.</summary>
    public sealed class LockedDomainPortal : MonoBehaviour, IInteractable
    {
        private TextMeshPro message;
        private Coroutine hideRoutine;
        public string InteractionVerb => "Examinar o selo";
        public int InteractionPriority => 20;
        public bool CanInteract(InteractionContext context) => isActiveAndEnabled;

        public void Interact(InteractionContext context)
        {
            RuntimeAudio.PlayOneShot("10_sigil_error", .22f);
            GrimoireCatalog.Discover("A07");
            if (message == null)
            {
                var go = new GameObject("SealFeedback");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0, 1.6f, 0);
                message = go.AddComponent<TextMeshPro>();
                message.alignment = TextAlignmentOptions.Center;
                message.fontSize = 2;
                message.color = new Color(.82f, .73f, .95f);
                message.text = "O selo não responde";
                message.GetComponent<MeshRenderer>().sortingOrder = 80;
            }
            message.gameObject.SetActive(true);
            if (hideRoutine != null) StopCoroutine(hideRoutine);
            hideRoutine = StartCoroutine(Hide());
        }

        private IEnumerator Hide()
        {
            yield return new WaitForSeconds(2.5f);
            if (message != null) message.gameObject.SetActive(false);
            hideRoutine = null;
        }
    }
}
