using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MementoMori.Core;
using MementoMori.Interaction;
using MementoMori.Puzzles;
using MementoMori.UI;
using MementoMori.World;
using MementoMori.Dialogue;
using MementoMori.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MementoMori.Verification
{
    /// <summary>
    /// Opt-in acceptance trace for the academic beta. It is not included in release builds and
    /// runs only when a Development Build (or the Editor) receives --ct-evidence=&lt;file&gt;.
    /// The trace drives the same scene services and puzzle controllers used by the player.
    /// </summary>
    public sealed class CtEvidenceRunner : MonoBehaviour
    {
        [Serializable] private sealed class Entry { public string id; public bool passed; public string detail; }
        [Serializable] private sealed class Report { public string target; public string utc; public Entry[] checks; public bool passed; public string error; }

        private readonly List<Entry> entries = new();
        private string outputPath;
        private bool running;
        private string error;
        private string crescentTrace = "not-run"; // CT-007 physical route trace

#if UNITY_EDITOR
        public static void RunBatchEvidence()
        {
            var path = Path.GetFullPath("TestResults/ct-evidence-current.json");
            if (File.Exists(path)) File.Delete(path);
            UnityEditor.SessionState.SetString("MementoMori.CtEvidencePath", path);
            UnityEditor.EditorApplication.update += ExitBatchEvidence;
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
            UnityEditor.EditorApplication.isPlaying = true;
        }

        private static void ExitBatchEvidence()
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) return;
            UnityEditor.EditorApplication.update -= ExitBatchEvidence;
            UnityEditor.EditorApplication.Exit(File.Exists(Path.GetFullPath("TestResults/ct-evidence-current.json")) ? 0 : 1);
        }

        public static void StartEditorEvidence(string path)
        {
            var existing = FindFirstObjectByType<CtEvidenceRunner>();
            if (existing != null)
            {
                existing.outputPath = path;
                existing.BeginIfNeeded();
                Debug.Log($"[CT-EVIDENCE] Existing Editor runner configured: {path}");
                return;
            }
            var host = new GameObject("__CtEvidenceRunner");
            if (Application.isPlaying) DontDestroyOnLoad(host);
            var runner = host.AddComponent<CtEvidenceRunner>();
            runner.outputPath = path;
            runner.BeginIfNeeded();
            Debug.Log($"[CT-EVIDENCE] Editor runner created: {path}");
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartFromCommandLine()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#if UNITY_EDITOR
            var editorPath = UnityEditor.SessionState.GetString("MementoMori.CtEvidencePath", string.Empty);
            if (!string.IsNullOrWhiteSpace(editorPath))
            {
                UnityEditor.SessionState.EraseString("MementoMori.CtEvidencePath");
                CreateRunner(editorPath);
                return;
            }
#endif
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (!arg.StartsWith("--ct-evidence=", StringComparison.OrdinalIgnoreCase)) continue;
                CreateRunner(arg.Substring("--ct-evidence=".Length).Trim('\"'));
                return;
            }
