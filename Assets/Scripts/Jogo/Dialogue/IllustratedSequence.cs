using System;
using UnityEngine;

namespace MementoMori.Dialogue
{
    [CreateAssetMenu(menuName = "Memento Mori/Illustrated Sequence")]
    public sealed class IllustratedSequence : ScriptableObject
    {
        public string eventId;
        public bool allowSkip = true;
        public bool finalArtwork;
        public Font pixelFont;
        public Material lineArtMaterial;
        public IllustratedBeat[] beats = Array.Empty<IllustratedBeat>();
    }

    [Serializable]
    public sealed class IllustratedBeat
    {
        public int beforeLine;
        public string action;
        [TextArea(10, 40)] public string[] asciiFrames = Array.Empty<string>();
        public IllustratedFrameVariant[] choiceVariants = Array.Empty<IllustratedFrameVariant>();
        public string[] FramesFor(MementoMori.Core.GameState state)
        {
            if (choiceVariants != null)
                foreach (var variant in choiceVariants)
                    if (variant != null && variant.asciiFrames != null && variant.asciiFrames.Length > 0
                        && V61ChoiceReactivity.SelectedOption(state, variant.choiceId) == variant.optionId)
                        return variant.asciiFrames;
            return asciiFrames;
        }
        public Sprite prop;
        public Vector2 propPosition = new(.6f, .25f);
        public Sprite[] melanthaFrames = Array.Empty<Sprite>();
        public Sprite[] poeFrames = Array.Empty<Sprite>();
        public Sprite[] andrealphusFrames = Array.Empty<Sprite>();
        public Vector2 melanthaPosition = new(.45f, .45f);
        public Vector2 poePosition = new(.72f, .24f);
        public Vector2 andrealphusPosition = new(.75f, .52f);
        public float actorHeight = .8f;
        [Min(.1f)] public float duration = 1.2f;
        public string sound;
    }

    [Serializable]
    public sealed class IllustratedFrameVariant
    {
        public string choiceId;
        public string optionId;
        [TextArea(10, 40)] public string[] asciiFrames = Array.Empty<string>();
    }
}
