using MementoMori.Core;
using MementoMori.Dialogue;
using MementoMori.Interaction;
using MementoMori.Poe;
using UnityEngine;
using System.Text.RegularExpressions;

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
        // Canonical answers distributed over the four authored corridors; correct indices stay 2,1,3.
        private static readonly int[][] RoundPassages = { new[] { 0, 2, 1 }, new[] { 0, 1, 3 }, new[] { 3, 0, 2 } };
        public int Round => Mathf.Clamp(round, 0, 2);
        public string Question => CanonicalLine(new[] { "E01_D001", "E01_D004", "E01_D007" }[Round]);
        public bool IsPassageAvailable(int passage) => !Solved && System.Array.IndexOf(RoundPassages[Round], passage) >= 0;
        private string CanonicalLine(string id)
        {
            if (!V61EventCatalog.TryGet("E01", out var entry)) return string.Empty;
            foreach (var line in entry.lines) if (line.id == id) return Loc.Line(id, line.text);
            return string.Empty;
        }
        public string ChoiceLabel(int passage)
        {
            var index = System.Array.IndexOf(RoundPassages[Round], passage);
            if (index < 0) return string.Empty;
            var line = CanonicalLine(new[] { "E01_D002", "E01_D005", "E01_D008" }[Round]);
            var options = Regex.Matches(line, @"\[([^\]]+)\]");
            return index < options.Count ? options[index].Groups[1].Value : string.Empty;
        }
        public bool ContainsPlayer(Vector2 point)
        {
            if (passages == null || passages.Length == 0 || passages[0] == null) return false;
            var bounds = new Bounds(passages[0].position, Vector3.zero);
            foreach (var passage in passages) if (passage != null) bounds.Encapsulate(passage.position);
            bounds.Expand(new Vector3(7f, 8f, 0f));
            return point.x >= bounds.min.x && point.x <= bounds.max.x && point.y >= bounds.min.y && point.y <= bounds.max.y;
        }
        [SerializeField] private DialogueData completionDialogue;
        [SerializeField] private Vector2 returnPosition = new(-2.1f, -2.2f);
        [SerializeField] private Transform[] passages;
        int round;
        int errors;
        readonly AudioSource[] whispers = new AudioSource[4];
        Transform listener;
        private void Awake()
        {
            if (completionDialogue == null) completionDialogue = Resources.Load<DialogueData>("Dialogue/DLG_L_ECHO_COMPLETE");
        }
        private void Start()
        {
            var state=GameState.Instance;
            if (GetComponent<MementoMori.UI.EchoCorridorPrompt>() == null) gameObject.AddComponent<MementoMori.UI.EchoCorridorPrompt>();
            if(state!=null) round=state.HasFlag(StoryFlag.EchoTrial03Complete)?3:state.HasFlag(StoryFlag.EchoTrial02Complete)?2:state.HasFlag(StoryFlag.EchoTrial01Complete)?1:0;
        }
        // Each passage murmurs short fragments with silences in between (never a constant drone). The correct
        // passage speaks a little more often and more clearly, which is the clue. XY attenuation/pan avoids the
        // camera Z distance silencing positional audio in a 2D game.
        readonly float[] phraseStart = new float[4], phraseEnd = new float[4], nextPhrase = new float[4];
        private void Update()
        {
            if (listener == null) listener = Object.FindAnyObjectByType<MementoMori.Player.PlayerController>()?.transform;
            var audible = !Solved && listener != null && ContainsPlayer(listener.position);
            var now = Time.unscaledTime;
            for (var i = 0; i < whispers.Length; i++)
            {
                if (!audible || !IsPassageAvailable(i) || passages == null || i >= passages.Length || passages[i] == null)
                { if (whispers[i] != null && whispers[i].isPlaying) whispers[i].Stop(); phraseEnd[i] = 0f; continue; }
                if (whispers[i] == null && !CreateVoice(i)) continue;
                var voice = whispers[i];
                var correct = i == CorrectPassages[Round];
                if (now >= phraseEnd[i] && now >= nextPhrase[i])
                {
                    // Start a fragment: random point of the recording, 1.4-3 s long.
                    var length = Random.Range(1.4f, 3f);
                    voice.time = Random.Range(0f, Mathf.Max(0f, voice.clip.length - length - .1f));
                    voice.pitch = (correct ? .97f : .84f + i * .05f) * Random.Range(.97f, 1.03f);
                    voice.GetComponent<AudioLowPassFilter>().cutoffFrequency = correct ? 5200f : 2600f;
                    phraseStart[i] = now; phraseEnd[i] = now + length;
                    nextPhrase[i] = phraseEnd[i] + (correct ? Random.Range(1.2f, 2.6f) : Random.Range(2.4f, 5.5f));
                    if (!voice.isPlaying) voice.Play();
                }
                if (now >= phraseEnd[i]) { if (voice.isPlaying) voice.Stop(); continue; }
                var envelope = Mathf.Clamp01((now - phraseStart[i]) / .3f) * Mathf.Clamp01((phraseEnd[i] - now) / .45f);
                var delta = (Vector2)passages[i].position - (Vector2)listener.position;
                var falloff = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(1.5f, 18f, delta.magnitude));
                var effects = AccessibilitySettings.Instance == null ? .65f : AccessibilitySettings.Instance.EffectsVolume;
                voice.volume = (correct ? .5f : .3f) * effects * falloff * envelope;
                voice.panStereo = Mathf.Clamp(delta.x / 8f, -.85f, .85f);
                voice.transform.position = passages[i].position;
            }
        }

        private bool CreateVoice(int i)
        {
            var clip = Resources.Load<AudioClip>("Audio/21_echo_whisper_loop");
            if (clip == null) return false;
            var go = new GameObject("EchoWhisper_" + (i + 1));
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            var mixer = Resources.Load<UnityEngine.Audio.AudioMixer>("MementoMoriMixer");
            var groups = mixer == null ? null : mixer.FindMatchingGroups("SFX");
            if (groups != null && groups.Length > 0) source.outputAudioMixerGroup = groups[0];
            source.clip = clip; source.loop = false; source.playOnAwake = false; source.spatialBlend = 0f;
            go.AddComponent<AudioLowPassFilter>().cutoffFrequency = 3400f;
            var reverb = go.AddComponent<AudioReverbFilter>();
            reverb.reverbPreset = AudioReverbPreset.User;
            reverb.room = -1400f; reverb.reverbLevel = -900f; reverb.decayTime = 2.2f; reverb.reflectionsDelay = .04f;
            whispers[i] = source;
            // Stagger the first fragments so the four voices never start together.
            nextPhrase[i] = Time.unscaledTime + .4f + i * 1.1f;
            return true;
        }
        private void OnDisable() { foreach (var source in whispers) if (source != null) source.Stop(); }
        public bool Solved => GameState.Instance != null && GameState.Instance.HasFlag(StoryFlag.EchoTrial03Complete);
        public void Configure(DialogueData dialogue, Transform[] corridorPassages)
        {
            completionDialogue = dialogue;
            passages = corridorPassages;
        }
        public bool Select(int passage, Transform player)
        {
            if (Solved || round >= CorrectPassages.Length || !IsPassageAvailable(passage)) return false;
            GrimoireCatalog.Discover("P0" + (round + 4));
            if (passage != CorrectPassages[round])
            {
                errors++;
                GameState.Instance?.IncrementCounter("echo.errors");
                MementoMori.Audio.RuntimeAudio.PlayOneShot("10_sigil_error", .28f);
                if (player != null)
                {
                    player.position = returnPosition;
                    var body = player.GetComponent<Rigidbody2D>();
                    if (body != null) { body.position = returnPosition; body.linearVelocity = Vector2.zero; }
                    player.GetComponent<MementoMori.Player.PlayerController>()?.ClearAutomationMoveInput();
                }
                if (errors >= 3 && passages != null && CorrectPassages[round] < passages.Length)
                    Object.FindAnyObjectByType<PoeFollower>()?.HintAt(passages[CorrectPassages[round]].position);
                return false;
            }
            errors = 0;
            round++;
            GamepadInput.Rumble(.2f, .3f, round == 3 ? .4f : .15f);
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