#endif
        }

        private static void CreateRunner(string path)
        {
            var host = new GameObject("__CtEvidenceRunner");
            DontDestroyOnLoad(host);
            host.AddComponent<CtEvidenceRunner>().outputPath = path;
        }

        private void Start()
        {
            BeginIfNeeded();
        }

        private void BeginIfNeeded()
        {
            if (!Application.isPlaying || string.IsNullOrWhiteSpace(outputPath) || running) return;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            running = true;
            // Graphical Editor evidence can be launched without a focused Game view.
            // Keep the real scene coroutines advancing in that supported execution mode.
            Application.runInBackground = true;
            Time.timeScale = 1f;
            yield return RunChecks();
        }

        private IEnumerator RunChecks()
        {
            try
            {
            if (SceneManager.GetActiveScene().name != "MainMenu")
                SceneManager.LoadScene("MainMenu");
            yield return WaitForScene("MainMenu");
            var mainMenuDeadline = Time.realtimeSinceStartup + 5f;
            while (GameObject.Find("MainMenuCanvas") == null && Time.realtimeSinceStartup < mainMenuDeadline)
                yield return null;
            Check("CT-001", GameObject.Find("MainMenuCanvas") != null, "MainMenu construÃ­do sem exceÃ§Ã£o.");

            GameManager.Instance.StartNewGame();
            yield return WaitForScene("Quarto");
            Check("CT-002", SceneManager.GetActiveScene().name == "Quarto", "SessÃ£o limpa chegou ao Quarto.");

            var bed = FindFirstObjectByType<BedController>();
            var locked = bed != null && !BedController.HasRoomRequirements(GameState.Instance);
            foreach (var objectName in new[] { "PoeBowl", "PoeToy", "Photo", "Window", "Candles", "RitualItem", "Grimoire" })
            {
                var trigger = GameObject.Find(objectName)?.GetComponent<DialogueTrigger>();
                if (trigger != null && trigger.CanInteract(default)) trigger.Interact(default);
                yield return null;
            }
            Check("CT-003", locked && BedController.HasRoomRequirements(GameState.Instance), "Bloqueio da cama e as sete interaÃ§Ãµes do quarto foram verificados.");

            // The normal UI invokes this same state change before the 30â€“50 second transition.
            bed?.Interact(new MementoMori.Interaction.InteractionContext(GameObject.FindGameObjectWithTag("Player")));
            bed?.ConfirmSleep();
            // Preserve the actual dream transition: this CT must wait for BedController's
            // coroutine instead of forcing a scene load before DreamTransitionComplete exists.
            yield return WaitForScene("Labirinto", 45f);
            Check("CT-004", GameState.Instance.HasFlag(StoryFlag.DreamTransitionComplete), "TransiÃ§Ã£o onÃ­rica e destino Labirinto confirmados.");

            var falseDoor = FindFirstObjectByType<FalseDoorController>();
            falseDoor?.Interact(default);
            var player = GameObject.FindGameObjectWithTag("Player")?.transform;
            var revealTrigger = GameObject.Find("PoeReveal")?.transform;
            if (player != null && revealTrigger != null)
            {
                player.position = revealTrigger.position;
                Physics2D.SyncTransforms();
                yield return new WaitForFixedUpdate();
            }
            Check("CT-005", GameState.Instance.PoeRevealed && GameObject.Find("PoeRouteTrigger_01") != null && GameObject.Find("PoeRouteTrigger_02") != null, "RevelaÃ§Ã£o e dois pontos de Poe disponÃ­veis.");

            var echoes = FindFirstObjectByType<EchoCorridorPuzzle>();
            var preserved = echoes != null && !echoes.Select(0, player) && echoes.Select(2, player) && echoes.Select(1, player) && echoes.Select(3, player);
            Check("CT-006", GameState.Instance.HasFlag(StoryFlag.FalseDoorTriggered) && preserved && GameState.Instance.HasFlag(StoryFlag.EchoTrial03Complete), "Porta falsa e trÃªs rodadas recuperÃ¡veis dos Ecos concluÃ­das.");

            var domainPortal = GameObject.Find("MoonPortal")?.GetComponent<Portal>();
            if (domainPortal == null)
            {
                Check("CT-007", false, "MoonPortal ausente antes dos puzzles do Dominio.");
                Check("CT-008", false, "Skipped because CT-007 did not reach DominioLua.");
                Check("CT-009", false, "Skipped because CT-007 did not reach DominioLua.");
                yield break;
            }
            domainPortal.Interact(default);
            yield return WaitForScene("DominioLua");

            var petals = FindObjectsByType<GardenPetalPuzzle>(FindObjectsSortMode.None);
            Array.Sort(petals, (left, right) => left.Petal.CompareTo(right.Petal));
            var interactor = GameObject.FindGameObjectWithTag("Player");
            foreach (var petal in petals)
            {
                if (petal.Petal == MoonPetal.Minguante)
                {
                    foreach (var index in new[] { 2, 1, 0 })
                    {
                        InvokeInteraction(GameObject.Find("FlorMinguante_" + (index + 1)), interactor);
                        yield return null;
                    }
                }
                else if (petal.Petal == MoonPetal.Crescente)
                {
                    var crescentFollowers = FindObjectsByType<MementoMori.Poe.PoeFollower>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    var crescentFollower = Array.Find(crescentFollowers, follower => follower.gameObject.activeInHierarchy) ?? (crescentFollowers.Length == 0 ? null : crescentFollowers[0]);
                    petal.ConfigureCrescentRule(interactor?.transform, crescentFollower);
                    yield return MoveInteractorOutsideCrescent(interactor, petal.transform, 3.5f);
                    var crescentDeadline = Time.realtimeSinceStartup + 20f;
                    while (Time.realtimeSinceStartup < crescentDeadline && !petal.CanCollect(interactor?.transform))
                    {
                        yield return null;
                    }
                    crescentTrace += $" | crescentOpen={petal.CanCollect(interactor?.transform)}; finalPlayer={interactor?.transform.position}; finalPoe={(FindFirstObjectByType<MementoMori.Poe.PoeFollower>() == null ? "missing" : FindFirstObjectByType<MementoMori.Poe.PoeFollower>().transform.position.ToString())}";
                    InvokeInteraction(petal.gameObject, interactor);
                    yield return null;
                }
                else
                {
                    var reflected = GameObject.Find("PetalaCheia_Reflexo");
                    if (interactor != null && reflected != null) interactor.transform.position = reflected.transform.position;
                    // The real interaction lives at the reflection, not on the source petal.
                    InvokeInteraction(reflected, interactor);
                    yield return null;
                }
            }
            var gardenDeadline = Time.realtimeSinceStartup + 5f;
            while (!GameState.Instance.HasFlag(StoryFlag.GardenComplete) && Time.realtimeSinceStartup < gardenDeadline)
                yield return null;
            var mirrors = FindFirstObjectByType<PuzzleMirror>();
            var mirrorSymbols = FindObjectsByType<MirrorSymbol>(FindObjectsSortMode.None);
            if (mirrors != null) foreach (var symbol in mirrorSymbols) if (symbol.SymbolId == "Present") InvokeInteraction(symbol.gameObject, interactor);
            if (mirrors != null) foreach (var id in new[] { "Delayed", "Ahead", "Absent" }) foreach (var symbol in mirrorSymbols) if (symbol.SymbolId == id) InvokeInteraction(symbol.gameObject, interactor);
            var gardenComplete = GameState.Instance.HasFlag(StoryFlag.GardenComplete);
            var mirrorSolved = GameState.Instance.MirrorPuzzleSolved;
            var crescent = Array.Find(petals, current => current.Petal == MoonPetal.Crescente);
            var poes = FindObjectsByType<MementoMori.Poe.PoeFollower>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var playerDistance = crescent == null || interactor == null ? -1f : Vector2.Distance(interactor.transform.position, crescent.transform.position);
            var poeDistance = crescent == null || poes.Length == 0 ? -1f : Vector2.Distance(poes[0].transform.position, crescent.transform.position);
            Check("CT-007", gardenComplete && mirrors != null && mirrors.ErrorCount == 1 && mirrorSolved, $"Jardim={gardenComplete}; espelhos={(mirrors == null ? "ausentes" : mirrors.State.ToString())}; erros={(mirrors == null ? -1 : mirrors.ErrorCount)}; concluÃ­do={mirrorSolved}; pÃ©talas={petals.Length}; jogador={playerDistance};poe={poeDistance};poeAtivo={(poes.Length > 0 && poes[0].gameObject.activeInHierarchy)}; rota={crescentTrace}.");

            var gallery = GameObject.Find("GalleryDoor_Cheia") != null && GameObject.Find("GalleryHiddenWall") != null;
            GameObject.Find("GalleryHiddenWall")?.GetComponent<DialogueTrigger>()?.Interact(default);
            Check("CT-008", gallery && GameState.Instance.HasFlag(StoryFlag.HiddenDoorRevealed), "Galeria, porta Cheia falsa e parede sem sÃ­mbolo verificados.");

            var sigil = FindFirstObjectByType<SigilRingPuzzle>();
            var phaseRing = GameObject.Find("SigilRing_Fases");
            var memoryRing = GameObject.Find("SigilRing_MemÃ³rias");
            var intentionRing = GameObject.Find("SigilRing_IntenÃ§Ã£o");
            if (phaseRing != null) for (var i = 0; i < 3; i++) InvokeInteraction(phaseRing, interactor);
            if (memoryRing != null) for (var i = 0; i < 4; i++) InvokeInteraction(memoryRing, interactor);
            if (intentionRing != null) InvokeInteraction(intentionRing, interactor);
            // Names with accented characters are locale-sensitive in generated scenes. Fall back
            // to the real ring components so the interaction sequence remains gameplay-driven.
            foreach (var ring in FindObjectsByType<SigilRingInteractable>(FindObjectsSortMode.None))
            {
                var rotations = ring.Ring == SigilRing.Phase ? 3 : ring.Ring == SigilRing.Memory ? 3 : 1;
                for (var i = 0; i < rotations; i++) InvokeInteraction(ring.gameObject, interactor);
            }
            var retained = sigil != null && sigil.GetProgress() == 3;
            Check("CT-009", retained && sigil.Solved, $"AnÃ©is={(sigil == null ? "ausente" : sigil.GetProgress().ToString())}; resolvido={(sigil != null && sigil.Solved)}.");

            var finalPortal = GameObject.Find("MoonPortalArt")?.GetComponent<Portal>();
            Check("CT-010", finalPortal != null && !finalPortal.CanInteract(default), "Portal final permanece bloqueado antes do fragmento.");

            var pause = FindFirstObjectByType<PauseMenuController>();
            pause?.Toggle(); var paused = Time.timeScale == 0f; pause?.Toggle();
            var fragment = FindFirstObjectByType<FragmentCollectible>();
            if (fragment == null)
            {
                Check("CT-011", false, "FragmentCollectible not found in DominioLua.");
                yield break;
            }
            fragment.Interact(default);
            yield return WaitForScene("FinalBeta");
            var fragmentCollected = GameState.Instance.FragmentCollected;
            GameManager.Instance.ReturnToMenu();
            yield return WaitForScene("MainMenu");
            Check("CT-011", fragmentCollected && paused && SceneManager.GetActiveScene().name == "MainMenu", "Fragmento, FinalBeta, pausa e retorno ao menu concluÃ­dos.");

            }
            finally
            {
                if (entries.Count != 11 && string.IsNullOrEmpty(error))
                    error = $"CT trace interrupted after {entries.Count} of 11 checks; inspect Editor.log for the exception.";
                WriteReport();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit(entries.Exists(entry => !entry.passed) || !string.IsNullOrEmpty(error) ? 1 : 0);
#endif
            }
        }

        private IEnumerator WaitForScene(string expected, float timeout = 20f)
        {
            var deadline = Time.realtimeSinceStartup + timeout;
            while ((SceneManager.GetActiveScene().name != expected || GameManager.Instance == null) && Time.realtimeSinceStartup < deadline) yield return null;
            yield return null;
            yield return null;
            // A cena Ã© construÃ­da em Awake/Start e contÃ©m vÃ¡rios objetos runtime.
            // Aguarde a montagem completa antes de consultar os contratos dos CTs.
            yield return new WaitForSecondsRealtime(2.5f);
        }

        private void Check(string id, bool passed, string detail)
        {
            entries.Add(new Entry { id = id, passed = passed, detail = detail });
            Debug.Log($"[CT-EVIDENCE] {id}: {(passed ? "PASS" : "FAIL")} - {detail}");
        }

        private static bool InvokeInteraction(GameObject target, GameObject interactor)
        {
            var interactable = target == null ? null : target.GetComponent<IInteractable>();
            if (interactable == null) return false;
            var context = new InteractionContext(interactor);
            if (!interactable.CanInteract(context)) return false;
            interactable.Interact(context);
            return true;
        }

        private IEnumerator MoveInteractorOutsideCrescent(GameObject interactor, Transform crescent, float minimumDistance)
        {
            if (interactor == null || crescent == null)
            {
                crescentTrace = "invalid-player-or-crescent";
                yield break;
            }
            var controller = interactor.GetComponent<PlayerController>();
            if (controller == null)
            {
                crescentTrace = "missing-player-controller";
                yield break;
            }
            var hitbox = interactor.GetComponent<Collider2D>();
            var radius = hitbox == null ? .3f : Mathf.Clamp(Mathf.Max(hitbox.bounds.extents.x, hitbox.bounds.extents.y) * .8f, .15f, .45f);
            var start = (Vector2)interactor.transform.position;
            var radialBlockers = DescribeRadialBlockers(interactor, start, crescent.position, minimumDistance, radius);
            var route = FindNavigableCrescentRoute(interactor, start, crescent.position, minimumDistance, radius);
            if (route == null || route.Count == 0)
            {
                crescentTrace = $"DIRECT_RADIAL_PATH=BLOCKED; blockers={radialBlockers}; route=not-found";
                yield break;
            }

            var followers = FindObjectsByType<MementoMori.Poe.PoeFollower>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var poe = Array.Find(followers, follower => follower.gameObject.activeInHierarchy) ?? (followers.Length == 0 ? null : followers[0]);
            if (poe == null)
            {
                crescentTrace = $"DIRECT_RADIAL_PATH=BLOCKED; blockers={radialBlockers}; route=found; POE_FOLLOW_POSITIONING_BUG=missing-follower";
                yield break;
            }
            foreach (var follower in followers)
            {
                follower.Configure(interactor.transform, 3f, 1.1f);
                follower.BeginFollowing();
                follower.HintAt(crescent.position);
            }
            var trace = new List<string> { $"DIRECT_RADIAL_PATH=BLOCKED", $"blockers={radialBlockers}", $"start={start}", $"crescent={crescent.position}", $"target={route[route.Count - 1]}", $"requiredDistance={minimumDistance}" };
            for (var index = 1; index < route.Count; index++)
            {
                var waypoint = route[index];
                var deadline = Time.realtimeSinceStartup + Mathf.Max(1.5f, Vector2.Distance(interactor.transform.position, waypoint) / 2f + 1f);
                var blocked = false;
                while (Vector2.Distance(interactor.transform.position, waypoint) > .08f && Time.realtimeSinceStartup < deadline)
                {
                    controller.SetAutomationMoveInput((waypoint - (Vector2)interactor.transform.position).normalized);
                    yield return new WaitForFixedUpdate();
                }
                blocked = Vector2.Distance(interactor.transform.position, waypoint) > .12f;
                trace.Add($"wp{index}={interactor.transform.position};d={Vector2.Distance(interactor.transform.position, waypoint):F2};poe={(poe == null ? "missing" : poe.transform.position.ToString())};blocked={blocked}");
                if (blocked) break;
            }
            controller.ClearAutomationMoveInput();
            Physics2D.SyncTransforms();
            crescentTrace = string.Join(" | ", trace);
        }

        private static List<Vector2> FindNavigableCrescentRoute(GameObject interactor, Vector2 start, Vector2 crescent, float minimumDistance, float radius)
        {
            const float cellSize = .5f;
            const int searchRadius = 32;
            var queue = new Queue<Vector2Int>();
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            var origin = new Vector2Int(0, 0);
            queue.Enqueue(origin);
            visited.Add(origin);
            Vector2Int? goal = null;
            var directions = new[] { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down };
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                var world = start + (Vector2)node * cellSize;
                var distance = Vector2.Distance(world, crescent);
                if (distance >= minimumDistance && distance <= minimumDistance + .75f && IsNavigable(interactor, world, radius))
                {
                    goal = node;
                    break;
                }
                foreach (var cardinal in directions)
                {
                    var next = node + cardinal;
                    if (Mathf.Abs(next.x) > searchRadius || Mathf.Abs(next.y) > searchRadius || !visited.Add(next)) continue;
                    var nextWorld = start + (Vector2)next * cellSize;
                    if (!IsNavigable(interactor, nextWorld, radius) || !IsClear(interactor, world, nextWorld, radius)) continue;
                    cameFrom[next] = node;
                    queue.Enqueue(next);
                }
            }
            if (!goal.HasValue) return null;
            var route = new List<Vector2>();
            for (var node = goal.Value; ; node = cameFrom[node])
            {
                route.Add(start + (Vector2)node * cellSize);
                if (node == origin) break;
            }
            route.Reverse();
            return route;
        }

        private static bool IsNavigable(GameObject interactor, Vector2 point, float radius)
        {
            foreach (var hit in Physics2D.OverlapCircleAll(point, radius))
                if (IsSolidBlocker(hit, interactor)) return false;
            return true;
        }

        private static bool IsClear(GameObject interactor, Vector2 from, Vector2 to, float radius)
        {
            var delta = to - from;
            foreach (var hit in Physics2D.CircleCastAll(from, radius, delta.normalized, delta.magnitude))
                if (IsSolidBlocker(hit.collider, interactor)) return false;
            return true;
        }

        private static string DescribeRadialBlockers(GameObject interactor, Vector2 start, Vector2 crescent, float minimumDistance, float radius)
        {
            var outward = (start - crescent).normalized;
            if (outward.sqrMagnitude < .01f) outward = Vector2.left;
            var names = new List<string>();
            foreach (var hit in Physics2D.CircleCastAll(start, radius, outward, minimumDistance))
            {
                if (!IsSolidBlocker(hit.collider, interactor)) continue;
                var description = $"{hit.collider.gameObject.name}:{hit.collider.GetType().Name}:bounds={hit.collider.bounds}:layer={LayerMask.LayerToName(hit.collider.gameObject.layer)}:position={hit.collider.transform.position}";
                if (!names.Contains(description)) names.Add(description);
            }
            return names.Count == 0 ? "none-reported" : string.Join(";", names);
        }

        private static bool IsSolidBlocker(Collider2D collider, GameObject interactor) => collider != null && collider.enabled && collider.gameObject.activeInHierarchy && !collider.isTrigger && collider.gameObject != interactor;

        private void WriteReport()
        {
            var report = new Report { target = Application.isEditor ? "Editor" : "WindowsDevelopmentBuild", utc = DateTime.UtcNow.ToString("O"), checks = entries.ToArray(), passed = string.IsNullOrEmpty(error) && !entries.Exists(entry => !entry.passed), error = error };
            var fullPath = Path.GetFullPath(outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, JsonUtility.ToJson(report, true));
            Debug.Log($"[CT-EVIDENCE] JSON: {fullPath}");
        }
    }
}

