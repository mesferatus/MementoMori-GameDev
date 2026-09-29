using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.Interaction;
using MementoMori.Poe;
using UnityEngine;

namespace MementoMori.World
{
    /// <summary>Three recoverable rounds: the altered memory, not the repeated phrase, is correct.</summary>
    public sealed class EchoCorridorPuzzle : MonoBehaviour
    {
        // The indices remain stable for scene bindings; labels keep the choices narrative-first.
        static readonly int[] CorrectPassages = { 2, 1, 3 };
        public static readonly string[] CorrectChoiceMeanings =
        {
            "Uma imagem preserva forma.",
            "Eu continuei chamando.",
            "Você voltou porque abriu."
        };
        public const string WrongFinalChoiceMeaning = "Você voltou porque morreu.";
        static readonly string[] RepeatedChoices = { "Uma imagem preserva presença.", "Eu parei de chamar.", WrongFinalChoiceMeaning };
        public string ChoiceLabel(int passage)
        {
            var current = Mathf.Clamp(round, 0, CorrectPassages.Length - 1);
            return passage == CorrectPassages[current] ? CorrectChoiceMeanings[current] : RepeatedChoices[current];
        }
        [SerializeField] private DialogueData completionDialogue;
        [SerializeField] private Vector2 returnPosition = new(-2.1f, -2.2f);
        [SerializeField] private Transform[] passages;
        int round;
        int errors;
        private void Awake()
        {
            completionDialogue ??= Resources.Load<DialogueData>("Dialogue/DLG_L_ECHO_COMPLETE");
        }
        public bool Solved => GameState.Instance != null && GameState.Instance.HasFlag(StoryFlag.EchoTrial03Complete);
        public void Configure(DialogueData dialogue, Transform[] corridorPassages)
        {
            completionDialogue = dialogue;
            passages = corridorPassages;
        }
        public bool Select(int passage, Transform player)
        {
            if (Solved || round >= CorrectPassages.Length) return false;
            GrimoireCatalog.Discover("P0" + (round + 4));
            if (passage != CorrectPassages[round])
            {
                errors++;
                GameState.Instance?.IncrementCounter("echo.errors");
                if (player != null) player.position = returnPosition;
                if (errors >= 3 && passages != null && CorrectPassages[round] < passages.Length)
                    Object.FindAnyObjectByType<PoeFollower>()?.HintAt(passages[CorrectPassages[round]].position);
                return false;
            }
            errors = 0;
            round++;
            var state = GameState.Instance;
            if (round == 1) state?.SetFlag(StoryFlag.EchoTrial01Complete);
            if (round == 2) state?.SetFlag(StoryFlag.EchoTrial02Complete);
            if (round == 3)
            {
                state?.SetFlag(StoryFlag.EchoTrial03Complete);
                state?.SaveCheckpoint();
                StoryProgression.Instance?.SaveCheckpoint(CheckpointId.Echoes);
                DialogueManager.Instance?.StartDialogue(completionDialogue);
            }
            return true;
        }
    }
}
