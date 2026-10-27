using System;
using System.Collections;
using System.Collections.Generic;
using MementoMori.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MementoMori.Audio;

namespace MementoMori.Dialogue
{
    public sealed class DialogueManager : MonoBehaviour
    {
        private const string DialogueGate = "Dialogue";
        public static DialogueManager Instance { get; private set; }

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text speakerLabel;
        [SerializeField] private Text bodyLabel;
        [SerializeField] private TMP_Text speakerLabelTmp;
        [SerializeField] private TMP_Text bodyLabelTmp;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Sprite melanthaPortrait;
        [SerializeField] private Sprite poePortrait;
        [SerializeField] private Sprite andrealphusPortrait;
        [SerializeField] private GameObject continueCursor;

        private DialogueData activeDialogue;
        private int lineIndex;
        private Coroutine revealRoutine;
        private bool lineFullyVisible;
        private readonly List<DialogueLine> history = new();
        private GameObject historyPanel;
        private Text historyLabel;
        [SerializeField, Range(10f, 120f)] private float defaultCharactersPerSecond = 38f;
        [SerializeField] private bool instantAdvance;
        public bool IsOpen => activeDialogue != null;
        public string CurrentSpeaker => IsOpen && lineIndex < activeDialogue.Lines.Length
            ? activeDialogue.Lines[lineIndex].Speaker : string.Empty;
        public IReadOnlyList<DialogueLine> History => history;
        public event Action OnDialogueCompleted;

        public void Configure(CanvasGroup group, Text speaker, Text body)
        {
            canvasGroup = group;
            speakerLabel = speaker;
            bodyLabel = body;
            SetVisible(false);
            CreateHistoryPanel();
        }

