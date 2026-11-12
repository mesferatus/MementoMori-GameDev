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
        private static readonly string[] Names = { "Lua", "Livro", "Memória", "Repouso" };
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
                SayV61(0, 2);
                return;
            }
            if (!state.HasFlag(StoryFlag.RoomRitualItemStored))
            {
                SayV61(2, 2);
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
                SayV61(8, 1);
                return;
            }
            var next = state.IncrementPuzzleProgress(ProgressId);
            RuntimeAudio.PlayOneShot("22_candle_ignite", .45f);
            if (next == 4) state.SetFlag(StoryFlag.RoomCandlesDone);
            if (next == 1) SayV61(4, 4);
            else if (next == 4) SayV61(9, 2, true);
            else Say("DLG_Q_CANDLE_ORDER_SUCCESS");
        }

        private static void SayV61(int first, int count, bool completesEvent = false)
        {
            var dialogue = V61EventCatalog.CreateDialogueSegment("Q03", "Q03." + first, first, count);
            var manager = DialogueManager.Instance;
            if (dialogue == null || manager == null || !manager.TryStartDialogue(dialogue))
            {
                if (dialogue != null) Destroy(dialogue);
                return;
            }
            void Complete()
            {
                manager.OnDialogueCompleted -= Complete;
                if (completesEvent) GameState.Instance?.SetCounter("event.Q03", 1);
                Destroy(dialogue);
            }
            manager.OnDialogueCompleted += Complete;
        }

        private static void Say(string id)
        {
            DialogueManager.Instance?.StartDialogue(Resources.Load<DialogueData>("Dialogue/" + id));
        }

        public static string NameFor(int index) => index >= 0 && index < Names.Length ? Names[index] : "vela";
    }

}
