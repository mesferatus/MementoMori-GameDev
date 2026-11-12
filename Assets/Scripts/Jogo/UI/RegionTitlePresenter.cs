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
        readonly List<(string id, string title, Bounds bounds)> regions = new();
        readonly HashSet<string> visited = new();
        Transform player;
        CanvasGroup group;
        TMP_Text label;
        string candidate, title;
        float enteredAt, shownAt = -100f;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            SceneManager.sceneLoaded -= Attach;
            SceneManager.sceneLoaded += Attach;
        }
        static void Attach(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Labirinto" && scene.name != "DominioLua") return;
            if (FindFirstObjectByType<RegionTitlePresenter>() != null) return;
            var host = new GameObject("RegionTitles");
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<RegionTitlePresenter>();
        }
        void Start()
        {
            var environment = GameObject.Find("V65_Environment");
            if (environment != null) foreach (Transform room in environment.transform)
            {
                var match = Regex.Match(room.name, @"^([LM]\d\d)_(.+)$");
                if (!match.Success || match.Groups[2].Value.StartsWith("layer_")) continue;
                // Authored room roots have a floor layer. Ignore individual walls, lights and props.
                Renderer floor = null;
                foreach (var renderer in room.GetComponentsInChildren<Renderer>())
                {
                    var n = renderer.name.ToLowerInvariant();
                    if ((n.Contains("piso") || n.Contains("chao") || n.Contains("chão") || n.Contains("floor")) && renderer.bounds.size.x > 4f)
                    { if (floor == null || renderer.bounds.size.x > floor.bounds.size.x) floor = renderer; }
                }
                if (floor == null) foreach (var renderer in room.GetComponentsInChildren<Renderer>())
                    if (renderer.name.Contains("layer_agua") && renderer.bounds.size.x > 4f) { floor = renderer; break; }
                if (floor == null) continue;
                var bounds = floor.bounds;
                bounds.Expand(new Vector3(-.5f, -.5f, 0));
                var id = match.Groups[1].Value;
                var title = id switch { "M01" => "Entrada da Lua", "M02" => "Jardim Lunar", "M03" => "Salão dos Sete Espelhos", "M04" => "Corredor Ilusório", "M05" => "Galeria dos Ciclos", "M06" => "Sigilo Fragmentado", "M07" => "Câmara do Fragmento", "M08" => "Portal Final", _ => match.Groups[2].Value };
                regions.Add((id, title, bounds));
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
        void Update()
        {
            if (group == null) return;
            if (player == null) player = FindFirstObjectByType<Player.PlayerController>()?.transform;
            if (player == null) return;
            string inside = null, insideTitle = null;
            foreach (var region in regions)
            {
                var p = player.position; p.z = region.bounds.center.z;
                if (!region.bounds.Contains(p)) continue;
                inside = region.id; insideTitle = region.title; break;
            }
            if (candidate != inside) { candidate = inside; enteredAt = Time.unscaledTime; }
            var blocked = InputGate.Instance != null && InputGate.Instance.IsBlocked;
            if (!blocked && inside != null && !visited.Contains(inside) && Time.unscaledTime - enteredAt > .8f)
            { visited.Add(inside); title = insideTitle; shownAt = Time.unscaledTime; }
            var age = Time.unscaledTime - shownAt;
            var alpha = age < .7f ? age / .7f : age < 3f ? 1f : Mathf.Clamp01(1f - (age - 3f) / 1.2f);
            group.alpha = blocked ? 0f : Mathf.Clamp01(alpha);
            label.text = title == null ? string.Empty : Loc.T(title);
        }
    }
}
