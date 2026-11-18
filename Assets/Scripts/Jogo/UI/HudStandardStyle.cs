using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MementoMori.UI
{
    /// <summary>
    /// One look for every in-game box: the dialogue box, its portrait frame and the objective panel use the same
    /// dark fill, double lilac rule and star corners as the pause, map, puzzle, hint and choice panels, with the
    /// game's heading font for names/titles and body font for text. Layout is re-asserted each frame because the
    /// dialogue and objective controllers resize their parts as lines and objectives change.
    /// </summary>
    public sealed class HudStandardStyle : MonoBehaviour
    {
        static readonly Color Fill = new(.035f, .015f, .06f, .92f);
        RectTransform box, portrait, portraitImage, speaker, body, objective, objectiveTitle, objectiveText;
        bool styled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register() { SceneManager.sceneLoaded -= Attach; SceneManager.sceneLoaded += Attach; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AttachOpenScene() => Attach(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        static void Attach(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu" || FindAnyObjectByType<HudStandardStyle>() != null) return;
            var host = new GameObject("HudStandardStyle");
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<HudStandardStyle>();
        }

        static RectTransform Find(string name)
        {
            foreach (var rect in Resources.FindObjectsOfTypeAll<RectTransform>())
                if (rect.name == name && rect.gameObject.scene.IsValid()) return rect;
            return null;
        }

        static void Plate(RectTransform rect)
        {
            var image = rect.GetComponent<Image>();
            if (image != null) { image.sprite = null; image.color = Fill; image.preserveAspect = false; }
            ClearOldFrames(rect);
            MenuPanelStyle.FrameOnly(rect);
        }

        /// <summary>Removes the earlier straight-line frames (holders without an image) baked into prefabs.</summary>
        public static void ClearOldFrames(Transform rect)
        {
            for (var i = rect.childCount - 1; i >= 0; i--)
            {
                var child = rect.GetChild(i);
                if (child.name == "Moldura" && child.GetComponent<Image>() == null) Destroy(child.gameObject);
            }
        }

        // Ornaments that make the plain frame read as a dialogue box: an emblem on the top rule, a name tab that
        // breaks the top border at the left, small diamonds closing the tab.
        RectTransform nameTab;
        void Details(RectTransform target)
        {
            if (target.Find("Emblema") == null)
            {
                var emblem = MenuPanelStyle.NewImage("Emblema", target, MenuPanelStyle.Diamond(), MenuPanelStyle.Lilac);
                emblem.anchorMin = emblem.anchorMax = new Vector2(.5f, 1f); emblem.sizeDelta = new Vector2(30f, 30f);
                var core = MenuPanelStyle.NewImage("EmblemaCentro", emblem, MenuPanelStyle.Diamond(), new Color(.035f, .015f, .06f, 1f));
                core.anchorMin = core.anchorMax = new Vector2(.5f, .5f); core.sizeDelta = new Vector2(14f, 14f);
                foreach (var side in new[] { -1f, 1f })
                {
                    var dot = MenuPanelStyle.NewImage("EmblemaPonto", target, MenuPanelStyle.Diamond(), MenuPanelStyle.LilacSoft);
                    dot.anchorMin = dot.anchorMax = new Vector2(.5f, 1f); dot.sizeDelta = new Vector2(12f, 12f); dot.anchoredPosition = new Vector2(34f * side, 0f);
                }
            }
            if (speaker != null && nameTab == null)
            {
                nameTab = MenuPanelStyle.NewImage("AbaNome", target, null, new Color(.06f, .025f, .1f, 1f));
                nameTab.SetSiblingIndex(speaker.GetSiblingIndex());
                MenuPanelStyle.FrameOnly(nameTab);
            }
        }

        void LateUpdate()
        {
            if (!styled)
            {
                box = Find("DialogueBox"); portrait = Find("PortraitFrame"); portraitImage = Find("PortraitImage");
                speaker = Find("CharacterName_TMP"); body = Find("DialogueText_TMP");
                objective = Find("ObjectivePanel"); objectiveTitle = Find("Title_TMP"); objectiveText = Find("ObjectiveText_TMP");
                if (box == null && objective == null) return;
                // Stars baked into the prefabs lose their generated diamond sprite on save: give it back.
                foreach (var star in Resources.FindObjectsOfTypeAll<Image>())
                    if (star.name == "Estrela" && star.sprite == null && star.gameObject.scene.IsValid()) star.sprite = MenuPanelStyle.Diamond();
                if (box != null) { Plate(box); Details(box); }
                if (portrait != null)
                {
                    Plate(portrait);
                    var back = portrait.GetComponent<Image>(); if (back != null) back.color = new Color(.85f, .78f, 1f, 1f);
                }
                if (objective != null) Plate(objective);
                if (speaker != null && speaker.GetComponent<TMP_Text>() is TMP_Text name)
                {
                    // The speaker's name stands out: larger, bright lilac with a soft glow, underlined by a rule.
                    name.font = MenuPanelStyle.HeadingFont; name.fontSize = 34f; name.characterSpacing = 6f;
                    name.color = new Color(.9f, .76f, 1f); name.alignment = TextAlignmentOptions.BottomLeft;
                    var old = speaker.Find("Linha"); if (old != null) Destroy(old.gameObject);
                    name.enableAutoSizing = false; name.fontStyle = FontStyles.UpperCase;
                }
                if (body != null && body.GetComponent<TMP_Text>() is TMP_Text text)
                {
                    text.font = MenuPanelStyle.BodyFont; text.fontSize = 40f; text.color = MenuPanelStyle.TitleColor;
                    text.alignment = TextAlignmentOptions.TopLeft; text.lineSpacing = -4f;
                }
                if (objectiveTitle != null && objectiveTitle.GetComponent<TMP_Text>() is TMP_Text title)
                { title.font = MenuPanelStyle.HeadingFont; title.color = MenuPanelStyle.CaptionColor; title.characterSpacing = 8f; }
                if (objectiveText != null && objectiveText.GetComponent<TMP_Text>() is TMP_Text goal)
                { goal.font = MenuPanelStyle.BodyFont; goal.fontSize = 31f; goal.fontSizeMax = 31f; goal.fontSizeMin = 26f; goal.enableAutoSizing = true; goal.color = MenuPanelStyle.TitleColor; goal.fontStyle = FontStyles.Normal; }
                styled = true;
            }
            if (box != null)
            {
                box.anchorMin = box.anchorMax = new Vector2(.5f, .045f); box.pivot = new Vector2(.5f, 0f);
                box.sizeDelta = new Vector2(1260f, 250f); box.anchoredPosition = Vector2.zero;
                var hasFace = portrait != null && portrait.gameObject.activeSelf;
                var left = hasFace ? .23f : .05f;
                if (portrait != null)
                {
                    portrait.anchorMin = portrait.anchorMax = new Vector2(0f, .5f); portrait.pivot = new Vector2(.5f, .5f);
                    portrait.sizeDelta = new Vector2(236f, 236f); portrait.anchoredPosition = new Vector2(144f, 0f);
                }
                // The face fills its frame (it floated small in a big empty square), on a soft lilac backdrop.
                if (portraitImage != null) { portraitImage.anchorMin = portraitImage.anchorMax = new Vector2(.5f, .5f); portraitImage.pivot = new Vector2(.5f, .5f); portraitImage.sizeDelta = new Vector2(218f, 218f); portraitImage.anchoredPosition = Vector2.zero; }
                var speakerText = speaker != null ? speaker.GetComponent<TMP_Text>() : null;
                var named = speakerText != null && speaker.gameObject.activeInHierarchy && !string.IsNullOrWhiteSpace(speakerText.text);
                if (speaker != null)
                {
                    speakerText.alignment = TextAlignmentOptions.Center;
                    speakerText.ForceMeshUpdate();
                    var width = Mathf.Clamp(speakerText.preferredWidth + 70f, 200f, 520f);
                    speaker.anchorMin = speaker.anchorMax = new Vector2(left, 1f); speaker.pivot = new Vector2(0f, .5f);
                    speaker.sizeDelta = new Vector2(width, 58f); speaker.anchoredPosition = new Vector2(10f, 0f);
                    if (nameTab != null)
                    {
                        nameTab.gameObject.SetActive(named);
                        nameTab.anchorMin = nameTab.anchorMax = speaker.anchorMin; nameTab.pivot = speaker.pivot;
                        nameTab.sizeDelta = speaker.sizeDelta; nameTab.anchoredPosition = speaker.anchoredPosition;
                        nameTab.SetSiblingIndex(Mathf.Max(0, speaker.GetSiblingIndex() - 1));
                    }
                }
                if (body != null)
                {
                    var tutorial = FindAnyObjectByType<RoomTutorialController>();
                    var bottom = tutorial != null && tutorial.ShowsDialogueControls ? .22f : .08f;
                    body.anchorMin = new Vector2(left, bottom); body.anchorMax = new Vector2(.95f, .9f);
                    body.pivot = new Vector2(0f, .5f); body.offsetMin = new Vector2(28f, 0f); body.offsetMax = new Vector2(-28f, 0f);
                    if (body.GetComponent<TMP_Text>() is TMP_Text line) { line.enableAutoSizing = false; line.alignment = TextAlignmentOptions.MidlineLeft; line.useMaxVisibleDescender = false; }
                }
            }
            if (objective != null)
            {
                objective.sizeDelta = new Vector2(600f, 165f);
                if (objectiveTitle != null)
                {
                    objectiveTitle.anchorMin = new Vector2(.06f, 1f); objectiveTitle.anchorMax = new Vector2(.94f, 1f);
                    objectiveTitle.pivot = new Vector2(.5f, 1f); objectiveTitle.sizeDelta = new Vector2(0f, 44f);
                    objectiveTitle.anchoredPosition = new Vector2(0f, -18f);
                }
                if (objectiveText != null)
                {
                    // Centre in the remaining content area below the heading, not in the whole card.
                    objectiveText.anchorMin = Vector2.zero; objectiveText.anchorMax = Vector2.one;
                    var headingBottom = objectiveTitle != null ? -objectiveTitle.anchoredPosition.y + objectiveTitle.rect.height : 62f;
                    const float contentPadding = 14f;
                    objectiveText.offsetMin = new Vector2(42f, contentPadding);
                    objectiveText.offsetMax = new Vector2(-42f, -(headingBottom + contentPadding));
                    var goal = objectiveText.GetComponent<TMP_Text>();
                    if (goal != null) { goal.alignment = TextAlignmentOptions.Center; goal.lineSpacing = 0f; }
                }
            }
        }
    }
}
