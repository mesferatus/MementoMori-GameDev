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
        static readonly Color Ink = new(.80f, .86f, 1f), Blue = new(.42f, .74f, 1f);
        public static bool IsOpen { get; private set; }
        public static int CloseConsumedFrame { get; private set; } = -1;

        [System.Serializable] sealed class WorldRect { public float xMin, yMin, width, height; }

        GameObject root;
        RectTransform frame, sheet, dot, halo;
        readonly Dictionary<string, TMP_Text> names = new();
        readonly Dictionary<string, Vector2> labelAt = new();
        readonly List<RectTransform> trailDots = new();
        readonly Queue<Vector2> trail = new();
        Rect world;
        bool hasMap;
        TMP_Text title, footer, tip;
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
            // One-time discovery tip, a few seconds after the first arrival in a mapped domain.
            if (GameState.Instance != null && GameState.Instance.GetCounter("map.tip") == 0) tipAt = Time.unscaledTime + 8f;
        }

        void Update()
        {
            UpdateTip();
            RecordTrail();
            var toggle = Input.GetKeyDown(KeyCode.M) || GamepadInput.MapDown();
            if (IsOpen)
            {
                if (toggle || InputBindings.CancelDown()) { Close(); return; }
                Refresh();
                return;
            }
            if (!toggle) return;
            // Never over dialogue, puzzles, the grimoire or the pause menu.
            if (InputGate.Instance != null && InputGate.Instance.IsBlocked) return;
            Open();
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
            if (InputGate.Instance != null && InputGate.Instance.IsBlocked) { tipAt = Time.unscaledTime + 2f; return; }
            if (tip == null)
            {
                var canvasGo = new GameObject("MapTipCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
                canvasGo.transform.SetParent(transform, false);
                var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 88;
                Scale(canvasGo.GetComponent<CanvasScaler>());
                tip = NewText("MapTip", canvasGo.transform, TMP_Settings.defaultFontAsset, 22f, new Vector2(.02f, .02f), new Vector2(.4f, .07f));
                tip.alignment = TextAlignmentOptions.BottomLeft; tip.color = new Color(.79f, .72f, .84f);
                GameState.Instance?.SetCounter("map.tip", 1);
            }
            var age = Time.unscaledTime - tipAt;
            tip.text = Loc.T(GamepadInput.UsingGamepad ? "Select — mapa" : "M — mapa");
            tip.alpha = age < .5f ? age / .5f : age < 6f ? .9f : Mathf.Clamp01(.9f - (age - 6f));
            if (age > 7f || IsOpen) { Destroy(tip.transform.parent.gameObject); tip = null; tipAt = -1f; }
        }

        void Open()
        {
            if (root == null) Build();
            if (RegionTitlePresenter.CurrentRegion != null) MarkVisited(RegionTitlePresenter.CurrentRegion, null);
            IsOpen = true;
            InputGate.Instance?.Block(Gate);
            root.SetActive(true);
            MementoMori.Audio.RuntimeAudio.PlayOneShot("24_region_chime", .1f);
            Canvas.ForceUpdateCanvases();
            Refresh();
        }

        void Close()
        {
            IsOpen = false;
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

            var veil = NewImage("Veil", root.transform, new Color(.03f, .04f, .07f, 1f));
            Stretch(veil.rectTransform, Vector2.zero, Vector2.one);

            var book = Resources.Load<GameObject>("GrimoireRuntimeHost")?.GetComponent<MementoMori.World.GrimoireScreen>();
            var font = book != null && book.HeadingFont != null ? book.HeadingFont : TMP_Settings.defaultFontAsset;
            title = NewText("Title", root.transform, font, 44f, new Vector2(.2f, .915f), new Vector2(.8f, .99f));
            footer = NewText("Footer", root.transform, font, 20f, new Vector2(.2f, .01f), new Vector2(.8f, .055f));

            frame = new GameObject("Frame", typeof(RectTransform)).GetComponent<RectTransform>();
            frame.SetParent(root.transform, false);
            Stretch(frame, new Vector2(.03f, .07f), new Vector2(.97f, .91f));

            var picture = Resources.Load<Texture2D>("Mapas/" + scene);
            var bounds = Resources.Load<TextAsset>("Mapas/" + scene + "_bounds");
            hasMap = picture != null && bounds != null;
            var rect = hasMap ? JsonUtility.FromJson<WorldRect>(bounds.text) : new WorldRect { width = 1, height = 1 };
            world = new Rect(rect.xMin, rect.yMin, rect.width, rect.height);

            // The sheet keeps the drawing's real proportions, centered in the frame.
            sheet = new GameObject("Sheet", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter)).GetComponent<RectTransform>();
            sheet.SetParent(frame, false);
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
                var label = NewText("Name_" + r.id, sheet, font, 17f, Vector2.zero, Vector2.zero);
                label.rectTransform.sizeDelta = new Vector2(220f, 50f);
                label.textWrappingMode = TextWrappingModes.Normal;
                label.outlineWidth = .3f; label.outlineColor = new Color32(6, 10, 22, 255);
                names[r.id] = label;
                labelAt[r.id] = FloorCentre(r.floors, r.bounds);
            }
            halo = NewImage("MelanthaHalo", sheet, new Color(Blue.r, Blue.g, Blue.b, .28f)).rectTransform;
            halo.sizeDelta = new Vector2(30f, 30f); halo.localEulerAngles = new Vector3(0, 0, 45f);
            dot = NewImage("Melantha", sheet, Blue).rectTransform;
            dot.sizeDelta = new Vector2(13f, 13f); dot.localEulerAngles = new Vector3(0, 0, 45f);
            dot.gameObject.AddComponent<Outline>().effectColor = new Color(.9f, .97f, 1f, .9f);
            root.SetActive(false);
        }

        /// <summary>Mean of the module's floor cells: the renderer bounds of a tilemap can be far larger than the room.</summary>
        static Vector2 FloorCentre(List<UnityEngine.Tilemaps.Tilemap> floors, Bounds fallback)
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
            footer.text = Loc.T(GamepadInput.UsingGamepad ? "Select ou B — fechar" : "M ou Esc — fechar");
            foreach (var pair in names)
            {
                var visited = Visited(pair.Key);
                pair.Value.gameObject.SetActive(visited && hasMap);
                if (!visited) continue;
                var region = RegionTitlePresenter.MapRegions.Find(r => r.id == pair.Key);
                var here = pair.Key == RegionTitlePresenter.CurrentRegion;
                pair.Value.text = Loc.T(region.title ?? pair.Key);
                pair.Value.color = here ? new Color(1f, .96f, .84f) : Ink;
                pair.Value.fontStyle = here ? FontStyles.Bold : FontStyles.Normal;
                Place(pair.Value.rectTransform, labelAt[pair.Key]);
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
            var visible = player != null && hasMap;
            dot.gameObject.SetActive(visible); halo.gameObject.SetActive(visible);
            if (!visible) return;
            Place(dot, player.position); Place(halo, player.position);
            halo.SetAsLastSibling(); dot.SetAsLastSibling();
            var calm = AccessibilitySettings.Instance != null && AccessibilitySettings.Instance.ReduceFlashes;
            var pulse = calm ? 0f : Mathf.Sin(Time.unscaledTime * 3.5f);
            halo.localScale = Vector3.one * (1.1f + .35f * pulse);
            dot.localScale = Vector3.one * (1f + .12f * pulse);
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
