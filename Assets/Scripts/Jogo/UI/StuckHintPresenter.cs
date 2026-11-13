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
            ["Recomponha o Sigilo Fragmentado."] = ("Fase, memória e intenção. Três anéis, nenhuma paciência.", "Anéis do sigilo: a lua Nova, a Tigela do quarto e o verbo CHAMAR."),
            ["Pegue o Fragmento."] = ("O Fragmento está logo ali. Dramaticamente esperando.", "Vá até a Câmara do Fragmento e interaja com ele."),
            ["Atravesse o portal de volta."] = ("Tenho o que vim buscar. Hora de ir.", "Volte ao Portal Final e atravesse."),
        };

        string objective;
        float since, shownAt = -100f;
        int level;
        CanvasGroup group;
        TMP_Text speaker, body;

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

        void Start()
        {
            var root = new GameObject("HintCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 89;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            group = root.GetComponent<CanvasGroup>(); group.alpha = 0f; group.interactable = group.blocksRaycasts = false;

            var plate = new GameObject("Plate", typeof(RectTransform), typeof(Image));
            plate.transform.SetParent(root.transform, false);
            var image = plate.GetComponent<Image>(); image.color = new Color(.07f, .05f, .11f, .82f); image.raycastTarget = false;
            var rect = image.rectTransform;
            rect.anchorMin = new Vector2(.26f, .05f); rect.anchorMax = new Vector2(.74f, .17f); rect.offsetMin = rect.offsetMax = Vector2.zero;

            var book = Resources.Load<GameObject>("GrimoireRuntimeHost")?.GetComponent<MementoMori.World.GrimoireScreen>();
            var font = book != null && book.HeadingFont != null ? book.HeadingFont : TMP_Settings.defaultFontAsset;
            speaker = Label("Speaker", plate.transform, font, 20f, new Vector2(.03f, .62f), new Vector2(.97f, .96f), new Color(.79f, .72f, .84f));
            body = Label("Body", plate.transform, TMP_Settings.defaultFontAsset, 24f, new Vector2(.03f, .06f), new Vector2(.97f, .66f), new Color(.93f, .89f, .96f));
            body.fontStyle = FontStyles.Italic;
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
            if (current != objective) { objective = current; since = 0f; level = 0; group.alpha = 0f; shownAt = -100f; }
            var blocked = InputGate.Instance != null && InputGate.Instance.IsBlocked;
            // The clock only runs while the player can actually play: not in dialogue, menus, puzzles or pause.
            if (!blocked && Time.timeScale > 0f) since += Time.unscaledDeltaTime;
            if (level == 0 && since >= SoftAfter) Show(1);
            else if (level == 1 && since >= DirectAfter) Show(2);
            var age = Time.unscaledTime - shownAt;
            var alpha = age < .4f ? age / .4f : age < Visible ? 1f : Mathf.Clamp01(1f - (age - Visible) / Fade);
            group.alpha = blocked ? 0f : alpha;
        }

        void Show(int hintLevel)
        {
            level = hintLevel;
            if (!TryGetHints(objective, out var soft, out var direct)) return;
            var poe = PoeWalksWithMelantha();
            speaker.text = poe ? "Poe" : "Melantha";
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
