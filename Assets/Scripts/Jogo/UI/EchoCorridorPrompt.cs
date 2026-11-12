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
            root.GetComponent<Canvas>().sortingOrder = 110;
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
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .15f);
            rect.sizeDelta = new Vector2(1000, 150);
            card.GetComponent<Image>().color = new Color(.025f, .012f, .045f, .86f);
            card.GetComponent<Image>().raycastTarget = false;
            var text = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            text.transform.SetParent(card.transform, false);
            label = text.GetComponent<TMP_Text>();
            label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(28, 12); label.rectTransform.offsetMax = new Vector2(-28, -12);
            var book = Resources.Load<GameObject>("GrimoireRuntimeHost")?.GetComponent<MementoMori.World.GrimoireScreen>();
            label.font = book != null && book.BodyFont != null ? book.BodyFont : TMP_Settings.defaultFontAsset;
            label.fontSize = 27; label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(.83f, .75f, .89f);
            label.raycastTarget = false;
        }

        private void Update()
        {
            if (group == null || puzzle == null) return;
            if (player == null) player = FindAnyObjectByType<Player.PlayerController>()?.transform;
            var show = player != null && puzzle.ContainsPlayer(player.position) && !puzzle.Solved
                && (InputGate.Instance == null || !InputGate.Instance.IsBlocked);
            group.alpha = Mathf.MoveTowards(group.alpha, show ? 1f : 0f, Time.unscaledDeltaTime * 4f);
            label.text = "<size=21>" + Loc.T("Escute os corredores") + " · " + (puzzle.Round + 1) + "/3</size>\n" + puzzle.Question;
        }
    }
}
