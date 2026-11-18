using System.Collections.Generic;
using MementoMori.Core;
using MementoMori.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.Regions
{
    /// <summary>
    /// Puzzle rooms open in story order: a room whose earlier puzzles are not solved yet cannot be entered.
    /// Stepping onto its floor puts Melantha back where she stood just before and the hint banner says why.
    /// Also the single source the map reads to mark puzzle rooms (locked, current, solved).
    /// </summary>
    public sealed class PuzzleRoomGate : MonoBehaviour
    {
        public enum RoomStatus { Locked, Current, Solved }

        // Order follows the objective chain (ObjectiveToastController).
        static readonly (string id, System.Func<GameState, bool> solved)[] Labyrinth =
        {
            ("L03", s => s.HasFlag(StoryFlag.EchoTrial03Complete) || s.Puzzles.GetSolved("lab.echoes")),
            ("L04", s => s.Puzzles.GetSolved("lab.false_door")),
            ("L05", s => s.Puzzles.GetSolved("lab.voice_well")),
            ("L06", s => s.Puzzles.GetSolved("lab.skyless_courtyard")),
            ("L07", s => s.Puzzles.GetSolved("lab.core")),
            ("L08", s => s.Puzzles.GetSolved("lab.moon_activation") || s.HasFlag(StoryFlag.MoonDomainUnlocked)),
        };
        static readonly (string id, System.Func<GameState, bool> solved)[] Moon =
        {
            ("M02", s => s.HasFlag(StoryFlag.GardenComplete)),
            ("M03", s => s.HasFlag(StoryFlag.MirrorPuzzleComplete)),
            ("M04", s => s.Puzzles.GetSolved("moon.illusory_corridor")),
            ("M05", s => s.Puzzles.GetSolved("moon.cycles")),
            ("M06", s => s.HasFlag(StoryFlag.SigilPuzzleComplete)),
            ("M07", s => s.HasFlag(StoryFlag.FragmentCollected)),
        };

        static (string id, System.Func<GameState, bool> solved)[] Order(string scene) =>
            scene == "Labirinto" ? Labyrinth : scene == "DominioLua" ? Moon : null;

        /// <summary>Status of a puzzle room, or null when the room has no puzzle.</summary>
        public static RoomStatus? Status(string scene, string id)
        {
            var order = Order(scene); var state = GameState.Instance;
            if (order == null || state == null) return null;
            var earlierOpen = true;
            foreach (var room in order)
            {
                var solved = room.solved(state);
                if (room.id == id) return solved ? RoomStatus.Solved : earlierOpen ? RoomStatus.Current : RoomStatus.Locked;
                if (!solved) earlierOpen = false;
            }
            return null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register() { SceneManager.sceneLoaded -= Attach; SceneManager.sceneLoaded += Attach; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AttachOpenScene() => Attach(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        static void Attach(Scene scene, LoadSceneMode mode)
        {
            if (Order(scene.name) == null || FindFirstObjectByType<PuzzleRoomGate>() != null) return;
            var host = new GameObject("PuzzleRoomGate");
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<PuzzleRoomGate>();
        }

        Player.PlayerController player;
        Rigidbody2D body;
        readonly Queue<Vector2> trail = new();
        Vector2 lastFree; bool hasFree;
        float nextMessage;

        void FixedUpdate()
        {
            if (player == null)
            {
                player = FindFirstObjectByType<Player.PlayerController>();
                body = player == null ? null : player.GetComponent<Rigidbody2D>();
                if (player == null) return;
            }
            var scene = gameObject.scene.name;
            Vector2 at = body != null ? body.position : (Vector2)player.transform.position;
            string lockedId = null;
            foreach (var region in RegionTitlePresenter.MapRegions)
            {
                if (Status(scene, region.id) != RoomStatus.Locked) continue;
                foreach (var floor in region.floors)
                    if (floor != null && floor.HasTile(floor.WorldToCell(at))) { lockedId = region.id; break; }
                if (lockedId != null) break;
            }
            if (lockedId == null)
            {
                // Keep a point a few steps back so the push lands clear of the doorway, not on its edge.
                trail.Enqueue(at); while (trail.Count > 12) trail.Dequeue();
                lastFree = trail.Peek(); hasFree = true;
                return;
            }
            // Loaded inside a locked room (old save): never trap her, just let her walk out.
            if (!hasFree) return;
            if (body != null) { body.position = lastFree; body.linearVelocity = Vector2.zero; }
            player.transform.position = new Vector3(lastFree.x, lastFree.y, player.transform.position.z);
            player.ClearAutomationMoveInput();
            trail.Clear(); trail.Enqueue(lastFree);
            if (Time.unscaledTime >= nextMessage)
            {
                nextMessage = Time.unscaledTime + 4f;
                StuckHintPresenter.Say("Ainda não. Algo nesta sala só vai me ouvir depois do que ficou para trás.");
            }
        }
    }
}
