using System.IO;
using MementoMori.Dialogue;
using MementoMori.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MementoMori.EditorTools
{
    public static class UiBetaIntegrationEditor
    {
        private const string UiRoot = "Assets/Art/UI";
        private const string PrefabRoot = "Assets/Prefabs/UI";

        private static class UiLayout
        {
            public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
            public static readonly Vector2 MenuButtonSize = new Vector2(500f, 68f);
            public static readonly Vector2 PauseContainerSize = new Vector2(760f, 650f);
            public static readonly Vector2 DialogueSize = new Vector2(1320f, 440f);
            public static readonly Vector2 PromptSize = new Vector2(500f, 160f);
            public static readonly Vector2 ObjectiveSize = new Vector2(600f, 210f);

            public const float ButtonSpacing = 20f;
            public const float PauseTitleSize = 52f;
            public const float MenuButtonTextSize = 31f;
            public const float BodyTextSize = 29f;
        }

        private static class UiColors
        {
            public static readonly Color ClearText = new Color(.94f, .88f, .98f, 1f);
            public static readonly Color BodyText = new Color(.94f, .9f, .86f, 1f);
            public static readonly Color Dark = new Color(.03f, .018f, .04f, 1f);
            public static readonly Color Dimmer = new Color(.024f, .019f, .043f, .72f);
            public static readonly Color Ornament = new Color(.68f, .48f, .86f, .24f);
            public static readonly Color ButtonNormal = new Color(.18f, .12f, .25f, .74f);
        }

        [MenuItem("Memento Mori/Integrate Beta UI")]
        public static void Integrate()
        {
            Directory.CreateDirectory(PrefabRoot);
            ConfigureImports();
            var dialogue = BuildDialoguePrefab();
            var prompt = BuildPromptPrefab();
            var objective = BuildObjectivePrefab();
            var pause = BuildPausePrefab();
            var gameplay = BuildGameplayPrefab(dialogue, prompt, objective, pause);

            IntegrateGameplayScene("Assets/Scenes/Quarto.unity", gameplay);
            IntegrateGameplayScene("Assets/Scenes/Labirinto.unity", gameplay);
            IntegrateGameplayScene("Assets/Scenes/DominioLua.unity", gameplay);
            IntegrateMainMenu("Assets/Scenes/MainMenu.unity");
            IntegrateFinalBeta("Assets/Scenes/FinalBeta.unity");

            AssetDatabase.SaveAssets();
            Debug.Log("Beta UI integration complete.");
        }

        private static void ConfigureImports()
        {
            foreach (var path in Directory.GetFiles(UiRoot, "*.png", SearchOption.AllDirectories))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
                if (importer == null) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static GameObject BuildDialoguePrefab()
        {
            var root = CanvasRoot("DialogueUI", 90);
            var group = root.GetComponent<CanvasGroup>();
            var panel = ImageObj("DialogueBox", root.transform, S("Gameplay/11_dialogue_box.png"), false);
            Fixed(panel.rectTransform, new Vector2(.5f, .04f), new Vector2(.5f, 0f), Vector2.zero, UiLayout.DialogueSize);
            panel.type = Image.Type.Simple;
            panel.preserveAspect = true;

            var frame = ImageObj("PortraitFrame", panel.transform, S("Gameplay/12_portrait_frame.png"), true);
            Fixed(frame.rectTransform, new Vector2(0f, .5f), new Vector2(.5f, .5f), new Vector2(-20f, 0f), new Vector2(300f, 300f));
            var portrait = ImageObj("PortraitImage", frame.transform, null, true);
            Fixed(portrait.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(270f, 270f));
            portrait.color = new Color(1f, 1f, 1f, .25f);

            var speaker = Tmp("CharacterName_TMP", panel.transform, "MELANTHA", 30, TextAlignmentOptions.Center, TextStyle.Title);
            Fixed(speaker.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(150f, -91f), new Vector2(270f, 48f));
            var body = Tmp("DialogueText_TMP", panel.transform, "(Talvez eu encontre uma pista aqui...)", 30, TextAlignmentOptions.TopLeft, TextStyle.Body);
            Fixed(body.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(290f, -168f), new Vector2(950f, 205f));
            var cursor = ImageObj("ContinueCursor", panel.transform, S("Gameplay/17_dialogue_cursor_01.png"), true);
            Fixed(cursor.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-62f, 44f), new Vector2(38f, 38f));

            root.AddComponent<DialogueManager>().Configure(group, speaker, body, portrait, cursor.gameObject);
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            return SavePrefab(root, "DialogueUI");
        }

        private static GameObject BuildPromptPrefab()
        {
            var root = CanvasRoot("InteractionPromptUI", 85);
            var group = root.GetComponent<CanvasGroup>();
            var bg = ImageObj("PromptBackground", root.transform, S("Gameplay/14_interaction_prompt_bg.png"), false);
            Fixed(bg.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, -18f), UiLayout.PromptSize);
            bg.type = Image.Type.Simple;
            bg.preserveAspect = true;
            var key = ImageObj("Keycap_E", bg.transform, S("Gameplay/15_keycap_E.png"), true);
            Fixed(key.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(-170f, 0f), new Vector2(180f, 180f));
            var icon = ImageObj("InteractionIcon", bg.transform, S("Gameplay/16_interaction_hand_icon.png"), true);
            Fixed(icon.rectTransform, new Vector2(0f, .5f), new Vector2(0f, .5f), new Vector2(185f, 0f), new Vector2(58f, 70f));
            icon.gameObject.SetActive(false);
            var text = Tmp("ActionText_TMP", bg.transform, "Interagir", 22, TextAlignmentOptions.MidlineLeft, TextStyle.Body);
            Fixed(text.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(60f, 0f), new Vector2(300f, 78f));
            text.enableAutoSizing = true; text.fontSizeMin = 19; text.fontSizeMax = 22;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Truncate;
            root.AddComponent<InteractionPromptUI>().Configure(group, text);
            group.alpha = 0f;
            group.blocksRaycasts = false;
            return SavePrefab(root, "InteractionPromptUI");
        }

        private static GameObject BuildObjectivePrefab()
        {
            var root = CanvasRoot("ObjectiveUI", 82);
            var group = root.GetComponent<CanvasGroup>();
            var panel = ImageObj("ObjectivePanel", root.transform, S("Gameplay/13_objective_panel.png"), false);
            Fixed(panel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-50f, -45f), UiLayout.ObjectiveSize);
            panel.type = Image.Type.Simple;
            panel.preserveAspect = true;
            var title = Tmp("Title_TMP", panel.transform, "OBJETIVO", 27, TextAlignmentOptions.Center, TextStyle.Title);
            Fixed(title.rectTransform, new Vector2(.5f, 1f), new Vector2(.5f, 1f), new Vector2(0f, -55f), new Vector2(450f, 42f));
            var text = Tmp("ObjectiveText_TMP", panel.transform, "Encontre uma forma de sair\ndo Quarto", 29, TextAlignmentOptions.Center, TextStyle.Body);
            Fixed(text.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, -27f), new Vector2(455f, 80f));
            var controller = root.AddComponent<ObjectiveToastController>();
            typeof(ObjectiveToastController).GetField("canvasGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, group);
            typeof(ObjectiveToastController).GetField("objectiveTextTmp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, text);
            group.alpha = 0f;
            group.blocksRaycasts = false;
            return SavePrefab(root, "ObjectiveUI");
        }

        private static GameObject BuildPausePrefab()
        {
            var root = CanvasRoot("PauseUI", 100);
            var dimmer = ImageObj("Dimmer", root.transform, null, false);
            Stretch(dimmer.rectTransform, 0f, 0f, 1f, 1f);
            dimmer.color = UiColors.Dimmer;
            dimmer.raycastTarget = true;

            var container = RectObj("PauseContainer", root.transform);
            Fixed(container, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, -15f), UiLayout.PauseContainerSize);

            var ornament = ImageObj("BackgroundOrnament", container, S("MainMenu/08_menu_center_sigil.png"), true);
            Fixed(ornament.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, 0f), new Vector2(500f, 500f));
            ornament.color = UiColors.Ornament;

            var title = Tmp("Title", container, "PAUSA", (int)UiLayout.PauseTitleSize, TextAlignmentOptions.Center, TextStyle.Display);
            Fixed(title.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, 235f), new Vector2(600f, 85f));

            var top = ImageObj("TopDecoration", container, S("MainMenu/07_menu_divider_ornament.png"), true);
            Fixed(top.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, 162f), new Vector2(390f, 38f));

            var buttons = RectObj("ButtonsContainer", container);
            Fixed(buttons, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, -28f), new Vector2(520f, 244f));
            var layout = buttons.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = UiLayout.ButtonSpacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            ButtonObj("Button_Continuar", buttons, "CONTINUAR", UiLayout.MenuButtonSize, 31, true);
            ButtonObj("Button_Reiniciar checkpoint", buttons, "REINICIAR CHECKPOINT", UiLayout.MenuButtonSize, 27, true);
            ButtonObj("Button_Menu principal", buttons, "MENU PRINCIPAL", UiLayout.MenuButtonSize, 27, true);

            var bottom = ImageObj("BottomDecoration", container, S("MainMenu/07_menu_divider_ornament.png"), true);
            Fixed(bottom.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, -224f), new Vector2(390f, 38f));

            root.AddComponent<PauseMenuController>().ConfigurePanel(container.gameObject);
            container.gameObject.SetActive(false);
            return SavePrefab(root, "PauseUI");
        }

        private static GameObject BuildGameplayPrefab(GameObject dialogue, GameObject prompt, GameObject objective, GameObject pause)
        {
            var root = CanvasRoot("GameplayUI", 80);
            InstantiatePrefab(dialogue, root.transform, "DialogueUI");
            InstantiatePrefab(prompt, root.transform, "InteractionPromptUI");
            InstantiatePrefab(objective, root.transform, "ObjectiveUI");
            InstantiatePrefab(pause, root.transform, "PauseUI");
            return SavePrefab(root, "GameplayUI");
        }

        private static void IntegrateGameplayScene(string scenePath, GameObject gameplayPrefab)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            foreach (var existing in Object.FindObjectsByType<DialogueManager>(FindObjectsInactive.Include))
                Object.DestroyImmediate(existing.gameObject);
            foreach (var existing in Object.FindObjectsByType<InteractionPromptUI>(FindObjectsInactive.Include))
                Object.DestroyImmediate(existing.gameObject);
            foreach (var existing in Object.FindObjectsByType<PauseMenuController>(FindObjectsInactive.Include))
                Object.DestroyImmediate(existing.gameObject);
            foreach (var existing in Object.FindObjectsByType<ObjectiveToastController>(FindObjectsInactive.Include))
                Object.DestroyImmediate(existing.gameObject);
            foreach (var existing in GameObjectsNamed("GameplayCanvas"))
                Object.DestroyImmediate(existing);
            foreach (var existing in GameObjectsNamed("GameplayUI"))
                Object.DestroyImmediate(existing);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(gameplayPrefab, scene);
            instance.name = "GameplayUI";
            WirePause(instance);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void IntegrateMainMenu(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            foreach (var existing in GameObjectsNamed("MainMenuCanvas"))
                Object.DestroyImmediate(existing);
            foreach (var existing in Object.FindObjectsByType<MainMenuController>(FindObjectsInactive.Include))
                Object.DestroyImmediate(existing.gameObject);

            var canvas = CanvasRoot("MainMenuCanvas", 10);
            MoveToScene(canvas, scene);
            var placeholder = ImageObj("MainMenuBackgroundPlaceholder", canvas.transform, null, false);
            Stretch(placeholder.rectTransform, 0f, 0f, 1f, 1f);
            placeholder.color = UiColors.Dark;

            var sigil = ImageObj("CenterSigil", canvas.transform, S("MainMenu/08_menu_center_sigil.png"), true);
            Fixed(sigil.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, -86f), new Vector2(500f, 500f));
            sigil.color = new Color(.68f, .48f, .86f, .18f);

            var moon = ImageObj("MoonEmblem", canvas.transform, S("MainMenu/06_menu_moon_emblem.png"), true);
            Fixed(moon.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, 318f), new Vector2(260f, 165f));
            var logo = ImageObj("Logo_MementoMoriBeta", canvas.transform, S("MainMenu/01_menu_logo_beta.png"), true);
            Fixed(logo.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, 185f), new Vector2(760f, 220f));

            var buttons = RectObj("MainMenuButtons", canvas.transform);
            Fixed(buttons, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, -145f), new Vector2(520f, 244f));
            var layout = buttons.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var controllerGo = new GameObject("MainMenuController");
            MoveToScene(controllerGo, scene);
            var controller = controllerGo.AddComponent<MainMenuController>();
            var credits = CreditsPanel(canvas.transform);
            controller.ConfigureCredits(credits);
            MenuButton(buttons, "Button_Jogar", "JOGAR", controller, nameof(MainMenuController.Play));
            MenuButton(buttons, "Button_Creditos", "CRÉDITOS", controller, nameof(MainMenuController.ShowCredits));
            MenuButton(buttons, "Button_Sair", "SAIR", controller, nameof(MainMenuController.Quit));
            MenuButton(credits.transform, "Button_Voltar", "VOLTAR", controller, nameof(MainMenuController.HideCredits));
            credits.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void IntegrateFinalBeta(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var canvas = Object.FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas == null) return;
            ConfigureExistingCanvas(canvas, 10);

            foreach (var name in new[] { "FinalBetaUI", "FinalPanel_Visual", "FinalCenterEmblem", "FinalFragmentLarge", "FinalLogo_Visual", "FinalBetaSubtitle_TMP", "FinalQuote_TMP", "FinalThanks_TMP", "FinalBeta_BorderTop", "FinalBeta_BorderBottom", "FinalBeta_BorderLeft", "FinalBeta_BorderRight" })
                foreach (var existing in GameObjectsNamed(name))
                    Object.DestroyImmediate(existing);

            var finalText = FindFinalText();
            var returnButton = FindReturnButton();
            DestroyUnusedFinalLabels(finalText, returnButton);

            var root = RectObj("FinalBetaUI", canvas.transform);
            Stretch(root, 0f, 0f, 1f, 1f);
            var background = ImageObj("Background", root, S("Screens/25_final_text_panel.png"), false);
            Fixed(background.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(920f, 680f));
            background.color = new Color(.03f, .025f, .05f, .78f);
            var logo = ImageObj("FinalTitle", root, S("MainMenu/01_menu_logo_beta.png"), true);
            Fixed(logo.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, 300f), new Vector2(700f, 190f));
            var subtitle = Tmp("FinalBetaSubtitle_TMP", root, "FinalBeta", 30, TextAlignmentOptions.Center, TextStyle.Title);
            Fixed(subtitle.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, 204f), new Vector2(420f, 46f));
            var emblem = ImageObj("CentralOrnament", root, S("Screens/26_final_center_emblem.png"), true);
            Fixed(emblem.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, 72f), new Vector2(360f, 260f));
            var fragment = ImageObj("FinalFragmentLarge", root, S("Screens/27_final_fragment_large.png"), true);
            Fixed(fragment.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, 66f), new Vector2(110f, 150f));

            if (finalText != null)
            {
                finalText.transform.SetParent(root, false);
                finalText.text = "ALGUMAS COISAS NÃO TERMINAM.\nELAS APENAS MUDAM DE LUGAR.\n\nOBRIGADO POR CARREGAR ESTA MEMÓRIA ATÉ AQUI.";
                finalText.fontSize = 28;
                finalText.color = UiColors.BodyText;
                finalText.alignment = TextAnchor.MiddleCenter;
                finalText.horizontalOverflow = HorizontalWrapMode.Wrap;
                finalText.verticalOverflow = VerticalWrapMode.Truncate;
                Fixed(finalText.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, -176f), new Vector2(780f, 170f));
            }

            if (returnButton != null)
            {
                returnButton.transform.SetParent(root, false);
                Fixed(returnButton.GetComponent<RectTransform>(), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, -302f), UiLayout.MenuButtonSize);
                StyleButton(returnButton);
                var label = returnButton.GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.text = "VOLTAR AO MENU";
                    label.fontSize = 24;
                    label.color = UiColors.ClearText;
                    label.alignment = TextAnchor.MiddleCenter;
                    label.horizontalOverflow = HorizontalWrapMode.Wrap;
                    label.verticalOverflow = VerticalWrapMode.Truncate;
                }
                var tmp = returnButton.GetComponentInChildren<TMP_Text>(true);
                if (tmp != null)
                {
                    tmp.text = "VOLTAR AO MENU";
                    ApplyTextStyle(tmp, 26, TextAlignmentOptions.Center, TextStyle.Title);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void WirePause(GameObject root)
        {
            var pause = root.GetComponentInChildren<PauseMenuController>(true);
            var buttons = root.GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
            {
                button.onClick = new Button.ButtonClickedEvent();
                if (button.name.Contains("Continuar")) AddCall(button, pause, nameof(PauseMenuController.Toggle));
                else if (button.name.Contains("Reiniciar")) AddCall(button, pause, nameof(PauseMenuController.RestartScene));
                else if (button.name.Contains("Menu")) AddCall(button, pause, nameof(PauseMenuController.ReturnToMenu));
                StyleButton(button);
            }
        }

        private static GameObject CanvasRoot(string name, int order)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            ConfigureExistingCanvas(root.GetComponent<Canvas>(), order);
            return root;
        }

        private static void ConfigureExistingCanvas(Canvas canvas, int order)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = canvas.GetComponent<CanvasScaler>() ?? canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = UiLayout.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            if (canvas.GetComponent<GraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        private static RectTransform RectObj(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one;
            return go.GetComponent<RectTransform>();
        }

        private static Image ImageObj(string name, Transform parent, Sprite sprite, bool preserveAspect)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one;
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            image.preserveAspect = sprite != null && preserveAspect;
            return image;
        }

        private enum TextStyle
        {
            Body,
            Title,
            Display
        }

        private static TMP_Text Tmp(string name, Transform parent, string text, int size, TextAlignmentOptions align, TextStyle style = TextStyle.Body)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one;
            var tmp = go.GetComponent<TMP_Text>();
            tmp.text = text;
            ApplyTextStyle(tmp, size, align, style);
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void ApplyTextStyle(TMP_Text tmp, int size, TextAlignmentOptions align, TextStyle style)
        {
            tmp.enableAutoSizing = false;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = style == TextStyle.Body ? UiColors.BodyText : UiColors.ClearText;
            tmp.fontStyle = style == TextStyle.Body ? FontStyles.Normal : FontStyles.Bold;
            tmp.characterSpacing = style == TextStyle.Display ? 8f : style == TextStyle.Title ? 6f : 1f;
            tmp.wordSpacing = style == TextStyle.Body ? 2f : 4f;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
        }

        private static Button ButtonObj(string name, Transform parent, string text, Vector2 size, int textSize, bool framed)
        {
            var image = ImageObj(name, parent, S("MainMenu/02_button_frame_normal.png"), false);
            image.type = Image.Type.Sliced;
            image.raycastTarget = true;
            image.color = framed ? Color.white : UiColors.ButtonNormal;
            image.rectTransform.sizeDelta = size;
            image.rectTransform.localScale = Vector3.one;
            var layout = image.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;
            layout.minWidth = size.x;
            layout.minHeight = size.y;
            var button = image.gameObject.AddComponent<Button>();
            StyleButton(button);
            var label = Tmp("Label_TMP", image.transform, text, textSize, TextAlignmentOptions.Center, TextStyle.Title);
            Stretch(label.rectTransform, .08f, .12f, .92f, .88f);
            if (text.Length > 14)
            {
                label.fontSize = Mathf.Min(textSize, 26);
                label.characterSpacing = 2f;
            }
            return button;
        }

        private static void MenuButton(Transform parent, string name, string text, Object target, string method)
        {
            var button = ButtonObj(name, parent, text, UiLayout.MenuButtonSize, (int)UiLayout.MenuButtonTextSize, true);
            AddCall(button, target, method);
            AddButtonDecorations(button.transform);
        }

        private static GameObject CreditsPanel(Transform parent)
        {
            var root = new GameObject("Credits", typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(parent, false);
            Stretch(root.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f);
            var panel = ImageObj("CreditsPanel", root.transform, S("Screens/23_credits_panel.png"), false);
            Fixed(panel.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(820f, 560f));
            panel.type = Image.Type.Sliced;
            var title = ImageObj("CreditsTitleFrame", panel.transform, S("Screens/24_credits_title_frame.png"), true);
            Fixed(title.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, 182f), new Vector2(420f, 70f));
            var titleText = Tmp("Title_TMP", title.transform, "CRÉDITOS", 32, TextAlignmentOptions.Center, TextStyle.Title);
            Stretch(titleText.rectTransform, .08f, .08f, .92f, .92f);
            var body = Tmp("Body_TMP", panel.transform, "Memento Mori Beta\n\nDesenvolvimento, narrativa e direção visual\nCallisto / Monespira", 24, TextAlignmentOptions.Center, TextStyle.Body);
            Fixed(body.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, 20f), new Vector2(620f, 190f));
            return root;
        }

        private static void AddButtonDecorations(Transform button)
        {
            var left = ImageObj("SelectorLeft", button, S("MainMenu/05_menu_selector_diamond.png"), true);
            Fixed(left.rectTransform, new Vector2(0f, .5f), new Vector2(.5f, .5f), new Vector2(-35f, 0f), new Vector2(32f, 32f));
            var right = ImageObj("SelectorRight", button, S("MainMenu/05_menu_selector_diamond.png"), true);
            Fixed(right.rectTransform, new Vector2(1f, .5f), new Vector2(.5f, .5f), new Vector2(35f, 0f), new Vector2(32f, 32f));
            var glow = ImageObj("GlowOverlay", button, S("MainMenu/10_button_glow_overlay.png"), false);
            Stretch(glow.rectTransform, 0f, 0f, 1f, 1f);
            glow.color = new Color(1f, 1f, 1f, .16f);
        }

        private static void StyleButton(Button button)
        {
            var image = button.GetComponent<Image>();
            if (image == null) return;
            image.sprite = S("MainMenu/02_button_frame_normal.png");
            image.type = Image.Type.Sliced;
            image.raycastTarget = true;
            image.color = Color.white;
            var state = button.spriteState;
            state.highlightedSprite = S("MainMenu/03_button_frame_hover.png");
            state.pressedSprite = S("MainMenu/04_button_frame_pressed.png");
            state.selectedSprite = state.highlightedSprite;
            state.disabledSprite = S("MainMenu/02_button_frame_normal.png");
            button.spriteState = state;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.selectedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(.78f, .68f, .88f, 1f);
            colors.disabledColor = new Color(1f, 1f, 1f, .42f);
            button.colors = colors;
            button.transition = Selectable.Transition.SpriteSwap;
        }

        private static Text FindFinalText()
        {
            foreach (var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include))
                if (text.GetComponentInParent<Button>() == null)
                    return text;
            return null;
        }

        private static Button FindReturnButton()
        {
            foreach (var button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include))
                if (button.name.ToLowerInvariant().Contains("return") || button.name.ToLowerInvariant().Contains("menu"))
                    return button;
            return Object.FindAnyObjectByType<Button>(FindObjectsInactive.Include);
        }

        private static void DestroyUnusedFinalLabels(Text finalText, Button returnButton)
        {
            foreach (var tmp in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include))
                if (tmp.GetComponentInParent<Button>() == null)
                    Object.DestroyImmediate(tmp.gameObject);

            foreach (var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include))
            {
                if (text == finalText || text.GetComponentInParent<Button>() != null)
                    continue;
                Object.DestroyImmediate(text.gameObject);
            }

            foreach (var button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include))
            {
                if (button == returnButton)
                    continue;
                if (button.name.StartsWith("Button_Voltar", System.StringComparison.Ordinal) || button.name.StartsWith("Button_Return", System.StringComparison.Ordinal))
                    Object.DestroyImmediate(button.gameObject);
            }
        }

        private static void AddCall(Button button, Object target, string method)
        {
            var action = (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), target, method);
            UnityEventTools.AddPersistentListener(button.onClick, action);
        }

        private static Sprite S(string relativePath) => AssetDatabase.LoadAssetAtPath<Sprite>($"{UiRoot}/{relativePath}");

        private static GameObject SavePrefab(GameObject source, string name)
        {
            var path = $"{PrefabRoot}/{name}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(source, path);
            Object.DestroyImmediate(source);
            return prefab;
        }

        private static void InstantiatePrefab(GameObject prefab, Transform parent, string name)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localScale = Vector3.one;
        }

        private static void MoveToScene(GameObject go, Scene scene) => SceneManager.MoveGameObjectToScene(go, scene);

        private static GameObject[] GameObjectsNamed(string name)
        {
            return System.Array.FindAll(Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include), go => go.name == name && go.scene.IsValid());
        }

        private static void Fixed(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
        }

        private static void Stretch(RectTransform rect, float minX, float minY, float maxX, float maxY)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }
    }
}
