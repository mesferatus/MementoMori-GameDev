using System;
using UnityEngine;

namespace MementoMori.Dialogue
{
    [CreateAssetMenu(menuName = "Memento Mori/Dialogue Data", fileName = "DLG_")]
    public sealed class DialogueData : ScriptableObject
    {
        [SerializeField] private string sequenceId;
        [SerializeField] private DialogueLine[] lines;
        [SerializeField] private DialogueCondition[] conditions;
        public DialogueLine[] Lines => lines;
        public string SequenceId => sequenceId;
        public DialogueCondition[] Conditions => conditions;

        public void Configure(string id, DialogueLine[] sequence, DialogueCondition[] requirements = null)
        {
            sequenceId = id;
            lines = sequence ?? Array.Empty<DialogueLine>();
            conditions = requirements ?? Array.Empty<DialogueCondition>();
        }

        public bool IsAvailable(MementoMori.Core.GameState state)
        {
            if (conditions == null || conditions.Length == 0) return true;
            foreach (var condition in conditions)
                if (!condition.Matches(state)) return false;
            return true;
        }
    }

    [Serializable]
    public struct DialogueLine
    {
        [TextArea(1, 4)] public string Text;
        public string Speaker;
        [Min(0.01f)] public float CharactersPerSecond;
        [Min(0f)] public float PauseBefore;
        [Min(0f)] public float PauseAfter;
        public bool Thought;
        public bool Narration;
        public bool LockMovement;
    }

    [Serializable]
    public struct DialogueCondition
    {
        public MementoMori.Core.StoryFlag Flag;
        public bool Required;

        public bool Matches(MementoMori.Core.GameState state)
        {
            if (state == null) return !Required;
            return state.HasFlag(Flag) == Required;
        }
    }
}
