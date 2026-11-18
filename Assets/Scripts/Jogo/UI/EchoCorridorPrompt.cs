using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MementoMori.UI
{
    /// <summary>The authored E01 question stays readable while the player searches the corridors.</summary>
    public sealed class EchoCorridorPrompt : MonoBehaviour
    {
        private EchoCorridorPuzzle puzzle;
        private Transform player;
        private CanvasGroup group;
        private TMP_Text label;

        private void Start()
        {
            puzzle = GetComponent<EchoCorridorPuzzle>();
            var root = new GameObject("EchoQuestion", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            root.GetComponent<Canvas>().sortingOrder = 88;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            group = root.GetComponent<CanvasGroup>();
            group.blocksRaycasts = group.interactable = false;
            group.alpha = 0f;
            var card = new GameObject("Question", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(root.transform, false);
            var rect = (RectTransform)card.transform;
            // A wide "riddle" strip of its own: no crescents for the text to collide with, generous margins,
            // the round as a small spaced caption above the question. Sits above the hint banner's band.
            // Same place and width as the dialogue box (they never show together).
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .045f); rect.pivot = new Vector2(.5f, 0f);
            rect.sizeDelta = new Vector2(1260f, 170f); rect.anchoredPosition = Vector2.zero;
            var image = card.GetComponent<Image>();
            image.raycastTarget = false; image.color = new Color(.035f, .015f, .06f, .9f);
            MenuPanelStyle.FrameOnly(rect);
            var text = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            text.transform.SetParent(card.transform, false);
            label = text.GetComponent<TMP_Text>();
            label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(70, 16); label.rectTransform.offsetMax = new Vector2(-70, -16);
            label.enableAutoSizing = true; label.fontSizeMin = 22; label.fontSizeMax = 34;
            var book = Resources.Load<GameObject>("GrimoireRuntimeHost")?.GetComponent<MementoMori.World.GrimoireScreen>();
            label.font = book != null && book.BodyFont != null ? book.BodyFont : TMP_Settings.defaultFontAsset;
            label.fontSize = 29; label.alignment = TextAlignmentOptions.Center; label.lineSpacing = -6f;
            label.color = new Color(.83f, .75f, .89f);
            label.raycastTarget = false;
        }

        private void Update()
        {
            if (group == null || puzzle == null) return;
            if (player == null) player = FindAnyObjectByType<Player.PlayerController>()?.transform;
            // Never under a dialogue box or cutscene: the two texts overlapped at the bottom of the screen.
            var talking = DialogueManager.Instance != null && (DialogueManager.Instance.IsOpen || DialogueManager.Instance.IsCinematic);
            var show = player != null && puzzle.ContainsPlayer(player.position) && !puzzle.Solved && !talking
                && (InputGate.Instance == null || !InputGate.Instance.IsBlocked);
            group.alpha = Mathf.MoveTowards(group.alpha, show ? 1f : 0f, Time.unscaledDeltaTime * 4f);
            label.text = "<size=62%><cspace=4><color=#B9A3D6>" + Loc.T("ESCUTE OS CORREDORES") + " · " + (puzzle.Round + 1) + "/3</color></cspace></size>\n" + puzzle.Question;
        }
    }
}
