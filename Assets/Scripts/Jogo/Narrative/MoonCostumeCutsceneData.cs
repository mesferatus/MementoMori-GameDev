using UnityEngine;

namespace MementoMori.Narrative
{
    /// <summary>Approved clips the M01 costume cutscene needs at runtime that no Moon Domain controller carries.</summary>
    public sealed class MoonCostumeCutsceneData : ScriptableObject
    {
        [Tooltip("Melantha's lunar sigil pose (melantha_ritual_cast).")]
        public AnimationClip melanthaGlow;
    }
}
