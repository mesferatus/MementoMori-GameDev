using System;
using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.Interaction;
using TMPro;
using UnityEngine;


namespace MementoMori.Regions
{
    [Serializable]
    public class V6ChallengeDefinition
    {
        public string id, title, clue, success, prerequisiteFlag;
        public string[] choices, prerequisites;
        public int[] sequence;
        public float x, y;
        public bool requiresOtherLabyrinthPuzzles;
    }
    [Serializable] public class V6ChallengeLayout { public V6ChallengeDefinition[] challenges; }

    /// <summary>A local, recoverable interaction assembled by the Editor baker from data.</summary>
    public sealed class V6Challenge : MonoBehaviour, IInteractable
    {
        [SerializeField] private V6ChallengeDefinition definition;
        public static bool AnyOpen { get; private set; }
        public static int EscapeConsumedFrame { get; private set; }
        private GameObject panel;
        private TMP_Text feedback;
        private RegionPuzzleState model;
        private string Gate => "V6Challenge." + definition.id;
        public string InteractionVerb => definition.title;
        public int InteractionPriority => 8;
        public void Configure(V6ChallengeDefinition data) => definition=data;
        public bool CanInteract(InteractionContext context) => definition != null && panel == null
            && GameState.Instance != null && !GameState.Instance.Puzzles.GetSolved(definition.id)
            && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
        public void Interact(InteractionContext context)
        {
            if (!CanInteract(context)) return;
            model = new RegionPuzzleState(GameState.Instance,definition.id,RegionOperation.Sequence,definition.sequence,definition.choices.Length);
            BuildPanel(); AnyOpen=true;
            InputGate.Instance?.Block(Gate);
        }
        private void Update()
        {
            if (panel == null) return;
            if (Input.GetKeyDown(KeyCode.Escape)) { EscapeConsumedFrame=Time.frameCount; Close(); return; }
            var keys=new[]{KeyCode.Alpha1,KeyCode.Alpha2,KeyCode.Alpha3,KeyCode.Alpha4};
            for(var i=0;i<Mathf.Min(keys.Length,definition.choices.Length);i++)
                if(Input.GetKeyDown(keys[i])) Choose(i);
        }
        public void Choose(int index)
        {
            if (model == null || panel == null) return;
            if (definition.requiresOtherLabyrinthPuzzles)
                foreach (var id in GameState.Instance.Puzzles.LabyrinthIds)
                    if (id != definition.id && !GameState.Instance.Puzzles.GetSolved(id)) {
                        feedback.text="Ainda faltam caminhos a compreender no Labirinto."; return;
                    }
            if (definition.prerequisites != null)
                foreach(var id in definition.prerequisites)
                    if(!GameState.Instance.Puzzles.GetSolved(id)) {
                        feedback.text="Ainda faltam caminhos a compreender. Explore os desafios do Labirinto antes de ativar o portal.";
                        return;
                    }
            if (!string.IsNullOrEmpty(definition.prerequisiteFlag)
                && Enum.TryParse(definition.prerequisiteFlag,out StoryFlag flag)
                && !GameState.Instance.HasFlag(flag)) {
                feedback.text="Primeiro, examine este lugar e escute o que ele guarda.";
                return;
            }
            var accepted=model.Choose(index);
            feedback.text=model.Solved ? (string.IsNullOrEmpty(definition.success) ? "A inscrição se ilumina. Este caminho foi compreendido. Esc — voltar." : definition.success) :
                accepted ? "A inscrição responde. Continue a sequência." :
                "A luz recua. Releia a inscrição e tente novamente; seus outros caminhos estão preservados.";
            if(model.Solved) GameState.Instance.SaveCheckpoint(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, FindAnyObjectByType<MementoMori.Player.PlayerController>() is var player && player != null ? (Vector2)player.transform.position : (Vector2)transform.position);
        }
        private void BuildPanel()
        {
            panel=new GameObject("V6 Inscription",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));
            var canvas=panel.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=110;
            var scaler=panel.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1920,1080);
            var card=new GameObject("Parchment",typeof(RectTransform),typeof(UnityEngine.UI.Image));
            card.transform.SetParent(panel.transform,false);
            var rect=card.GetComponent<RectTransform>(); rect.anchorMin=new Vector2(.22f,.18f);rect.anchorMax=new Vector2(.78f,.82f);rect.offsetMin=rect.offsetMax=Vector2.zero;
            card.GetComponent<UnityEngine.UI.Image>().color=new Color(.035f,.025f,.065f,.98f);
            Text(card.transform,definition.title,40,new Vector2(.05f,.84f),new Vector2(.95f,.96f),new Color(.85f,.72f,.46f));
            Text(card.transform,definition.clue,27,new Vector2(.07f,.60f),new Vector2(.93f,.83f),new Color(.88f,.84f,.94f));
            for(var i=0;i<definition.choices.Length;i++) {
                var index=i; var y=.51f-i*.085f;
                var button=new GameObject("Choice"+i,typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button));
                button.transform.SetParent(card.transform,false);
                var br=button.GetComponent<RectTransform>();br.anchorMin=new Vector2(.08f,y);br.anchorMax=new Vector2(.92f,y+.07f);br.offsetMin=br.offsetMax=Vector2.zero;
                button.GetComponent<UnityEngine.UI.Image>().color=new Color(.16f,.12f,.23f);
                button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(()=>Choose(index));
                Text(button.transform,(i+1)+"  "+definition.choices[i],25,Vector2.zero,Vector2.one,Color.white);
            }
            feedback=Text(card.transform,"Escolha pelos números ou pelo mouse. Esc — voltar",23,new Vector2(.07f,.02f),new Vector2(.93f,.16f),new Color(.75f,.72f,.84f));
        }
        private static TMP_Text Text(Transform parent,string value,int size,Vector2 min,Vector2 max,Color color)
        {
            var text=new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            text.transform.SetParent(parent,false);text.rectTransform.anchorMin=min;text.rectTransform.anchorMax=max;
            text.rectTransform.offsetMin=new Vector2(8,2);text.rectTransform.offsetMax=new Vector2(-8,-2);
            text.text=value;text.font=TMP_Settings.defaultFontAsset;text.fontSize=size;text.color=color;
            text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
            return text;
        }
        public void Close()
        {
            if(panel!=null) AnyOpen=false;
            if(definition!=null) InputGate.Instance?.Release(Gate);
            if(panel!=null) Destroy(panel);
            panel=null; model=null;
        }
        private void OnDisable()=>Close();
    }
}



