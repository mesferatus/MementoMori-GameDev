using System.Collections.Generic;
using MementoMori.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MementoMori.UI
{
    /// <summary>
    /// Gentle help for a player who is stuck: after a while on the same objective a soft nudge appears,
    /// later a direct one. Poe "says" it when he walks with Melantha (with a meow), otherwise it is
    /// Melantha's own sarcastic thought. UI only: it never changes story state, flags or the input gate.
    /// </summary>
    public sealed class StuckHintPresenter : MonoBehaviour
    {
        const float SoftAfter = 150f, DirectAfter = 300f, Visible = 7f, Fade = .8f;
        static readonly Dictionary<string, (string soft, string direct)> Hints = new()
        {
            ["Examine o quarto."] = ("Alguma coisa neste quarto quer ser notada. Que original.", "Chegue perto da fotografia, do grimório e da janela e interaja com cada um."),
            ["Olhe a fotografia com atenção."] = ("Aquela fotografia está me encarando há tempo demais.", "Vá até a fotografia e examine-a."),
            ["Consulte o grimório."] = ("O grimório não vai se ler sozinho. Infelizmente.", "Examine o livro aberto no quarto."),
            ["Feche a janela."] = ("Tem uma corrente de ar aqui. Clássico.", "Vá até a janela e feche-a."),
            ["Coloque a âncora no centro do círculo."] = ("O círculo no chão está esperando alguma coisa. Não é aplauso.", "Pegue o objeto ritual e guarde-o no centro do círculo."),
            ["Acenda as velas na ordem do grimório."] = ("Vela acesa na ordem errada é só decoração.", "Abra o grimório e acenda as velas na ordem que ele descreve."),
            ["Ative o círculo."] = ("Tudo pronto. Falta só a parte mágica, né.", "Volte ao centro do círculo e interaja."),
            ["Deite-se para atravessar."] = ("A cama. Dormir. É literalmente o próximo passo.", "Vá até a cama e interaja para dormir."),
            ["Siga as vozes nos Corredores dos Ecos."] = ("Uma das vozes soa mais clara e mais perto. As outras só fazem drama.", "Pare em cada bifurcação e siga o corredor onde a voz soa mais nítida e menos abafada."),
            ["Deixe os Corredores dos Ecos."] = ("Andar pra frente costuma ajudar.", "Continue pelo corredor onde as vozes terminaram."),
            ["Investigue a porta que devolve."] = ("Aquela porta promete uma saída. Promete.", "Atravesse a porta e repare para onde ela leva você."),
            ["Repare no que mudou na porta."] = ("A porta não quer ser vencida. Quer ser entendida.", "Na moldura: «Nem toda passagem se vence atravessando». Escolha reconhecer o retorno."),
            ["Escute o Poço das Vozes."] = ("O poço tem coisas a dizer. Todo mundo tem.", "Interaja com o poço e ouça as quatro vozes até o fim."),
            ["Reconheça a voz que contradiz sua memória."] = ("Uma daquelas vozes era minha. E mentiu.", "Escolha a voz que parece sua, mas que você não lembra de ter dito."),
            ["Faça as sombras convergirem no Pátio sem Céu."] = ("Três obeliscos e uma estrela ausente. Geometria, que delícia.", "Gire os obeliscos até as sombras apontarem para o mesmo ponto: Sul, Norte, Leste."),
            ["Abra o Núcleo Central."] = ("«O que abre não sustenta.» Alguém achou isso poético.", "Alinhe na ordem: Nome abre, Vínculo sustenta, Preço fecha."),
            ["Leve as marcas ao Santuário da Lua."] = ("As marcas querem ir para o Santuário da Lua. Eu também queria férias.", "No Santuário da Lua, escolha «Atravessar»: não possuir, não apagar."),
            ["Atravesse o portal da Lua."] = ("O portal está aberto. Dá pra sentir daqui.", "Volte ao portal da Lua e entre nele."),
            ["Nade até as flores do Jardim Lunar."] = ("As flores estão do outro lado da água. Claro que estão.", "Entre na água do Jardim Lunar e nade até tocar as flores."),
            ["Decifre o Salão dos Sete Espelhos."] = ("Sete espelhos e nenhum me favorece.", "Toque os espelhos na ordem que os símbolos indicam; depois de alguns erros, eles mostram pistas."),
            ["Encontre a saída do Corredor Ilusório."] = ("Não confie na imagem perfeita. Nunca é boa ideia.", "Siga a falha, depois o silêncio, depois a passagem."),
            ["Leia as portas da Galeria dos Ciclos."] = ("Oito portas, oito fases. Alguém aqui gosta de arquivo.", "Leia as oito portas e ordene as marcas: Marca, Reflexo, Memória."),
            ["Recomponha o Sigilo Fragmentado."] = ("Fase, memória e intenção. Três anéis, nenhuma paciência.", "Anéis do sigilo: a lua Minguante, o Grimório e o verbo SUSTENTAR."),
            ["Pegue o Fragmento."] = ("O Fragmento está logo ali. Dramaticamente esperando.", "Vá até a Câmara do Fragmento e interaja com ele."),
            ["Atravesse o portal de volta."] = ("Tenho o que vim buscar. Hora de ir.", "Volte ao Portal Final e atravesse."),
        };

        string objective, spokenLine;
        float since, shownAt = -100f;
        int level;
        CanvasGroup group;
        TMP_Text speaker, body;
        Image icon;
        static StuckHintPresenter instance;
        /// <summary>True while a hint is on screen (the echo question steps aside, they share the same place).</summary>
        public static bool Showing => instance != null && instance.group != null && instance.group.alpha > .05f;

        /// <summary>Shows one line in the hint banner now (room locked, gentle warnings).</summary>
        public static void Say(string line)
        {
            if (instance == null || instance.body == null) return;
            instance.open = true;
            instance.spokenLine = line;
            instance.speaker.text = PoeWalksWithMelantha() ? "Poe" : "Melantha";
            instance.body.text = Loc.T(line);
            if (Time.unscaledTime - instance.shownAt > 1.2f) MementoMori.Audio.RuntimeAudio.PlayOneShot("24_region_chime", .1f);
            instance.shownAt = Time.unscaledTime - .4f;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register() { SceneManager.sceneLoaded -= Attach; SceneManager.sceneLoaded += Attach; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AttachOpenScene() => Attach(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        static void Attach(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Quarto" && scene.name != "Labirinto" && scene.name != "DominioLua") return;
            if (FindFirstObjectByType<StuckHintPresenter>() != null) return;
            var host = new GameObject("StuckHints");
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<StuckHintPresenter>();
        }

        /// <summary>The direct hint for an objective, for tests and tooling.</summary>
        public static bool TryGetHints(string objectiveKey, out string soft, out string direct)
        {
            if (objectiveKey != null && Hints.TryGetValue(objectiveKey, out var pair)) { soft = pair.soft; direct = pair.direct; return true; }
            soft = direct = null; return false;
        }

        RectTransform star; Image starGlow; bool open;

        void OnEnable() => Loc.LanguageChanged += RefreshLanguage;
        void OnDisable() => Loc.LanguageChanged -= RefreshLanguage;
        void RefreshLanguage()
        {
            if (!open || body == null) return;
            if (spokenLine != null) { body.text = Loc.T(spokenLine); return; }
            var visibleSince = shownAt;
            Show(Mathf.Max(1, level));
            shownAt = visibleSince;
        }

        void Start()
        {
            instance = this;
            var root = new GameObject("HintCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 89;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                new GameObject("HintEventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

            // The hint lives beside the objective: a star button the player clicks when stuck. It glows once a
            // hint is ready; the first click gives a gentle nudge, the next one the direct answer.
            star = MenuPanelStyle.NewImage("EstrelaDica", root.transform, MenuPanelStyle.Diamond(), MenuPanelStyle.Lilac);
            star.anchorMin = star.anchorMax = new Vector2(1f, 1f); star.pivot = new Vector2(.5f, .5f);
            star.sizeDelta = new Vector2(58f, 58f); star.anchoredPosition = new Vector2(-730f, -152f);
            var starImage = star.GetComponent<Image>(); starImage.raycastTarget = true;
            var accent = MenuPanelStyle.Lilac; accent.a = 1f; starImage.color = accent;
            var outline = star.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(.045f, .012f, .075f, 1f); outline.effectDistance = new Vector2(1.5f, -1.5f);
            var button = star.gameObject.AddComponent<Button>(); button.targetGraphic = starImage;
            button.onClick.AddListener(Toggle);
            starGlow = MenuPanelStyle.NewImage("Brilho", star, MenuPanelStyle.Diamond(), new Color(.75f, .55f, 1f, 0f)).GetComponent<Image>();
            starGlow.rectTransform.anchorMin = starGlow.rectTransform.anchorMax = new Vector2(.5f, .5f);
            starGlow.rectTransform.sizeDelta = new Vector2(96f, 96f); starGlow.raycastTarget = false; starGlow.transform.SetAsFirstSibling();
            var core = MenuPanelStyle.NewImage("Centro", star, MenuPanelStyle.Diamond(), new Color(.05f, .022f, .078f, 1f));
            core.anchorMin = core.anchorMax = new Vector2(.5f, .5f); core.sizeDelta = new Vector2(26f, 26f); core.GetComponent<Image>().raycastTarget = false;
            var tag = MenuPanelStyle.NewText("Rotulo", star, MenuPanelStyle.HeadingFont, Loc.T("DICA"), 15f, MenuPanelStyle.TitleColor);
            tag.rectTransform.anchorMin = tag.rectTransform.anchorMax = new Vector2(.5f, 0f); tag.rectTransform.sizeDelta = new Vector2(120f, 24f);
            tag.rectTransform.anchoredPosition = new Vector2(0f, -14f); tag.alignment = TextAlignmentOptions.Center; tag.characterSpacing = 4f; tag.raycastTarget = false;
            LocalizedText.Bind(tag, () => Loc.T("DICA"));

            var plateGo = new GameObject("Plate", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            plateGo.transform.SetParent(root.transform, false);
            group = plateGo.GetComponent<CanvasGroup>(); group.alpha = 0f; group.interactable = group.blocksRaycasts = false;
            var image = plateGo.GetComponent<Image>(); image.color = new Color(.035f, .015f, .06f, .94f); image.raycastTarget = false;
            var rect = image.rectTransform;
            // Right under the objective panel, same width: it reads as the objective's footnote.
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f); rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(600f, 190f); rect.anchoredPosition = new Vector2(-84f, -250f);
            MenuPanelStyle.FrameOnly(rect);
            // Close button (x) in the corner: the card can be dismissed at once.
            var close = MenuPanelStyle.NewText("Fechar", rect, MenuPanelStyle.HeadingFont, "×", 34f, MenuPanelStyle.Lilac);
            close.rectTransform.anchorMin = close.rectTransform.anchorMax = new Vector2(1f, 1f); close.rectTransform.pivot = new Vector2(1f, 1f);
            close.rectTransform.sizeDelta = new Vector2(48f, 48f); close.rectTransform.anchoredPosition = new Vector2(-14f, -8f);
            close.alignment = TextAlignmentOptions.Center; close.raycastTarget = true;
            close.gameObject.AddComponent<Button>().onClick.AddListener(() => { open = false; shownAt = -100f; group.alpha = 0f; });
            speaker = Label("Speaker", rect, MenuPanelStyle.HeadingFont, 23f, new Vector2(.07f, .7f), new Vector2(.93f, .9f), MenuPanelStyle.CaptionColor);
            speaker.characterSpacing = 5f;
            body = Label("Body", rect, MenuPanelStyle.BodyFont, 29f, new Vector2(.07f, .1f), new Vector2(.93f, .68f), new Color(.93f, .89f, .96f));
            body.fontStyle = FontStyles.Italic; body.enableAutoSizing = true; body.fontSizeMin = 20f; body.fontSizeMax = 29f;
        }

        void Toggle()
        {
            if (open && level >= 2) { open = false; shownAt = -100f; return; }
            Show(Mathf.Clamp(open ? level + 1 : Mathf.Max(1, level), 1, 2));
            open = true;
        }

        static TMP_Text Label(string name, Transform parent, TMP_FontAsset font, float size, Vector2 min, Vector2 max, Color color)
        {
            var text = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
            text.transform.SetParent(parent, false);
            text.rectTransform.anchorMin = min; text.rectTransform.anchorMax = max; text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
            text.font = font; text.fontSize = size; text.color = color; text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.Normal; text.raycastTarget = false;
            return text;
        }

        void Update()
        {
            if (group == null) return;
            var current = ObjectiveToastController.Instance != null ? ObjectiveToastController.Instance.CurrentObjective : null;
            if (current != objective) { objective = current; since = 0f; level = 0; open = false; group.alpha = 0f; shownAt = -100f; }
            var blocked = InputGate.Instance != null && InputGate.Instance.IsBlocked;
            // The clock only runs while the player can actually play: not in dialogue, menus, puzzles or pause.
            if (!blocked && Time.timeScale > 0f) since += Time.unscaledDeltaTime;
            var hasHint = TryGetHints(objective, out _, out _);
            star.gameObject.SetActive(!blocked && hasHint);
            // Ready: the star breathes a glow so the player notices help is there (never pops up on its own).
            var ready = since >= SoftAfter && !open;
            var calm = AccessibilitySettings.Instance != null && AccessibilitySettings.Instance.ReduceFlashes;
            var glow = ready ? (calm ? .35f : .25f + .25f * Mathf.Sin(Time.unscaledTime * 3f)) : 0f;
            starGlow.color = new Color(.75f, .55f, 1f, glow);
            var age = Time.unscaledTime - shownAt;
            var alpha = age < .3f ? age / .3f : age < 12f ? 1f : Mathf.Clamp01(1f - (age - 12f) / Fade);
            if (alpha <= 0f) open = false;
            group.alpha = blocked ? 0f : alpha;
            group.blocksRaycasts = group.interactable = group.alpha > .5f;
        }

        void Show(int hintLevel)
        {
            spokenLine = null;
            level = hintLevel;
            if (!TryGetHints(objective, out var soft, out var direct)) return;
            var poe = PoeWalksWithMelantha();
            speaker.text = (poe ? "POE" : "MELANTHA") + "  ·  " + Loc.T(hintLevel == 1 ? "DICA" : "DICA DIRETA");
            var line = Loc.T(hintLevel == 1 ? soft : direct);
            body.text = poe ? Loc.T("(Poe mia e olha para você. Parece dizer:)") + " " + line : "(" + line + ")";
            shownAt = Time.unscaledTime;
            MementoMori.Audio.RuntimeAudio.PlayOneShot(poe ? "15_poe_soft_call" : "24_region_chime", poe ? .35f : .12f);
        }

        static bool PoeWalksWithMelantha()
        {
            var poe = FindAnyObjectByType<MementoMori.Poe.PoeFollower>();
            return poe != null && poe.isActiveAndEnabled && poe.State != MementoMori.Poe.PoeState.Hidden
                && poe.State != MementoMori.Poe.PoeState.Disabled && GameState.Instance != null
                && GameState.Instance.HasFlag(StoryFlag.PoeRevealed);
        }
    }
}
