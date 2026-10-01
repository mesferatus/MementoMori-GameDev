using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

namespace MementoMori.World
{
    /// <summary>Physical Quarto interaction and persistent four-tab grimoire interface.</summary>
    public sealed class GrimoireScreen : MonoBehaviour, IInteractable
    {
        private const string Gate = "Grimoire";
        private const int RowsPerListPage = 7;
        private static GrimoireScreen runtime;
        private static readonly string[] TabTitles = { "ANOTAÇÕES", "RITUAIS", "PISTAS", "MEMÓRIAS" };

        [SerializeField] private Sprite bookSprite;
        [SerializeField] private Sprite illustrationSprite;
        [SerializeField] private Sprite noteSprite;
        [SerializeField] private Sprite symbolsSprite;
        [SerializeField] private Sprite closeFrameSprite;
        [SerializeField] private TMP_FontAsset titleFont;
        [SerializeField] private TMP_FontAsset bodyFont;
        private Canvas canvas;
        private TMP_Text pageTitle;
        private TMP_Text pageBody;
        private TMP_Text pageNumber;
        private TMP_Text listNumber;
        private TMP_Text emptyLabel;
        private TMP_Text noteText;
        private TMP_Text noticeText;
        private Image noteImage;
        private Image pageIllustration;
        private Image pageSymbols;
        private Button[] entryButtons;
        private Button previousPage, nextPage, previousList, nextList;
        private Image[] tabFrames;
        private TMP_Text indexTitle;
        private Sprite tabFrameNormal;
        private Sprite tabFrameSelected;
        private Sprite tabFrameHover;
        private Sprite tabFrameLocked;
        private Sprite entryNormal;
        private Sprite entrySelected;
        private Sprite leftArrow;
        private Sprite rightArrow;
        private int page;
        private int pageCount = 1;
        private int listPage;
        private int activeTab;
        private readonly List<GrimoireCatalog.Entry> visibleEntries = new();
        private GrimoireCatalog.Entry selectedEntry;
        private float noticeUntil;
        private int lastUnlockedCount;
        private bool opening;
        private bool pendingFirstOpen;
        private bool runtimeHost;
        private readonly Dictionary<Canvas, bool> hiddenCanvases = new();
        public bool IsOpen { get; private set; }
        public static int EscapeConsumedFrame { get; private set; } = -1;
        public static bool AnyOpen { get; private set; }
        public string InteractionVerb => "Ler o grimório";
        public int InteractionPriority => 30;
        public bool CanInteract(InteractionContext context) => !AnyOpen && !opening && !pendingFirstOpen;

        public void Configure(Sprite book, TMP_FontAsset heading, TMP_FontAsset body)
        { bookSprite = book; titleFont = heading; bodyFont = body; }
        public void ConfigureDecorations(Sprite illustration, Sprite note, Sprite symbols)
        { illustrationSprite = illustration; noteSprite = note; symbolsSprite = symbols; }
        public void ConfigureCloseFrame(Sprite frame) { closeFrameSprite = frame; }

        public void Interact(InteractionContext context)
        {
            if (!CanInteract(context)) return;
            var dialogue = Resources.Load<DialogueData>("Dialogue/DLG_Q_GRIMOIRE_01");
            if (DialogueManager.Instance == null || dialogue == null) { OpenFirstTime(); return; }
            opening = true;
            void Continue()
            {
                DialogueManager.Instance.OnDialogueCompleted -= Continue;
                opening = false;
                OpenFirstTime();
            }
            DialogueManager.Instance.OnDialogueCompleted += Continue;
            DialogueManager.Instance.StartDialogue(dialogue);
        }

