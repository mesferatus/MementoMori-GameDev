using System;
using MementoMori.Core;
using MementoMori.Dialogue;
using UnityEngine;
using UnityEngine.UI;

namespace MementoMori.UI
{
    /// <summary>Runs the short, fragment-gated narrative closure of the beta slice.</summary>
    public sealed class FinalBetaController : MonoBehaviour
    {
        [SerializeField] private DialogueData[] narrativeEntries;
        [SerializeField] private Text finalText;
        [SerializeField] private Button returnButton;
        [SerializeField] private GameObject credits;

        private int entryIndex;
        private bool sequenceComplete;
        private Text speakerLabel;
        private Text dialogueLabel;

        public bool SequenceComplete => sequenceComplete;
        public int ImplementedEntryCount => narrativeEntries == null ? 0 : narrativeEntries.Length;

        private void Start()
        {
            if (returnButton != null)
            {
                returnButton.interactable = false;
                returnButton.onClick.AddListener(ReturnToMenu);
            }

            if (credits != null) credits.SetActive(false);
            if (GameState.Instance == null || !GameState.Instance.HasFlag(StoryFlag.FragmentCollected))
            {
                Debug.LogWarning("FinalBeta requires FragmentCollected; narrative closure was not started.", this);
                return;
            }

            if (finalText != null) finalText.gameObject.SetActive(false);
            EnsureDialogueManager();
            StartNextEntry();
        }

        private void EnsureDialogueManager()
        {
            if (DialogueManager.Instance != null) return;

            var root = new GameObject("FinalBetaDialogue", typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var panel = new GameObject("Panel", typeof(Image));
            panel.transform.SetParent(root.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(.12f, .08f);
            panelRect.anchorMax = new Vector2(.88f, .32f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(.05f, .04f, .1f, .96f);

            speakerLabel = CreateLabel(panel.transform, "Speaker", 22, new Vector2(.05f, .68f), new Vector2(.95f, .92f));
            dialogueLabel = CreateLabel(panel.transform, "Body", 20, new Vector2(.05f, .12f), new Vector2(.95f, .68f));
            root.AddComponent<DialogueManager>().Configure(root.GetComponent<CanvasGroup>(), speakerLabel, dialogueLabel);
        }

        private static Text CreateLabel(Transform parent, string name, int size, Vector2 min, Vector2 max)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);
            var label = labelObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = size;
            label.color = Color.white;
            label.alignment = TextAnchor.UpperLeft;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            var rect = label.rectTransform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return label;
        }

        private void StartNextEntry()
        {
            if (narrativeEntries == null || entryIndex >= narrativeEntries.Length)
            {
                CompleteSequence();
                return;
            }

            var dialogue = narrativeEntries[entryIndex++];
            if (dialogue == null)
            {
                StartNextEntry();
                return;
            }

            Action completed = null;
            completed = () =>
            {
                DialogueManager.Instance.OnDialogueCompleted -= completed;
                StartNextEntry();
            };
            DialogueManager.Instance.OnDialogueCompleted += completed;
            DialogueManager.Instance.StartDialogue(dialogue);
        }

        private void CompleteSequence()
        {
            sequenceComplete = true;
            if (finalText != null) finalText.gameObject.SetActive(true);
            if (credits != null) credits.SetActive(true);
            if (returnButton != null) returnButton.interactable = true;
        }

        public void ReturnToMenu()
        {
            if (!sequenceComplete) return;
            GameManager.Instance?.ReturnToMenu();
        }
    }
}