        public void Configure(CanvasGroup group, TMP_Text speaker, TMP_Text body, Image portrait, GameObject cursor)
        {
            canvasGroup = group;
            speakerLabelTmp = speaker;
            bodyLabelTmp = body;
            portraitImage = portrait;
            continueCursor = cursor;
            SetVisible(false);
            CreateHistoryPanel();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (!IsOpen) return;
            if (AccessibilitySettings.Instance != null)
                defaultCharactersPerSecond = Mathf.Clamp(38f * AccessibilitySettings.Instance.TextSpeed, 10f, 120f);
            if (Input.GetKeyDown(KeyCode.H)) { ToggleHistory(); return; }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                instantAdvance = !instantAdvance;
                if (instantAdvance) RevealImmediately();
                return;
            }
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) Advance();
        }

        public void StartDialogue(DialogueData dialogue)
        {
            if (dialogue == null || dialogue.Lines == null || dialogue.Lines.Length == 0 || IsOpen || !dialogue.IsAvailable(GameState.Instance))
                return;

            ApplyDialogueLayout();
            activeDialogue = dialogue;
            lineIndex = 0;
            InputGate.Instance?.Block(DialogueGate);
            SetVisible(true);
            RenderCurrentLine();
        }

        private void ApplyDialogueLayout()
        {
            if (speakerLabelTmp != null)
            {
                var nameRect = speakerLabelTmp.rectTransform;
                nameRect.anchorMin = nameRect.anchorMax = new Vector2(0f, 1f);
                nameRect.anchoredPosition = new Vector2(235f, -91f);
                nameRect.sizeDelta = new Vector2(270f, 48f);
                speakerLabelTmp.alignment = TextAlignmentOptions.Center;
            }

            if (portraitImage == null) return;
            var frame = portraitImage.transform.parent as RectTransform;
            if (frame != null && frame.name == "PortraitFrame")
                frame.sizeDelta = new Vector2(350f, 350f);
            var portraitRect = portraitImage.rectTransform;
            portraitRect.anchorMin = portraitRect.anchorMax = new Vector2(.5f, .5f);
            portraitRect.anchoredPosition = Vector2.zero;
            portraitRect.sizeDelta = new Vector2(250f, 250f);
            portraitImage.preserveAspect = true;
        }

        public void Advance()
        {
            if (!IsOpen)
                return;
            if (!lineFullyVisible)
            {
                RevealImmediately();
                return;
            }
            history.Add(activeDialogue.Lines[lineIndex]);
            lineIndex++;
            if (lineIndex < activeDialogue.Lines.Length)
            {
                RenderCurrentLine();
                return;
            }

            activeDialogue = null;
            RuntimeAudio.StopChatter();
            if (revealRoutine != null) StopCoroutine(revealRoutine);
            revealRoutine = null;
            SetVisible(false);
            InputGate.Instance?.Release(DialogueGate);
            OnDialogueCompleted?.Invoke();
        }

        private void RenderCurrentLine()
        {
            var line = activeDialogue.Lines[lineIndex];
            SetSpeakerText(line.Speaker);
            var scale = AccessibilitySettings.Instance == null ? 1f : AccessibilitySettings.Instance.FontScale;
            if (speakerLabel != null) speakerLabel.fontSize = Mathf.RoundToInt(22f * scale);
            if (bodyLabel != null) bodyLabel.fontSize = Mathf.RoundToInt(18f * scale);
            if (speakerLabelTmp != null) speakerLabelTmp.fontSize = Mathf.RoundToInt(34f * scale);
            if (bodyLabelTmp != null) bodyLabelTmp.fontSize = Mathf.RoundToInt(32f * scale);
            if (portraitImage != null)
            {
                portraitImage.sprite = line.Speaker == "Poe" ? poePortrait : line.Speaker == "Andrealphus" ? andrealphusPortrait : line.Speaker == "Melantha" ? melanthaPortrait : null;
                portraitImage.color = Color.white;
                portraitImage.preserveAspect = true;
                portraitImage.enabled = portraitImage.sprite != null;
            }
            if (continueCursor != null)
                continueCursor.SetActive(false);
            RuntimeAudio.StopChatter();
            if (revealRoutine != null) StopCoroutine(revealRoutine);
            revealRoutine = StartCoroutine(RevealLine(line));
        }

        public void SetTextSpeed(float charactersPerSecond) => defaultCharactersPerSecond = Mathf.Clamp(charactersPerSecond, 10f, 120f);

        private IEnumerator RevealLine(DialogueLine line)
        {
            lineFullyVisible = false;
            SetBodyText(string.Empty);
            if (line.PauseBefore > 0f) yield return new WaitForSecondsRealtime(line.PauseBefore);
            var content = line.Text ?? string.Empty;
            var speed = (line.CharactersPerSecond > 0f ? line.CharactersPerSecond : defaultCharactersPerSecond) * (AccessibilitySettings.Instance == null ? 1f : AccessibilitySettings.Instance.TextSpeed);
            if (instantAdvance || content.Length == 0)
            {
                SetBodyText(content);
            }
            else
            {
                var spokenCharacters = 0;
                var syllable = 0;
                var cadence = line.Speaker == "Andrealphus" ? 4 : line.Speaker == "Poe" ? 3 : 2;
                for (var i = 1; i <= content.Length; i++)
                {
                    SetBodyText(content.Substring(0, i));
                    if (char.IsLetterOrDigit(content[i - 1]) && ++spokenCharacters % cadence == 0)
                        RuntimeAudio.PlayChatter(line.Speaker, syllable++);
                    yield return new WaitForSecondsRealtime(1f / speed);
                }
            }
            RuntimeAudio.StopChatter();
            if (line.PauseAfter > 0f) yield return new WaitForSecondsRealtime(line.PauseAfter);
            lineFullyVisible = true;
            if (continueCursor != null)
                continueCursor.SetActive(true);
            revealRoutine = null;
        }

        private void RevealImmediately()
        {
            if (!IsOpen) return;
            if (revealRoutine != null) StopCoroutine(revealRoutine);
            RuntimeAudio.StopChatter();
            var line = activeDialogue.Lines[lineIndex];
            SetBodyText(line.Text ?? string.Empty);
            lineFullyVisible = true;
            if (continueCursor != null)
                continueCursor.SetActive(true);
            revealRoutine = null;
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup == null)
                return;
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
            if (!visible && continueCursor != null)
                continueCursor.SetActive(false);
        }

        private void SetSpeakerText(string value)
        {
            if (speakerLabel != null) speakerLabel.text = value;
            if (speakerLabelTmp != null) speakerLabelTmp.text = value;
        }

        private void SetBodyText(string value)
        {
            if (bodyLabel != null) bodyLabel.text = value;
            if (bodyLabelTmp != null) bodyLabelTmp.text = value;
        }

        private void CreateHistoryPanel()
        {
            if (historyPanel != null || bodyLabel == null || bodyLabel.canvas == null) return;
            historyPanel = new GameObject("DialogueHistory", typeof(RectTransform), typeof(Image));
            historyPanel.transform.SetParent(bodyLabel.canvas.transform, false);
            historyPanel.GetComponent<Image>().color = new Color(.05f, .04f, .1f, .96f);
            var rect = historyPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(.08f, .35f); rect.anchorMax = new Vector2(.92f, .86f);
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var labelObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(historyPanel.transform, false);
            historyLabel = labelObject.GetComponent<Text>();
            historyLabel.font = bodyLabel.font; historyLabel.fontSize = 15; historyLabel.color = Color.white;
            historyLabel.alignment = TextAnchor.UpperLeft; historyLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            var labelRect = historyLabel.rectTransform;
            labelRect.anchorMin = new Vector2(.04f, .06f); labelRect.anchorMax = new Vector2(.96f, .94f);
            labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;
            historyPanel.SetActive(false);
        }

        private void ToggleHistory()
        {
            CreateHistoryPanel();
            if (historyPanel == null) return;
            var show = !historyPanel.activeSelf;
            if (show && historyLabel != null)
            {
                var first = Mathf.Max(0, history.Count - 12);
                var value = "HISTÓRICO\n\n";
                for (var i = first; i < history.Count; i++) value += history[i].Speaker + ": " + history[i].Text + "\n\n";
                historyLabel.text = value;
            }
            historyPanel.SetActive(show);
        }
    }
}