        private void Update()
        {
            if (!runtimeHost) return;
            var unlocked = 0;
            foreach (var entry in GrimoireCatalog.Entries)
                if (GrimoireCatalog.IsUnlocked(entry.Id)) unlocked++;
            if (unlocked > lastUnlockedCount) ShowNotice();
            lastUnlockedCount = unlocked;
            if (noticeText != null && noticeText.gameObject.activeSelf && Time.unscaledTime >= noticeUntil)
                noticeText.gameObject.SetActive(false);
            if (!IsOpen)
            {
                if (Input.GetKeyDown(KeyCode.G) && GameState.Instance != null
                    && GameState.Instance.HasFlag(StoryFlag.RoomGrimoireRead)) Open();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q)) { Close(); return; }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) ShowPage(page + 1);
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) ShowPage(page - 1);
        }

        public void Open()
        {
            if (!runtimeHost) { EnsureRuntime().Open(); return; }
            if (IsOpen || Time.timeScale == 0f || DialogueManager.Instance != null && DialogueManager.Instance.IsOpen
                || InputGate.Instance != null && InputGate.Instance.IsBlocked) return;
            if (canvas == null) BuildUi();
            canvas.gameObject.SetActive(true);
            SelectTab(activeTab);
            IsOpen = true;
            AnyOpen = true;
            InputGate.Instance?.Block(Gate);
            HideOtherCanvases();
        }

        private void OpenFirstTime()
        {
            GameState.Instance?.SetFlag(StoryFlag.RoomGrimoireRead);
            EnsureRuntime();
            if (!pendingFirstOpen) StartCoroutine(OpenWhenAvailable());
        }

        private IEnumerator OpenWhenAvailable()
        {
            pendingFirstOpen = true;
            while (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen
                || InputGate.Instance != null && InputGate.Instance.IsBlocked
                || Time.timeScale == 0f)
                yield return null;
            pendingFirstOpen = false;
            if (GameState.Instance != null && GameState.Instance.HasFlag(StoryFlag.RoomGrimoireRead))
                runtime?.Open();
        }

        private GrimoireScreen EnsureRuntime()
        {
            if (runtime != null) return runtime;
            var host = new GameObject("PersistentGrimoireUI");
            DontDestroyOnLoad(host);
            runtime = host.AddComponent<GrimoireScreen>();
            runtime.runtimeHost = true;
            runtime.bookSprite = bookSprite;
            runtime.illustrationSprite = illustrationSprite;
            runtime.noteSprite = noteSprite;
            runtime.symbolsSprite = symbolsSprite;
            runtime.closeFrameSprite = closeFrameSprite;
            runtime.titleFont = titleFont;
            runtime.bodyFont = bodyFont;
            SceneManager.sceneLoaded += runtime.OnSceneLoaded;
            GrimoireCatalog.OnSceneEntered(SceneManager.GetActiveScene().name);
            return runtime;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => GrimoireCatalog.OnSceneEntered(scene.name);

        private void HideOtherCanvases()
        {
            hiddenCanvases.Clear();
            foreach (var other in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (other == canvas || other.transform.IsChildOf(canvas.transform)) continue;
                hiddenCanvases[other] = other.enabled;
                other.enabled = false;
            }
        }

        private void RestoreOtherCanvases()
        {
            foreach (var pair in hiddenCanvases)
                if (pair.Key != null) pair.Key.enabled = pair.Value;
            hiddenCanvases.Clear();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            AnyOpen = false;
            EscapeConsumedFrame = Time.frameCount;
            if (canvas != null) canvas.gameObject.SetActive(false);
            RestoreOtherCanvases();
            InputGate.Instance?.Release(Gate);
        }

        private void OnDestroy()
        {
            if (IsOpen) Close();
            if (runtime == this) { SceneManager.sceneLoaded -= OnSceneLoaded; runtime = null; }
        }

        public static void NotifyNewEntry(string id) => runtime?.ShowNotice();

        private void ShowNotice()
        {
            if (noticeText == null)
            {
                var root = new GameObject("GrimoireNotice", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
                root.transform.SetParent(transform, false);
                var noticeCanvas = root.GetComponent<Canvas>();
                noticeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                noticeCanvas.sortingOrder = 950;
                var scaler = root.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                noticeText = Text("Message", root.transform, "Nova entrada no grimório", titleFont, 20,
                    new Vector2(.38f, .94f), new Vector2(.62f, .99f), TextAlignmentOptions.Center);
                noticeText.color = new Color(.9f, .75f, .95f);
            }
            noticeText.gameObject.SetActive(true);
            noticeUntil = Time.unscaledTime + 3f;
        }

        public void ShowPage(int requested)
        {
            if (pageTitle == null) return;
            if (selectedEntry == null)
            {
                pageTitle.text = pageBody.text = pageNumber.text = string.Empty;
                pageIllustration.gameObject.SetActive(false);
                pageSymbols.gameObject.SetActive(false);
                noteImage.gameObject.SetActive(false);
                previousPage.interactable = nextPage.interactable = false;
                return;
            }
            var hint = selectedEntry.Hints.Length > 0;
            pageTitle.text = selectedEntry.Title;
            pageIllustration.gameObject.SetActive(selectedEntry.Id == "A01");
            pageSymbols.gameObject.SetActive(false);
            noteImage.gameObject.SetActive(!string.IsNullOrEmpty(selectedEntry.Note));
            noteText.text = selectedEntry.Note ?? string.Empty;
            pageBody.rectTransform.anchorMax = selectedEntry.Id == "A01" ? new Vector2(.80f, .54f) : new Vector2(.80f, .75f);
            pageBody.rectTransform.offsetMax = Vector2.zero;
            if (hint)
            {
                var state = GameState.Instance;
                var recordAvailable = selectedEntry.Id == "P03" && state != null && state.HasFlag(StoryFlag.VoiceWellComplete)
                    || selectedEntry.Id == "P12" && state != null && state.HasFlag(StoryFlag.AndrealphusMeeting02Complete);
                pageCount = selectedEntry.Hints.Length + (recordAvailable && !string.IsNullOrEmpty(selectedEntry.Record) ? 1 : 0);
                page = Mathf.Clamp(requested, 0, pageCount - 1);
                pageBody.text = page < selectedEntry.Hints.Length
                    ? "Pista " + (page == 0 ? "I" : page == 1 ? "II" : "III") + "\n\n" + selectedEntry.Hints[page]
                    : "Registro após " + (selectedEntry.Id == "P03" ? "conclusão" : "a cena") + "\n\n" + selectedEntry.Record;
                pageBody.pageToDisplay = 1;
            }
            else
            {
                pageBody.text = selectedEntry.Body ?? string.Empty;
                pageBody.pageToDisplay = 1;
                pageBody.ForceMeshUpdate();
                pageCount = Mathf.Max(1, pageBody.textInfo.pageCount);
                page = Mathf.Clamp(requested, 0, pageCount - 1);
                pageBody.pageToDisplay = page + 1;
            }
            pageNumber.text = $"{page + 1} / {pageCount}";
            previousPage.interactable = page > 0;
            nextPage.interactable = page + 1 < pageCount;
        }

        private void SelectTab(int tab)
        {
            activeTab = Mathf.Clamp(tab, 0, 3);
            visibleEntries.Clear();
            foreach (var entry in GrimoireCatalog.Entries)
                if (entry.Category == activeTab && GrimoireCatalog.IsUnlocked(entry.Id)) visibleEntries.Add(entry);
            indexTitle.text = TabTitles[activeTab];
            for (int i = 0; i < tabFrames.Length; i++)
            {
                tabFrames[i].sprite = i == activeTab ? tabFrameSelected : tabFrameNormal;
                tabFrames[i].color = Color.white;
            }
            listPage = 0;
            selectedEntry = visibleEntries.Count > 0 ? visibleEntries[0] : null;
            if (selectedEntry != null) GrimoireCatalog.MarkRead(selectedEntry.Id);
            RenderList();
            ShowPage(0);
        }

        private void RenderList()
        {
            var start = listPage * RowsPerListPage;
            for (var i = 0; i < entryButtons.Length; i++)
            {
                var index = start + i;
                var button = entryButtons[i];
                button.gameObject.SetActive(index < visibleEntries.Count);
                if (index >= visibleEntries.Count) continue;
                var entry = visibleEntries[index];
                button.GetComponentInChildren<TMP_Text>().text = entry.Title + (GrimoireCatalog.IsNew(entry.Id) ? "  NOVO" : "");
                var rowImage = button.GetComponent<Image>();
                rowImage.sprite = entry == selectedEntry ? entrySelected : entryNormal;
                rowImage.color = rowImage.sprite != null ? Color.white : entry == selectedEntry ? new Color(.72f, .55f, .73f, .48f) : Color.clear;
            }
            emptyLabel.gameObject.SetActive(visibleEntries.Count == 0);
            var count = Mathf.Max(1, Mathf.CeilToInt(visibleEntries.Count / (float)RowsPerListPage));
            listNumber.text = $"{listPage + 1} / {count}";
            previousList.interactable = listPage > 0;
            nextList.interactable = listPage + 1 < count;
        }

        private void ChangeListPage(int requested)
        {
            var count = Mathf.Max(1, Mathf.CeilToInt(visibleEntries.Count / (float)RowsPerListPage));
            listPage = Mathf.Clamp(requested, 0, count - 1);
            RenderList();
        }

        private void SelectEntry(int row)
        {
            var index = listPage * RowsPerListPage + row;
            if (index < 0 || index >= visibleEntries.Count) return;
            selectedEntry = visibleEntries[index];
            GrimoireCatalog.MarkRead(selectedEntry.Id);
            RenderList();
            ShowPage(0);
        }

        private void BuildUi()
        {
            var root = new GameObject("GrimoireOverlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            var dim = Image("Dim", root.transform, null, new Color(.015f, .008f, .025f, .87f), new Vector2(0, 0), new Vector2(1, 1));
            dim.rectTransform.offsetMin = dim.rectTransform.offsetMax = Vector2.zero;

            var book = Image("OpenBook", root.transform, bookSprite, Color.white, new Vector2(.04f, .10f), new Vector2(.96f, .89f));
            book.preserveAspect = true;
            book.rectTransform.offsetMin = book.rectTransform.offsetMax = Vector2.zero;
            book.raycastTarget = false;
            var atlas = Resources.LoadAll<Sprite>("UI/Grimoire/GRIMOIRE_UI_SHEET_V2");
            Sprite Cell(string name) => System.Array.Find(atlas, s => s.name == name);
            tabFrameNormal = Cell("TAB_NORMAL");
            tabFrameHover = Cell("TAB_HOVER");
            tabFrameSelected = Cell("TAB_SELECTED");
            tabFrameLocked = Cell("TAB_LOCKED");
            entryNormal = Cell("ENTRY_NORMAL");
            entrySelected = Cell("ENTRY_SELECTED");
            leftArrow = Cell("ARROW_LEFT");
            rightArrow = Cell("ARROW_RIGHT");
            tabFrames = new Image[TabTitles.Length];
            for (int i = 0; i < TabTitles.Length; i++)
            {
                int selected = i;
                float left = .26f + i * .12f;
                var frame = Image("Tab_" + i, root.transform, i == 0 ? tabFrameSelected : tabFrameNormal,
                    Color.white, new Vector2(left, .795f), new Vector2(left + .115f, .895f));
                frame.rectTransform.offsetMin = frame.rectTransform.offsetMax = Vector2.zero;
                tabFrames[i] = frame;
                var button = frame.gameObject.AddComponent<Button>();
                button.interactable = true;
                button.transition = Selectable.Transition.SpriteSwap;
                button.spriteState = new SpriteState { highlightedSprite = tabFrameHover, pressedSprite = tabFrameSelected, selectedSprite = tabFrameSelected, disabledSprite = tabFrameLocked };
                button.onClick.AddListener(() => SelectTab(selected));
                var caption = Text("Caption", frame.transform, TabTitles[i], titleFont, 20,
                    new Vector2(.08f, .14f), new Vector2(.92f, .85f), TextAlignmentOptions.Center);
                caption.color = new Color(.91f, .79f, .91f);
            }

            indexTitle = Text("IndexTitle", book.transform, "ANOTAÇÕES", titleFont, 30, new Vector2(.20f, .76f), new Vector2(.43f, .86f), TextAlignmentOptions.Center);
            entryButtons = new Button[RowsPerListPage];
            for (int i = 0; i < entryButtons.Length; i++)
            {
                int selected = i;
                var top = .755f - i * .075f;
                var row = Image("Entry_" + i, book.transform, entryNormal, Color.white, new Vector2(.21f, top - .067f), new Vector2(.45f, top));
                row.rectTransform.offsetMin = row.rectTransform.offsetMax = Vector2.zero;
                entryButtons[i] = row.gameObject.AddComponent<Button>();
                entryButtons[i].onClick.AddListener(() => SelectEntry(selected));
                var label = Text("Label", row.transform, "", bodyFont, 27, new Vector2(.12f, .08f), new Vector2(.92f, .92f), TextAlignmentOptions.MidlineLeft);
                label.enableAutoSizing = true; label.fontSizeMin = 22; label.fontSizeMax = 27;
            }
            emptyLabel = Text("Empty", book.transform, "Nenhuma entrada descoberta.", bodyFont, 19,
                new Vector2(.22f, .47f), new Vector2(.44f, .56f), TextAlignmentOptions.Center);
            previousList = ArrowButton("PreviousList", book.transform, new Vector2(.245f, .18f), new Vector2(.285f, .23f), leftArrow);
            previousList.onClick.AddListener(() => ChangeListPage(listPage - 1));
            listNumber = Text("ListNumber", book.transform, "1 / 1", bodyFont, 18,
                new Vector2(.30f, .18f), new Vector2(.36f, .23f), TextAlignmentOptions.Center);
            nextList = ArrowButton("NextList", book.transform, new Vector2(.38f, .18f), new Vector2(.42f, .23f), rightArrow);
            nextList.onClick.AddListener(() => ChangeListPage(listPage + 1));
            pageTitle = Text("PageTitle", book.transform, "", titleFont, 28, new Vector2(.54f, .75f), new Vector2(.80f, .86f), TextAlignmentOptions.Center);
            pageTitle.enableAutoSizing = true;
            pageTitle.fontSizeMin = 24;
            pageTitle.fontSizeMax = 28;
            pageIllustration = Image("PageIllustration", book.transform, illustrationSprite, Color.white, new Vector2(.60f, .54f), new Vector2(.74f, .74f));
            pageIllustration.preserveAspect = true;
            pageIllustration.raycastTarget = false;
            pageIllustration.rectTransform.offsetMin = pageIllustration.rectTransform.offsetMax = Vector2.zero;
            pageSymbols = Image("SigilSymbols", book.transform, symbolsSprite, Color.white, new Vector2(.62f, .57f), new Vector2(.73f, .73f));
            pageSymbols.preserveAspect = true;
            pageSymbols.raycastTarget = false;
            pageSymbols.rectTransform.offsetMin = pageSymbols.rectTransform.offsetMax = Vector2.zero;
            pageBody = Text("PageBody", book.transform, "", bodyFont, 29, new Vector2(.54f, .27f), new Vector2(.80f, .75f), TextAlignmentOptions.TopLeft);
            pageBody.enableAutoSizing = false;
            pageBody.textWrappingMode = TextWrappingModes.Normal;
            pageBody.overflowMode = TextOverflowModes.Page;
            pageBody.lineSpacing = 3;
            pageNumber = Text("PageNumber", book.transform, "1 / 1", bodyFont, 20, new Vector2(.63f, .20f), new Vector2(.72f, .25f), TextAlignmentOptions.Center);
            noteImage = Image("SideNote", root.transform, noteSprite, Color.white, new Vector2(.82f, .20f), new Vector2(.96f, .67f));
            noteImage.preserveAspect = true; noteImage.raycastTarget = false;
            noteImage.rectTransform.offsetMin = noteImage.rectTransform.offsetMax = Vector2.zero;
            noteText = Text("SideNoteText", noteImage.transform, "", bodyFont, 16,
                new Vector2(.31f, .22f), new Vector2(.74f, .78f), TextAlignmentOptions.Center);
            noteText.enableAutoSizing = true; noteText.fontSizeMin = 12; noteText.fontSizeMax = 16;
            noteText.textWrappingMode = TextWrappingModes.Normal;
            noteText.overflowMode = TextOverflowModes.Truncate;
            previousPage = ArrowButton("Previous", book.transform, new Vector2(.59f, .19f), new Vector2(.64f, .25f), leftArrow);
            previousPage.onClick.AddListener(() => ShowPage(page - 1));
            nextPage = ArrowButton("Next", book.transform, new Vector2(.72f, .19f), new Vector2(.77f, .25f), rightArrow);
            nextPage.onClick.AddListener(() => ShowPage(page + 1));
            var close = TextButton("Close", root.transform, "ESC   Fechar", new Vector2(.41f, .015f), new Vector2(.59f, .095f));
            close.onClick.AddListener(Close);
            canvas.gameObject.SetActive(false);
        }

        private Button ArrowButton(string name, Transform parent, Vector2 min, Vector2 max, Sprite sprite)
        {
            var image = Image(name, parent, sprite, Color.white, min, max);
            image.rectTransform.offsetMin = image.rectTransform.offsetMax = Vector2.zero;
            image.preserveAspect = true;
            return image.gameObject.AddComponent<Button>();
        }

        private Button TextButton(string name, Transform parent, string label, Vector2 min, Vector2 max)
        {
            var image = Image(name, parent, closeFrameSprite, closeFrameSprite != null ? Color.white : new Color(.10f, .055f, .15f, .98f), min, max);
            image.rectTransform.offsetMin = image.rectTransform.offsetMax = Vector2.zero;
            image.preserveAspect = closeFrameSprite != null;
            var button = image.gameObject.AddComponent<Button>();
            var caption = Text("Label", image.transform, label, bodyFont, 26, new Vector2(.08f, .08f), new Vector2(.92f, .92f), TextAlignmentOptions.Center);
            caption.color = new Color(.9f, .78f, .91f);
            return button;
        }

        private static Image Image(string name, Transform parent, Sprite sprite, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max;
            var image = go.GetComponent<Image>(); image.sprite = sprite; image.color = color;
            return image;
        }

        private static TMP_Text Text(string name, Transform parent, string text, TMP_FontAsset font, float size, Vector2 min, Vector2 max, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var label = go.GetComponent<TextMeshProUGUI>();
            label.font = font != null ? font : TMP_Settings.defaultFontAsset;
            label.text = text; label.fontSize = size; label.alignment = alignment;
            label.color = Color.black;
            label.raycastTarget = false;
            return label;
        }
    }
}
