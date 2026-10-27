using System;
using System.Collections;
using System.IO;
using MementoMori.Player;
using MementoMori.Core;
using MementoMori.Dialogue;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.EditorTools
{
    // Opt-in Editor-only physical check of the approved narrow M5-M6 opening.
    [InitializeOnLoad]
    public static class MoonLuizaPassageProbe
    {
        const string PendingKey = "MementoMori.MoonLuizaPassageProbe";
        const string ScenePath = "Assets/Scenes/DominioLua.unity";

        static MoonLuizaPassageProbe()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem("Memento Mori/Mapas Lua/Verificar passagem M5-M6")]
        static void Run()
        {
            if (SceneManager.GetActiveScene().path != ScenePath)
                throw new InvalidOperationException("Abra a cena DominioLua antes do teste.");
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Pare o PlayMode antes do teste.");
            SessionState.SetBool(PendingKey, true);
            EditorApplication.isPlaying = true;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(PendingKey, false)) return;
            SessionState.EraseBool(PendingKey);
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlaying)
                    new GameObject("__MoonLuizaPassageProbe").AddComponent<PassageProbeRunner>();
            };
        }

        public sealed class PassageProbeRunner : MonoBehaviour
        {
            IEnumerator Start()
            {
                var output = Path.GetFullPath("TestResults/EnvironmentAudit/Lua_M5_M6_PlayMode.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                var success = false;
                var detail = string.Empty;
                yield return new WaitForSecondsRealtime(2f);
                var player = GameObject.FindGameObjectWithTag("Player");
                var controller = player != null ? player.GetComponent<PlayerController>() : null;
                var body = player != null ? player.GetComponent<Rigidbody2D>() : null;
                if (SceneManager.GetActiveScene().path != ScenePath || controller == null || body == null)
                {
                    detail = "Cena, PlayerController ou Rigidbody2D ausente.";
                }
                else
                {
                    body.position = new Vector2(-4.5f, -78.5f);
                    body.linearVelocity = Vector2.zero;
                    Physics2D.SyncTransforms();
                    yield return new WaitForFixedUpdate();
                    var start = body.position;
                    var hits = Physics2D.CircleCastAll(start, 0.27f, Vector2.down, 5f);
                    var blockers = new System.Text.StringBuilder();
                    foreach (var hit in hits)
                    {
                        if (hit.collider == null || hit.collider.gameObject == player) continue;
                        blockers.Append(hit.collider.gameObject.name).Append('@')
                            .Append(hit.point).Append(" distance=")
                            .Append(hit.distance.ToString("F2")).Append("; ");
                    }
                    var deadline = Time.realtimeSinceStartup + 5f;
                    while (body.position.y > -82f && Time.realtimeSinceStartup < deadline)
                    {
                        controller.SetAutomationMoveInput(Vector2.down);
                        yield return new WaitForFixedUpdate();
                    }
                    controller.ClearAutomationMoveInput();
                    success = start.y > -80f && body.position.y < -82f;
                    detail = $"start={start}; end={body.position}; crossedWallY=-80; success={success}; blockers={blockers}; inputBlocked={InputGate.Instance?.IsBlocked}; dialogueOpen={DialogueManager.Instance?.IsOpen}";
                }
                File.WriteAllText(output, detail + Environment.NewLine);
                Debug.Log("[LUIZA-M5-M6] " + detail);
                EditorApplication.isPlaying = false;
            }
        }
    }
}
