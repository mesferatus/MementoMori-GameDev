using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.Interaction;
using UnityEngine;

namespace MementoMori.World
{
    /// <summary>Optional well: each interaction advances one altered memory without blocking progress.</summary>
    public sealed class VoiceWellController : MonoBehaviour, IInteractable
    {
        [SerializeField] DialogueData[] voices;
        [SerializeField] GameObject[] energyStages;
        int heard;
        private void Awake()
        {
            if (voices == null || voices.Length != 4)
            {
                voices = new DialogueData[4];
            }
            for (var i = 0; i < voices.Length; i++)
                if (voices[i] == null) voices[i] = Resources.Load<DialogueData>("Dialogue/DLG_L_VOICE_WELL_0" + (i + 1));
        }
        private void Start()
        {
            var state = GameState.Instance;
            heard = state != null && state.HasFlag(StoryFlag.VoiceWellComplete)
                ? voices.Length : Mathf.Clamp(state?.GetCounter("voiceWell.heard") ?? 0, 0, voices.Length);
            if (heard > 0) ApplyVisualStage(heard - 1);
        }
        public string InteractionVerb => "Escutar o poço";
        public int InteractionPriority => 4;
        public void Configure(params DialogueData[] lines) => voices = lines;
        public void ConfigureVisualStages(params GameObject[] stages) => energyStages = stages;
        public bool CanInteract(InteractionContext context) => voices != null && voices.Length > 0 && heard < voices.Length;
        public void Interact(InteractionContext context)
        {
            if (!CanInteract(context)) return;
            GrimoireCatalog.Discover("P03");
            GrimoireCatalog.Discover("R05");
            var index = Mathf.Min(heard, voices.Length - 1);
            heard++;
            GameState.Instance?.SetCounter("voiceWell.heard", heard);
            ApplyVisualStage(index);
            DialogueManager.Instance?.StartDialogue(voices[index]);
            if (heard >= voices.Length)
                GameState.Instance?.SetFlag(StoryFlag.VoiceWellComplete);
        }

        private void ApplyVisualStage(int index)
        {
            if (energyStages == null) return;
            for (var i = 0; i < energyStages.Length; i++)
                if (energyStages[i] != null) energyStages[i].SetActive(i == index);
        }
    }
}
