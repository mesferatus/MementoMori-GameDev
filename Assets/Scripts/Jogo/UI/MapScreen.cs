using System.Collections.Generic;
using MementoMori.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MementoMori.UI
{
    /// <summary>
    /// Drawn map of the Labyrinth and the Moon Domain: a blue-ink illustration made from a real top-down
    /// capture of the whole level (Resources/Mapas/&lt;scene&gt;.png, world rect in &lt;scene&gt;_bounds.json).
    /// A blue mark follows Melantha live and leaves a faint trail; rooms she has entered get their names
    /// (kept in the save as "map.visited.&lt;id&gt;"). M or the controller's Select toggles it; Esc / B close.
    /// It holds the input gate while open and never changes story state.
    /// </summary>
    public sealed class MapScreen : MonoBehaviour
    {
        const string Gate = "Map";
        const int TrailLength = 48;
        const float TrailStep = 1.2f;
        static readonly Color Ink = new(.93f, .88f, 1f), Blue = new(.42f, .74f, 1f);
        public static bool IsOpen { get; private set; }
        public static int CloseConsumedFrame { get; private set; } = -1;

        [System.Serializable] sealed class WorldRect { public float xMin, yMin, width, height; }

        GameObject root;
        RectTransform frame, pan, sheet, dot, halo;
        // Close-up that follows Melantha; wheel, +/- or the right stick zoom between the whole sheet and a room.
        const float MinZoom = 1f, MaxZoom = 3.2f, DefaultZoom = 2.2f;
        float zoom = DefaultZoom, targetZoom = DefaultZoom;
        // Hand-drag panning: the open hand shows the sheet can be grabbed, the closed hand while dragging.
        Vector2 dragOffset; bool dragging; Vector3 dragFrom;
        static Texture2D openHand, closedHand;
        readonly Dictionary<string, TMP_Text> names = new();
        readonly Dictionary<string, Vector2> labelAt = new();
        readonly Dictionary<string, RectTransform> plaques = new();
        readonly Dictionary<string, Image> puzzleIcons = new();
        const float LabelWidth = 132f;
        // Rooms whose floor layers cannot place a label: the Empty Chamber's layers hang from other modules'
        // grids, and the Echo Corridors region has no floor of its own. Both sit on the regular module
        // grid (columns x 18.25 / 47 / 75.75), so their label goes in the middle of the room itself.
        static readonly Dictionary<string, Vector2> LabelOverrides = new()
        {
            ["Labirinto/L02"] = new Vector2(75.75f, -10.5f),
            ["Labirinto/L03"] = new Vector2(47f, -33.7f),
        };
        readonly List<RectTransform> trailDots = new();
        readonly Queue<Vector2> trail = new();
        Rect world;
        bool hasMap;
        TMP_Text title, footer, mapShortcutLabel, bookShortcutLabel;
        CanvasGroup shortcutGroup;
        RectTransform mapShortcut, bookShortcut;
        Transform player;
        Vector2 lastTrail = new(float.MaxValue, 0f);
        float tipAt = -1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register() { SceneManager.sceneLoaded -= Attach; SceneManager.sceneLoaded += Attach; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AttachOpenScene() => Attach(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        static void Attach(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Labirinto" && scene.name != "DominioLua") return;
            if (FindFirstObjectByType<MapScreen>() != null) return;
            var host = new GameObject("MapScreen");
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<MapScreen>();
        }

        public static string VisitedKey(string id) => "map.visited." + id;
        static bool Visited(string id) => GameState.Instance != null && GameState.Instance.GetCounter(VisitedKey(id)) > 0;

        void OnEnable() => RegionTitlePresenter.RegionEntered += MarkVisited;
        void OnDisable()
        {
            RegionTitlePresenter.RegionEntered -= MarkVisited;
            if (IsOpen) { IsOpen = false; InputGate.Instance?.Release(Gate); }
        }

        static void MarkVisited(string id, string title)
        {
            if (GameState.Instance != null && GameState.Instance.GetCounter(VisitedKey(id)) == 0)
                GameState.Instance.SetCounter(VisitedKey(id), 1);
        }

        void Start()
        {
            // Compact shortcuts remain available beside the hint star during exploration.
            tipAt = Time.unscaledTime + 1f;
        }

        void Update()
        {
            UpdateTip();
            RecordTrail();
            var toggle = Input.GetKeyDown(KeyCode.M) || GamepadInput.MapDown();
            if (IsOpen)
            {
                if (toggle || InputBindings.CancelDown()) { Close(); return; }
                ReadZoom();
                ReadDrag();
                Refresh();
                return;
            }
            if (!toggle) return;
            // Never over dialogue, puzzles, the grimoire or the pause menu.
            if (InputGate.Instance != null && InputGate.Instance.IsBlocked) return;
            Open();
        }

        void ReadZoom()
        {
            var step = Input.mouseScrollDelta.y * .25f;
            if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus)) step += Time.unscaledDeltaTime * 2f;
            if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus)) step -= Time.unscaledDeltaTime * 2f;
            var pad = UnityEngine.InputSystem.Gamepad.current;
            if (pad != null) step += pad.rightStick.ReadValue().y * Time.unscaledDeltaTime * 2f;
            targetZoom = Mathf.Clamp(targetZoom + step, MinZoom, MaxZoom);
            zoom = Mathf.Lerp(zoom, targetZoom, 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));
        }

        void ReadDrag()
        {
            if (Input.GetMouseButtonDown(0)) { dragging = true; dragFrom = Input.mousePosition; }
            if (Input.GetMouseButtonUp(0)) dragging = false;
            if (dragging)
            {
                var scale = root != null ? root.GetComponentInParent<Canvas>()?.scaleFactor ?? 1f : 1f;
                dragOffset += (Vector2)(Input.mousePosition - dragFrom) / Mathf.Max(.01f, scale);
                dragFrom = Input.mousePosition;
            }
            // Gamepad: the left stick pans the sheet.
            var pad = UnityEngine.InputSystem.Gamepad.current;
            if (pad != null) dragOffset -= pad.leftStick.ReadValue() * 900f * Time.unscaledDeltaTime;
            SetHandCursor(dragging ? ClosedHand() : OpenHand());
        }

        static void SetHandCursor(Texture2D hand) => Cursor.SetCursor(hand, hand == null ? Vector2.zero : new Vector2(12f, 10f), CursorMode.Auto);
        static Texture2D OpenHand() => openHand != null ? openHand : openHand = DrawHand(false);
        static Texture2D ClosedHand() => closedHand != null ? closedHand : closedHand = DrawHand(true);

        // A 24x24 pixel-art glove in the UI's ink and lilac, drawn once in code (no new art asset needed).
        static Texture2D DrawHand(bool closed)
        {
            string[] rows = closed ? new[]
            {
                "........................", "........................", "........................", "........................",
                "........................", "........................", ".......##.##.##.........", "......#ll#ll#ll##.......",
                "......#llllllllll#......", ".....##llllllllll#......", "....#l#llllllllll#......", "....#llllllllllll#......",
                "....#lllllllllll#.......", ".....#llllllllll#.......", ".....#llllllllll#.......", "......#llllllll#........",
                "......#llllllll#........", ".......########.........", "........................", "........................",
                "........................", "........................", "........................", "........................",
            } : new[]
            {
                "........................", ".........##.............", "........#ll#.##.........", "........#ll##ll#........",
                "....##..#ll#ll#.##......", "...#ll#.#ll#ll##ll#.....", "...#ll#.#ll#ll#lll#.....", "....#ll##ll#ll#ll#......",
                "....#lll#llllllll#......", ".....#llllllllllll#.....", ".....#llllllllllll#.....", "......#lllllllllll#.....",
                "......#llllllllll#......", ".......#lllllllll#......", ".......#llllllll#.......", "........#lllllll#.......",
                "........#lllllll#.......", "........#########.......", "........................", "........................",
                "........................", "........................", "........................", "........................",
            };
            var tex = new Texture2D(24, 24, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var ink = new Color32(26, 10, 40, 255); var lilac = new Color32(226, 205, 247, 255); var clear = new Color32(0, 0, 0, 0);
            for (var y = 0; y < 24; y++)
            for (var x = 0; x < 24; x++)
            {
                var c = rows[23 - y][x];
                tex.SetPixel(x, y, c == '#' ? ink : c == 'l' ? lilac : clear);
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Breadcrumbs of where Melantha walked in this visit, drawn as a fading blue trail.</summary>
        void RecordTrail()
        {
            if (player == null) player = FindAnyObjectByType<MementoMori.Player.PlayerController>()?.transform;
            if (player == null) return;
            var p = (Vector2)player.position;
            if ((p - lastTrail).sqrMagnitude < TrailStep * TrailStep) return;
            lastTrail = p;
            trail.Enqueue(p);
            while (trail.Count > TrailLength) trail.Dequeue();
        }

        void UpdateTip()
        {
            if (tipAt < 0f || Time.unscaledTime < tipAt) return;
            if (shortcutGroup == null) BuildShortcuts();
            var blocked = InputGate.Instance != null && InputGate.Instance.IsBlocked;
            shortcutGroup.alpha = blocked || IsOpen ? 0f : 1f;
            shortcutGroup.interactable = shortcutGroup.blocksRaycasts = shortcutGroup.alpha > .5f;
            var hasBook = GameState.Instance != null && GameState.Instance.HasFlag(StoryFlag.RoomGrimoireRead);
            bookShortcut.gameObject.SetActive(true);
            bookShortcut.GetComponent<Button>().interactable = hasBook;
            mapShortcut.anchoredPosition = new Vector2(-910f, -152f);
            mapShortcutLabel.text = Loc.T("MAPA");
            bookShortcutLabel.text = Loc.T("Grimório").ToUpperInvariant();
        }

        void BuildShortcuts()
        {
            var canvasGo = new GameObject("GameplayShortcuts", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 89;
            Scale(canvasGo.GetComponent<CanvasScaler>());
            shortcutGroup = canvasGo.GetComponent<CanvasGroup>();
            mapShortcut = Shortcut(canvasGo.transform, "MapShortcut", -910f, false, out mapShortcutLabel);
            bookShortcut = Shortcut(canvasGo.transform, "GrimoireShortcut", -820f, true, out bookShortcutLabel);
            mapShortcut.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (!IsOpen && (InputGate.Instance == null || !InputGate.Instance.IsBlocked)) Open();
            });
            bookShortcut.GetComponent<Button>().onClick.AddListener(() =>
            {
                var book = FindAnyObjectByType<MementoMori.World.GrimoireScreen>();
                if (book != null && GameState.Instance != null && GameState.Instance.HasFlag(StoryFlag.RoomGrimoireRead)) book.Open();
            });
        }

        static RectTransform Shortcut(Transform parent, string name, float x, bool book, out TMP_Text label)
        {
            var accent = MenuPanelStyle.Lilac; accent.a = 1f;
            var root = MenuPanelStyle.NewImage(name, parent, MenuPanelStyle.Diamond(), accent);
            root.anchorMin = root.anchorMax = Vector2.one; root.pivot = new Vector2(.5f, .5f);
            root.sizeDelta = new Vector2(58f, 58f); root.anchoredPosition = new Vector2(x, -152f);
            var image = root.GetComponent<Image>(); image.raycastTarget = true;
            var outline = root.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(.045f, .012f, .075f, 1f); outline.effectDistance = new Vector2(1.5f, -1.5f);
            var button = root.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(.72f, .52f, .86f); colors.disabledColor = new Color(1f, 1f, 1f, .7f); button.colors = colors;
            var core = MenuPanelStyle.NewImage("Centro", root, MenuPanelStyle.Diamond(), new Color(.065f, .025f, .11f, 1f));
            core.anchorMin = core.anchorMax = new Vector2(.5f, .5f); core.sizeDelta = new Vector2(42f, 42f);
            core.GetComponent<Image>().raycastTarget = false;
            var backing = MenuPanelStyle.NewImage("FundoSimbolo", root, null, new Color(.10f, .035f, .16f, 1f));
            backing.anchorMin = backing.anchorMax = new Vector2(.5f, .5f); backing.sizeDelta = new Vector2(29f, 27f);
            backing.GetComponent<Image>().raycastTarget = false;
            Vector2[] shape = book
                ? new[] { new Vector2(-13,-10), new Vector2(-13,9), new Vector2(-5,11), new Vector2(0,7), new Vector2(5,11), new Vector2(13,9), new Vector2(13,-10), new Vector2(5,-8), new Vector2(0,-11), new Vector2(-5,-8), new Vector2(-13,-10) }
                : new[] { new Vector2(-13,-9), new Vector2(-13,9), new Vector2(-4,13), new Vector2(4,9), new Vector2(13,13), new Vector2(13,-5), new Vector2(4,-9), new Vector2(-4,-5), new Vector2(-13,-9) };
            for (var i = 1; i < shape.Length; i++) ShortcutStroke(root, shape[i - 1], shape[i]);
            if (book)
            {
                ShortcutStroke(root, new Vector2(0,-11), new Vector2(0,7));
                ShortcutStroke(root, new Vector2(-10,3), new Vector2(-5,5));
                ShortcutStroke(root, new Vector2(5,5), new Vector2(10,3));
            }
            else
            {
                ShortcutStroke(root, new Vector2(-4,-5), new Vector2(-4,13));
                ShortcutStroke(root, new Vector2(4,-9), new Vector2(4,9));
                ShortcutStroke(root, new Vector2(-10,0), new Vector2(-7,3));
                ShortcutStroke(root, new Vector2(7,-1), new Vector2(10,2));
            }
            label = MenuPanelStyle.NewText("Rotulo", root, MenuPanelStyle.HeadingFont, string.Empty, 15f, MenuPanelStyle.TitleColor);
            label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(.5f, 0f);
            label.rectTransform.sizeDelta = new Vector2(120f, 24f); label.rectTransform.anchoredPosition = new Vector2(0f, -14f);
            label.alignment = TextAlignmentOptions.Center; label.characterSpacing = book ? 2f : 4f; label.raycastTarget = false;
            return root;
        }

        static void ShortcutStroke(RectTransform parent, Vector2 a, Vector2 b)
        {
            var ink = Color.Lerp(MenuPanelStyle.Lilac, MenuPanelStyle.TitleColor, .4f); ink.a = 1f;
            var stroke = MenuPanelStyle.NewImage("Traco", parent, null, ink);
            stroke.anchorMin = stroke.anchorMax = new Vector2(.5f, .5f); stroke.pivot = new Vector2(.5f, .5f);
            stroke.anchoredPosition = (a + b) * .5f; stroke.sizeDelta = new Vector2(Vector2.Distance(a, b), 2f);
            stroke.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
            stroke.GetComponent<Image>().raycastTarget = false;
        }

        void Open()
        {
            if (root == null) Build();
            if (RegionTitlePresenter.CurrentRegion != null) MarkVisited(RegionTitlePresenter.CurrentRegion, null);
            IsOpen = true;
            zoom = targetZoom = DefaultZoom;
            InputGate.Instance?.Block(Gate);
            root.SetActive(true);
            MementoMori.Audio.RuntimeAudio.PlayOneShot("24_region_chime", .1f);
            Canvas.ForceUpdateCanvases();
            Refresh();
        }

        void Close()
        {
            IsOpen = false;
            dragging = false; dragOffset = Vector2.zero;
            SetHandCursor(null);
            CloseConsumedFrame = Time.frameCount;
            if (root != null) root.SetActive(false);
            InputGate.Instance?.Release(Gate);
        }

        static void Scale(CanvasScaler scaler)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
        }

        void Build()
        {
            var scene = SceneManager.GetActiveScene().name;
            root = new GameObject("MapCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 30000;
            Scale(root.GetComponent<CanvasScaler>());

            // Same veil, double lilac frame, spaced title and divider as the pause, settings and credits panels.
            MenuPanelStyle.Veil(root.transform, .96f);

            var book = Resources.Load<GameObject>("GrimoireRuntimeHost")?.GetComponent<MementoMori.World.GrimoireScreen>();
            var font = book != null && book.HeadingFont != null ? book.HeadingFont : TMP_Settings.defaultFontAsset;
            var nameFont = book != null && book.BodyFont != null ? book.BodyFont : font;
            title = NewText("Title", root.transform, font, 44f, new Vector2(.2f, .915f), new Vector2(.8f, .99f));
            title.color = MenuPanelStyle.TitleColor; title.characterSpacing = 12f; title.fontSize = 40f;
            footer = NewText("Footer", root.transform, nameFont, 20f, new Vector2(.2f, .01f), new Vector2(.8f, .055f));
            footer.color = MenuPanelStyle.CaptionColor; footer.characterSpacing = 2f;
            var titleBand = new GameObject("TitleBand", typeof(RectTransform)).GetComponent<RectTransform>();
            titleBand.SetParent(root.transform, false);
            Stretch(titleBand, new Vector2(0f, .925f), new Vector2(1f, .93f));
            MenuPanelStyle.Divider(titleBand, 0f, 420f);

            frame = new GameObject("Frame", typeof(RectTransform)).GetComponent<RectTransform>();
            frame.SetParent(root.transform, false);
            Stretch(frame, new Vector2(.03f, .07f), new Vector2(.97f, .91f));
            frame.gameObject.AddComponent<RectMask2D>();
            var outline = new GameObject("Moldura", typeof(RectTransform)).GetComponent<RectTransform>();
            outline.SetParent(root.transform, false);
            Stretch(outline, new Vector2(.03f, .07f), new Vector2(.97f, .91f));
            MenuPanelStyle.FrameOnly(outline);
            pan = new GameObject("Pan", typeof(RectTransform)).GetComponent<RectTransform>();
            pan.SetParent(frame, false);
            Stretch(pan, Vector2.zero, Vector2.one);

            var picture = Resources.Load<Texture2D>("Mapas/" + scene);
            var bounds = Resources.Load<TextAsset>("Mapas/" + scene + "_bounds");
            hasMap = picture != null && bounds != null;
            var rect = hasMap ? JsonUtility.FromJson<WorldRect>(bounds.text) : new WorldRect { width = 1, height = 1 };
            world = new Rect(rect.xMin, rect.yMin, rect.width, rect.height);

            // The sheet keeps the drawing's real proportions, centered in the frame.
            sheet = new GameObject("Sheet", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter)).GetComponent<RectTransform>();
            sheet.SetParent(pan, false);
            Stretch(sheet, Vector2.zero, Vector2.one);
            var raw = sheet.GetComponent<RawImage>(); raw.texture = picture; raw.raycastTarget = false;
            var fitter = sheet.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = hasMap ? (float)picture.width / picture.height : 1f;
            var border = sheet.gameObject.AddComponent<Outline>(); border.effectColor = new Color(Ink.r, Ink.g, Ink.b, .45f); border.effectDistance = new Vector2(2, -2);

            for (var i = 0; i < TrailLength; i++)
            {
                var crumb = NewImage("Trail", sheet, new Color(Blue.r, Blue.g, Blue.b, 0f)).rectTransform;
                crumb.sizeDelta = new Vector2(6f, 6f); crumb.localEulerAngles = new Vector3(0, 0, 45f);
                trailDots.Add(crumb);
            }
            foreach (var r in RegionTitlePresenter.MapRegions)
            {
                // Each name sits on a small dark plaque, wrapped to roughly a room's width, so neighbours
                // never run into each other and the ink stays readable over the lilac wash.
                var plaque = NewImage("Name_" + r.id, sheet, new Color(.05f, .022f, .078f, .9f)).rectTransform;
                plaque.gameObject.AddComponent<Outline>().effectColor = new Color(Ink.r, Ink.g, Ink.b, .35f);
                var label = NewText("Text", plaque, nameFont, 12f, Vector2.zero, Vector2.one);
                label.characterSpacing = 1.5f;
                label.textWrappingMode = TextWrappingModes.Normal;
                label.margin = new Vector4(6f, 2f, 6f, 2f);
                names[r.id] = label; plaques[r.id] = plaque;
                // Puzzle rooms carry a seal at the plaque's left: grey (locked), pulsing blue (next), gold (solved).
                var seal = MenuPanelStyle.NewImage("PuzzleSeal", plaque, MenuPanelStyle.Diamond(), Color.white);
                seal.anchorMin = seal.anchorMax = new Vector2(0f, .5f); seal.sizeDelta = new Vector2(16f, 16f);
                seal.anchoredPosition = new Vector2(-11f, 0f);
                seal.gameObject.AddComponent<Outline>().effectColor = new Color(.02f, .01f, .04f, .9f);
                puzzleIcons[r.id] = seal.GetComponent<Image>();
                labelAt[r.id] = LabelOverrides.TryGetValue(scene + "/" + r.id, out var fixedAt) ? fixedAt : FloorCentre(r.floors, r.bounds);
            }
            halo = NewImage("MelanthaHalo", sheet, new Color(Blue.r, Blue.g, Blue.b, .28f)).rectTransform;
            halo.sizeDelta = new Vector2(30f, 30f); halo.localEulerAngles = new Vector3(0, 0, 45f);
            dot = NewImage("Melantha", sheet, Blue).rectTransform;
            dot.sizeDelta = new Vector2(13f, 13f); dot.localEulerAngles = new Vector3(0, 0, 45f);
            dot.gameObject.AddComponent<Outline>().effectColor = new Color(.9f, .97f, 1f, .9f);
            root.SetActive(false);
        }

        /// <summary>
        /// The roomiest point of the module: the floor cell farthest from any edge. A plain average lands in
        /// a corridor when a region owns corridor strips too (Empty Chamber, Echo Corridors); this one always
        /// sits in the middle of the actual room.
        /// </summary>
        static Vector2 FloorCentre(List<UnityEngine.Tilemaps.Tilemap> floors, Bounds fallback)
        {
            var best = -1; var bestPoint = (Vector2)fallback.center;
            var mean = MeanFloor(floors, fallback);
            var bestTie = float.MaxValue;
            foreach (var map in floors)
            {
                if (map == null) continue;
                map.CompressBounds();
                var b = map.cellBounds; int w = b.size.x, h = b.size.y;
                if (w <= 0 || h <= 0) continue;
                // Two-pass chamfer distance (in cells) to the nearest non-floor cell.
                var d = new int[w * h];
                for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    var i = y * w + x;
                    if (!map.HasTile(new Vector3Int(b.xMin + x, b.yMin + y, b.zMin))) { d[i] = 0; continue; }
                    var left = x > 0 ? d[i - 1] : 0; var down = y > 0 ? d[i - w] : 0;
                    d[i] = Mathf.Min(left, down) + 1;
                }
                for (var y = h - 1; y >= 0; y--)
                for (var x = w - 1; x >= 0; x--)
                {
                    var i = y * w + x;
                    if (d[i] == 0) continue;
                    var right = x < w - 1 ? d[i + 1] : 0; var up = y < h - 1 ? d[i + w] : 0;
                    d[i] = Mathf.Min(d[i], Mathf.Min(right, up) + 1);
                    var world = (Vector2)map.GetCellCenterWorld(new Vector3Int(b.xMin + x, b.yMin + y, b.zMin));
                    var tie = (world - mean).sqrMagnitude;
                    if (d[i] > best || (d[i] == best && tie < bestTie)) { best = d[i]; bestTie = tie; bestPoint = world; }
                }
            }
            return best > 0 ? bestPoint : mean;
        }

        static Vector2 MeanFloor(List<UnityEngine.Tilemaps.Tilemap> floors, Bounds fallback)
        {
            var sum = Vector2.zero; var n = 0;
            foreach (var map in floors)
            {
                if (map == null) continue;
                map.CompressBounds();
                foreach (var cell in map.cellBounds.allPositionsWithin)
                    if (map.HasTile(cell)) { sum += (Vector2)map.GetCellCenterWorld(cell); n++; }
            }
            return n > 0 ? sum / n : (Vector2)fallback.center;
        }

        Vector2 ToSheet(Vector2 p) => new((p.x - world.xMin) / world.width, (p.y - world.yMin) / world.height);

        void Place(RectTransform item, Vector2 worldPosition)
        {
            item.anchorMin = item.anchorMax = ToSheet(worldPosition);
            item.anchoredPosition = Vector2.zero;
        }

        void Refresh()
        {
            var scene = SceneManager.GetActiveScene().name;
            title.text = Loc.T(scene == "DominioLua" ? "MAPA DO DOMÍNIO DA LUA" : "MAPA DO LABIRINTO");
            footer.text = Loc.T(GamepadInput.UsingGamepad ? "Analógico esquerdo — mover · direito — zoom · Select ou B — fechar" : "Arraste com o mouse — mover · roda ou +/− — zoom · M ou Esc — fechar");
            foreach (var pair in names)
            {
                var visited = Visited(pair.Key);
                var puzzle = MementoMori.Regions.PuzzleRoomGate.Status(scene, pair.Key);
                // Puzzle rooms are always marked, even unseen, so the player knows where the next challenge is.
                plaques[pair.Key].gameObject.SetActive((visited || puzzle != null) && hasMap);
                if (!visited && puzzle == null) continue;
                var region = RegionTitlePresenter.MapRegions.Find(r => r.id == pair.Key);
                var here = pair.Key == RegionTitlePresenter.CurrentRegion;
                var seal = puzzleIcons[pair.Key];
                seal.gameObject.SetActive(puzzle != null);
                var name = visited || puzzle == MementoMori.Regions.PuzzleRoomGate.RoomStatus.Solved || puzzle == MementoMori.Regions.PuzzleRoomGate.RoomStatus.Current ? Loc.T(region.title ?? pair.Key) : "???";
                if (puzzle == null) pair.Value.text = name;
                else
                {
                    var status = puzzle.Value;
                    var word = status == MementoMori.Regions.PuzzleRoomGate.RoomStatus.Solved ? "<color=#E8C77A>" + Loc.T("CONCLUÍDO") + "</color>"
                        : status == MementoMori.Regions.PuzzleRoomGate.RoomStatus.Current ? "<color=#9FD3FF>" + Loc.T("PRÓXIMO DESAFIO") + "</color>"
                        : "<color=#C9C0DA>" + Loc.T("TRANCADO") + "</color>";
                    pair.Value.text = name + "\n<size=78%>" + word + "</size>";
                    var sealPulse = .75f + .25f * Mathf.Sin(Time.unscaledTime * 4f);
                    seal.color = status == MementoMori.Regions.PuzzleRoomGate.RoomStatus.Solved ? new Color(.91f, .78f, .48f)
                        : status == MementoMori.Regions.PuzzleRoomGate.RoomStatus.Current ? new Color(.62f, .83f, 1f, sealPulse)
                        : new Color(.72f, .68f, .8f);
                }
                pair.Value.color = here ? new Color(1f, .93f, .72f) : new Color(.93f, .91f, 1f);
                pair.Value.fontStyle = here ? FontStyles.Bold : FontStyles.Normal;
                var size = pair.Value.GetPreferredValues(pair.Value.text, LabelWidth - 12f, 0f);
                plaques[pair.Key].sizeDelta = new Vector2(Mathf.Min(size.x, LabelWidth - 12f) + 12f, size.y + 4f);
                Place(plaques[pair.Key], labelAt[pair.Key]);
            }
            var index = 0; var count = trail.Count;
            foreach (var p in trail)
            {
                var crumb = trailDots[index];
                Place(crumb, p);
                crumb.GetComponent<Image>().color = new Color(Blue.r, Blue.g, Blue.b, .12f + .45f * (index + 1f) / Mathf.Max(1, count));
                index++;
            }
            for (; index < trailDots.Count; index++) trailDots[index].GetComponent<Image>().color = Color.clear;
            // Zoom the drawing and keep Melantha in the middle, without showing past the sheet's edges.
            pan.localScale = Vector3.one * zoom;
            var focus = player != null && hasMap ? ToSheet(player.position) : new Vector2(.5f, .5f);
            var sheetSize = sheet.rect.size; var view = frame.rect.size;
            var shift = -(focus - new Vector2(.5f, .5f)) * sheetSize * zoom + dragOffset;
            var slack = Vector2.Max(Vector2.zero, (sheetSize * zoom - view) * .5f);
            pan.anchoredPosition = new Vector2(Mathf.Clamp(shift.x, -slack.x, slack.x), Mathf.Clamp(shift.y, -slack.y, slack.y));
            var inverse = 1f / zoom;
            foreach (var crumb in trailDots) crumb.localScale = Vector3.one * inverse;
            var visible = player != null && hasMap;
            dot.gameObject.SetActive(visible); halo.gameObject.SetActive(visible);
            if (!visible) return;
            Place(dot, player.position); Place(halo, player.position);
            halo.SetAsLastSibling(); dot.SetAsLastSibling();
            var calm = AccessibilitySettings.Instance != null && AccessibilitySettings.Instance.ReduceFlashes;
            var pulse = calm ? 0f : Mathf.Sin(Time.unscaledTime * 3.5f);
            halo.localScale = Vector3.one * (1.1f + .35f * pulse) * inverse * 1.4f;
            dot.localScale = Vector3.one * (1f + .12f * pulse) * inverse * 1.4f;
        }

        static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        { rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; }

        static Image NewImage(string name, Transform parent, Color color)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(parent, false); image.color = color; image.raycastTarget = false;
            return image;
        }

        static TMP_Text NewText(string name, Transform parent, TMP_FontAsset font, float size, Vector2 min, Vector2 max)
        {
            var text = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
            text.transform.SetParent(parent, false);
            text.rectTransform.anchorMin = min; text.rectTransform.anchorMax = max; text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
            text.font = font; text.fontSize = size; text.color = Ink; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            return text;
        }
    }
}
