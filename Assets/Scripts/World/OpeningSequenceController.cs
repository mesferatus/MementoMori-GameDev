using System.Collections;
using MementoMori.Core;
using MementoMori.Audio;
using MementoMori.Dialogue;
using UnityEngine;
using UnityEngine.UI;

namespace MementoMori.World
{
    public sealed class OpeningSequenceController : MonoBehaviour
    {
        private IEnumerator Start()
        {
            InputGate.Instance?.Block("OpeningSequence");
            var canvasObject = new GameObject("OpeningOverlay", typeof(Canvas), typeof(CanvasGroup), typeof(Image));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var image = canvasObject.GetComponent<Image>();
            image.color = new Color(.01f, .005f, .02f, 1f);
            var group = canvasObject.GetComponent<CanvasGroup>();
            var textObject = new GameObject("OpeningText", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(canvasObject.transform, false);
            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(.9f, .88f, .95f, 0f);
            text.rectTransform.anchorMin = new Vector2(.12f, .4f);
            text.rectTransform.anchorMax = new Vector2(.88f, .6f);
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;

            // Keep the player and camera gated until the text has faded completely.
            RuntimeAudio.PlayOneShot("07_dialogue_blip", .2f);
            yield return new WaitForSecondsRealtime(.35f);
            RuntimeAudio.PlayOneShot("05_transition_sleep", .18f);
            yield return new WaitForSecondsRealtime(.55f);
            RuntimeAudio.PlayOneShot("12_portal_open", .14f);
            yield return new WaitForSecondsRealtime(.25f);
            text.text = "Algumas aus\u00eancias ocupam mais espa\u00e7o do que um corpo.";
            for (var i = 0; i < 12; i++)
            {
                text.color = new Color(.9f, .88f, .95f, i / 12f);
                yield return new WaitForSecondsRealtime(.06f);
            }
            yield return new WaitForSecondsRealtime(1.1f);
            for (var i = 0; i < 18; i++)
            {
                group.alpha = 1f - i / 18f;
                yield return new WaitForSecondsRealtime(.06f);
            }
            Destroy(canvasObject);
            InputGate.Instance?.Release("OpeningSequence");
            DialogueManager.Instance?.StartDialogue(Resources.Load<DialogueData>("Dialogue/DLG_ROOM_OPENING"));
        }
    }
}
