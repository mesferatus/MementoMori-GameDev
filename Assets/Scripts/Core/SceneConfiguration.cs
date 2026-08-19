using UnityEngine;

namespace MementoMori.Core
{
    /// <summary>Serialized beta contract visible in the Inspector for each authored scene.</summary>
    public sealed class SceneConfiguration : MonoBehaviour
    {
        [SerializeField] private string sceneId;
        [SerializeField] private string nextScene;
        [SerializeField] private bool requiresRitual;
        [SerializeField] private string[] domainAreas = new string[0];
        [SerializeField] private string[] mirrorSymbols = new string[0];
        [SerializeField] private string[] mirrorCorrectSymbols = new string[0];
        [SerializeField] private string[] sigilSequence = new string[0];
        [SerializeField] private string[] dialogueKeys = new string[0];

        public string SceneId => sceneId;
        public string NextScene => nextScene;
        public bool RequiresRitual => requiresRitual;
        public string[] DomainAreas => domainAreas ?? new string[0];
        public string[] MirrorSymbols => mirrorSymbols ?? new string[0];
        public string[] MirrorCorrectSymbols => mirrorCorrectSymbols ?? new string[0];
        public string[] SigilSequence => sigilSequence ?? new string[0];
        public string[] DialogueKeys => dialogueKeys ?? new string[0];

        public void Configure(string id, string destination, bool ritualRequired, string[] areas, string[] mirrors, string[] correctMirrors, string[] sigils, string[] dialogues)
        {
            sceneId = id;
            nextScene = destination;
            requiresRitual = ritualRequired;
            domainAreas = areas ?? new string[0];
            mirrorSymbols = mirrors ?? new string[0];
            mirrorCorrectSymbols = correctMirrors ?? new string[0];
            sigilSequence = sigils ?? new string[0];
            dialogueKeys = dialogues ?? new string[0];
        }
    }
}
