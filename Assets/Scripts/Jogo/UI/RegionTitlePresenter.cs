using System.Collections.Generic;
using System.Text.RegularExpressions;
using MementoMori.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace MementoMori.UI
{
    /// <summary>Once per region per scene visit; presentation never acquires the input gate.</summary>
    public sealed class RegionTitlePresenter : MonoBehaviour
    {
        // A module is the union of its own floor tiles. The old version used a single 27x5 floor strip,
        // so names appeared while crossing that strip mid-room (and in corridors), never on entry.
        sealed class Region { public string id, title; public List<UnityEngine.Tilemaps.Tilemap> floors = new(); public Bounds bounds; public bool hasBounds; }
        readonly List<Region> regions = new();
        /// <summary>Raised once when Melantha settles inside a module (id, title key).</summary>
        public static event System.Action<string, string> RegionEntered;
        /// <summary>Module footprints of the open scene, for the map screen.</summary>
        public static readonly List<(string id, string title, Bounds bounds, List<UnityEngine.Tilemaps.Tilemap> floors)> MapRegions = new();
        public static string CurrentRegion { get; private set; }
        readonly Dictionary<string, float> lastShown = new();
        const float ReshowCooldown = 15f;
        Transform player;
        CanvasGroup group;
        TMP_Text label;
        string candidate, title, current;
        float enteredAt, shownAt = -100f;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            SceneManager.sceneLoaded -= Attach;
            SceneManager.sceneLoaded += Attach;
        }
        // Entering Play without a scene reload never raises sceneLoaded for the scene already open.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AttachOpenScene() => Attach(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        static void Attach(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Labirinto" && scene.name != "DominioLua") return;
            if (FindFirstObjectByType<RegionTitlePresenter>() != null) return;
            var host = new GameObject("RegionTitles");
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<RegionTitlePresenter>();
        }
        static string TitleFor(string id, string roomName) => id switch
        {
            "M01" => "Entrada da Lua", "M02" => "Jardim Lunar", "M03" => "Salão dos Sete Espelhos", "M04" => "Corredor Ilusório",
            "M05" => "Galeria dos Ciclos", "M06" => "Sigilo Fragmentado", "M07" => "Câmara do Fragmento", "M08" => "Portal Final",
            _ => roomName
        };
        void Start()
        {
            var byId = new Dictionary<string, Region>();
            var environment = GameObject.Find("V65_Environment");
            if (environment != null) foreach (Transform room in environment.transform)
            {
                var match = Regex.Match(room.name, @"^([LM]\d\d)_(.+)$");
                if (!match.Success || match.Groups[2].Value.StartsWith("layer_") || match.Groups[2].Value.Contains("Flame")) continue;
                var id = match.Groups[1].Value;
                if (!byId.ContainsKey(id)) byId[id] = new Region { id = id, title = TitleFor(id, match.Groups[2].Value) };
            }
            var floorPattern = new Regex(@"^([LM]\d\d)_layer_(piso|agua)");
            var edgePattern = new Regex(@"^([LM]\d\d)_layer_(margem|escadas|sigilos)");
            foreach (var map in FindObjectsByType<UnityEngine.Tilemaps.Tilemap>(FindObjectsSortMode.None))
            {
                var floor = floorPattern.Match(map.name);
                if (floor.Success && byId.TryGetValue(floor.Groups[1].Value, out var region)) { region.floors.Add(map); continue; }
                var edge = edgePattern.Match(map.name);
                if (edge.Success && byId.TryGetValue(edge.Groups[1].Value, out var room))
                {
                    var renderer = map.GetComponent<Renderer>();
                    if (renderer == null) continue;
                    if (!room.hasBounds) { room.bounds = renderer.bounds; room.hasBounds = true; }
                    else room.bounds.Encapsulate(renderer.bounds);
                }
            }
            MapRegions.Clear(); CurrentRegion = null;
            foreach (var region in byId.Values)
            {
                if (region.floors.Count == 0 && !region.hasBounds) continue;
                regions.Add(region);
                var footprint = region.bounds; var has = region.hasBounds;
                foreach (var floor in region.floors)
                {
                    var r = floor != null ? floor.GetComponent<Renderer>() : null;
                    if (r == null) continue;
                    if (!has) { footprint = r.bounds; has = true; } else footprint.Encapsulate(r.bounds);
                }
                if (has) MapRegions.Add((region.id, region.title, footprint, region.floors));
            }
            var root = new GameObject("RegionTitleCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            root.GetComponent<Canvas>().sortingOrder = 90;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            group = root.GetComponent<CanvasGroup>(); group.alpha = 0; group.interactable = group.blocksRaycasts = false;
            var text = new GameObject("RegionTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            text.transform.SetParent(root.transform, false);
            label = text.GetComponent<TMP_Text>(); var book = Resources.Load<GameObject>("GrimoireRuntimeHost")?.GetComponent<MementoMori.World.GrimoireScreen>();
            label.font = book != null && book.HeadingFont != null ? book.HeadingFont : TMP_Settings.defaultFontAsset;
            label.fontSize = 30; label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(.79f, .72f, .84f); label.raycastTarget = false;
            label.rectTransform.anchorMin = new Vector2(.22f, .73f); label.rectTransform.anchorMax = new Vector2(.78f, .80f);
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
        }
        static bool Contains(Region region, Vector3 position)
        {
            foreach (var floor in region.floors)
                if (floor != null && floor.HasTile(floor.WorldToCell(position))) return true;
            if (region.floors.Count > 0 || !region.hasBounds) return false;
            var p = position; p.z = region.bounds.center.z;
            return region.bounds.Contains(p);
        }

        void Update()
        {
            if (group == null) return;
            if (player == null) player = FindFirstObjectByType<Player.PlayerController>()?.transform;
            if (player == null) return;
            string inside = null, insideTitle = null;
            foreach (var region in regions)
            {
                if (!Contains(region, player.position)) continue;
                inside = region.id; insideTitle = region.title; break;
            }
            if (candidate != inside) { candidate = inside; enteredAt = Time.unscaledTime; }
            var blocked = InputGate.Instance != null && InputGate.Instance.IsBlocked;
            // Announce on entry: only after Melantha has really been inside for a moment, and not again
            // when she steps out and back in right away.
            if (!blocked && inside != null && inside != current && Time.unscaledTime - enteredAt > .6f)
            {
                current = inside;
                CurrentRegion = inside;
                RegionEntered?.Invoke(inside, insideTitle);
                if (!lastShown.TryGetValue(inside, out var last) || Time.unscaledTime - last > ReshowCooldown)
                { lastShown[inside] = Time.unscaledTime; title = insideTitle; shownAt = Time.unscaledTime; MementoMori.Audio.RuntimeAudio.PlayOneShot("24_region_chime", .2f); }
            }
            if (inside == null && candidate == null && Time.unscaledTime - enteredAt > .6f) current = null;
            var age = Time.unscaledTime - shownAt;
            var alpha = age < .4f ? age / .4f : age < 2.6f ? 1f : Mathf.Clamp01(1f - (age - 2.6f) / .7f);
            group.alpha = blocked ? 0f : Mathf.Clamp01(alpha);
            label.text = title == null ? string.Empty : Loc.T(title);
        }
    }
}
