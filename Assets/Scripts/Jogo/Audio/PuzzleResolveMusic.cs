using MementoMori.Core;
using UnityEngine;

namespace MementoMori.Audio
{
    /// <summary>A violin phrase that resolves into a major chord whenever a puzzle (or its last step) is solved.</summary>
    public sealed class PuzzleResolveMusic : MonoBehaviour
    {
        static readonly StoryFlag[] Resolutions =
        {
            StoryFlag.EchoTrial03Complete, StoryFlag.VoiceWellComplete, StoryFlag.GardenComplete,
            StoryFlag.MirrorPuzzleComplete, StoryFlag.SigilPuzzleComplete, StoryFlag.FragmentCollected,
            StoryFlag.RoomCandlesDone, StoryFlag.FalseDoorTriggered,
        };
        GameState hooked;
        float lastAt = -10f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create()
        {
            if (FindAnyObjectByType<PuzzleResolveMusic>() != null) return;
            var go = new GameObject("PuzzleResolveMusic");
            DontDestroyOnLoad(go);
            go.AddComponent<PuzzleResolveMusic>();
        }

        void Update()
        {
            var state = GameState.Instance;
            if (state == hooked) return;
            if (hooked != null) { hooked.OnFlagChanged -= OnFlag; hooked.Puzzles.PuzzleSolved -= OnPuzzle; }
            hooked = state;
            if (hooked != null) { hooked.OnFlagChanged += OnFlag; hooked.Puzzles.PuzzleSolved += OnPuzzle; }
        }

        void OnFlag(StoryFlag flag, bool value)
        {
            if (value && System.Array.IndexOf(Resolutions, flag) >= 0) Play();
        }

        void OnPuzzle(string id) => Play();

        void Play()
        {
            // Several flags can land in the same moment (a puzzle id and its flag): one phrase only.
            if (Time.unscaledTime - lastAt < 3f) return;
            lastAt = Time.unscaledTime;
            RuntimeAudio.PlayOneShot("31_violin_resolve", .55f);
        }

        void OnDestroy()
        {
            if (hooked != null) { hooked.OnFlagChanged -= OnFlag; hooked.Puzzles.PuzzleSolved -= OnPuzzle; }
        }
    }
}
