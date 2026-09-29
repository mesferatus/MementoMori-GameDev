using MementoMori.Audio;
using MementoMori.Core;
using MementoMori.Dialogue;
using UnityEngine;

namespace MementoMori.World
{
    /// <summary>Four room candles follow the order written in the grimoire.</summary>
    public sealed class RoomCandlePuzzle : MonoBehaviour
    {
        public const string ProgressId = "Q_CANDLE_ORDER";
        private static readonly string[] Names = { "lua", "grimório", "retrato", "cama" };
        public int Progress => GameState.Instance?.GetPuzzleProgress(ProgressId) ?? 0;
        public bool Completed => GameState.Instance != null && GameState.Instance.HasFlag(StoryFlag.RoomCandlesDone);

        public void Activate(int index)
        {
            var state = GameState.Instance;
            if (state == null || Completed) return;
            GrimoireCatalog.Discover("A04");
            GrimoireCatalog.Discover("R02");
            GrimoireCatalog.Discover("P01");
            if (!state.HasFlag(StoryFlag.RoomWindowSecured))
            {
                Say("DLG_Q_CANDLE_FAIL_WIND");
                return;
            }
            if (!state.HasFlag(StoryFlag.RoomRitualItemStored))
            {
                Say("DLG_Q_CANDLE_BEFORE_ANCHOR");
                return;
            }
            if (!state.HasFlag(StoryFlag.RoomGrimoireRead))
            {
                Say("DLG_Q_CANDLE_BEFORE_ANCHOR");
                return;
            }
            if (index != Progress)
            {
                state.SetPuzzleProgress(ProgressId, 0);
                Say("DLG_Q_CANDLE_ORDER_ERROR");
                return;
            }
            var next = state.IncrementPuzzleProgress(ProgressId);
            RuntimeAudio.PlayOneShot("18_candle_extinguish", .45f);
            if (next == 4) state.SetFlag(StoryFlag.RoomCandlesDone);
            Say("DLG_Q_CANDLE_ORDER_SUCCESS");
        }

        private static void Say(string id)
        {
            DialogueManager.Instance?.StartDialogue(Resources.Load<DialogueData>("Dialogue/" + id));
        }

        public static string NameFor(int index) => index >= 0 && index < Names.Length ? Names[index] : "vela";
    }

}
