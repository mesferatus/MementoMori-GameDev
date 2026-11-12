using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MementoMori.Audio;
using MementoMori.Player;
using MementoMori.Core;

namespace MementoMori.Poe
{
    public enum PoeState { Hidden, Reveal, Waiting, Following, Leading, Inspecting, Refusing, Frightened, Mirrored, Dissolving, MovingToEventPoint, Disabled }

    public sealed class PoeFollower : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField, Min(0.1f)] private float speed = 3f;
        [SerializeField, Min(0f)] private float minimumDistance = 1.2f;
        [SerializeField, Min(0f)] private float personalSpace = .55f;
        [SerializeField, Min(0.1f)] private float eventTimeout = 5f;
        public PoeState State { get; private set; } = PoeState.Hidden;
        private Coroutine eventRoutine;
        private Animator animator;
        private SpriteRenderer visual;
        private Vector3 baseVisualScale;
        private bool visualScaleCaptured;
        private Vector3 lastVisualPosition;
        private string visualDirection = "front";
        private string animationState;
        [SerializeField] private Tilemap navigationFloor;
        private readonly Queue<Vector2> navigationRoute = new();
        private Vector2 routeDestination;
        private float nextRouteSearch;
        private const float RouteCellSize = .125f;
        private static readonly Vector2Int[] RouteDirections = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        private static readonly Vector2[] FloorSamples = { Vector2.zero, Vector2.left * .12f, Vector2.right * .12f, Vector2.up * .12f, Vector2.down * .12f };

        private void OnEnable()
        {
            animator = GetComponentInChildren<Animator>();
            if (animator != null && gameObject.scene.name != "DominioLua")
            {
                var original = Resources.Load<RuntimeAnimatorController>("Costumes/PoeOriginal");
                if (original != null) animator.runtimeAnimatorController = original;
            }
            visual = animator == null ? GetComponentInChildren<SpriteRenderer>() : animator.GetComponent<SpriteRenderer>();
            if (visual != null) visual.enabled = State != PoeState.Hidden && State != PoeState.Disabled;
            if (visual != null && !visualScaleCaptured)
            {
                baseVisualScale = visual.transform.localScale;
                visualScaleCaptured = true;
            }
            lastVisualPosition = transform.position;
            animationState = null;
        }
        private void Start()
        {
            if (player == null) player = Object.FindAnyObjectByType<PlayerController>()?.transform;
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "DominioLua"
                && GameState.Instance != null && GameState.Instance.HasFlag(StoryFlag.PoeRevealed))
            {
                State = PoeState.Following;
                navigationRoute.Clear();
            }
            ResolveSpawnOverlap();
        }

        // Scene loads and save restores can drop the companion on top of Melantha.
        private void ResolveSpawnOverlap()
        {
            if (player == null || State is PoeState.Hidden or PoeState.Disabled) return;
            Vector2 current = transform.position;
            if (Vector2.Distance(current, player.position) >= personalSpace * .9f) return;
            for (var ring = 0; ring < 3; ring++)
            {
                var radius = personalSpace + .25f * ring;
                for (var i = 0; i < 8; i++)
                {
                    var angle = i * Mathf.PI / 4f;
                    var candidate = (Vector2)player.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    if (HasFloorClearance(candidate) && ClearStep(candidate, candidate))
                    {
                        transform.position = candidate;
                        lastVisualPosition = transform.position;
                        return;
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (visual != null) visual.enabled = State != PoeState.Hidden && State != PoeState.Disabled;
            var delta = transform.position - lastVisualPosition;
            lastVisualPosition = transform.position;
            if (animator == null || animator.runtimeAnimatorController == null) return;
            var moving = delta.sqrMagnitude > .000001f;
            if (moving)
            {
                visualDirection = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? "side" : delta.y > 0 ? "back" : "front";
                if (visual != null) visual.flipX = visualDirection == "side" && delta.x < 0;
            }
            var next = State switch
            {
                PoeState.Dissolving => "poe_dissolve",
                PoeState.Frightened => "poe_scared_arch",
                PoeState.Inspecting => "poe_clue_look",
                PoeState.Refusing => "poe_alert",
                PoeState.Mirrored => "poe_clue_look",
                PoeState.Reveal => "poe_meow",
                PoeState.Leading when !moving => "poe_meow",
                PoeState.Waiting => "poe_sit_" + visualDirection,
                _ => "poe_" + (moving ? "walk_" : "sit_") + visualDirection
            };
            if (visual != null) visual.transform.localScale = baseVisualScale * .72f;
            if (next == animationState) return;
            animator.Play(next, 0, 0);
            animationState = next;
        }

        public void Configure(Transform followTarget, float followSpeed, float stopDistance)
        {
            player = followTarget;
            speed = Mathf.Max(0.1f, followSpeed);
            minimumDistance = Mathf.Max(0f, stopDistance);
        }

        public void ConfigureNavigationFloor(Tilemap floor)
        {
            navigationFloor = floor;
            navigationRoute.Clear();
            nextRouteSearch = 0f;
        }

        private void Update()
        {
            if (player == null) player = Object.FindAnyObjectByType<PlayerController>()?.transform;
            if (player == null) return;
            if (State == PoeState.Following)
            {
                var controller = player.GetComponent<PlayerController>();
                var destination = controller != null && controller.IsSwimming
                    ? controller.SafeSavePosition : (Vector2)player.position;
                var delta = destination - (Vector2)transform.position;
                if (delta.sqrMagnitude > minimumDistance * minimumDistance)
                    MoveOnFloor(destination);
            }
            MaintainPersonalSpace();
        }

        // The companion has no solid collider on purpose: a physical body would wedge
        // Melantha in the narrow corridors. Instead he keeps a personal space by locally
        // sidestepping to a legal floor cell, and yields the overlap when boxed in.
        private void MaintainPersonalSpace()
        {
            if (State is not (PoeState.Following or PoeState.Waiting or PoeState.Inspecting
                or PoeState.Refusing or PoeState.Frightened or PoeState.Mirrored)) return;
            Vector2 current = transform.position;
            var away = current - (Vector2)player.position;
            var distance = away.magnitude;
            if (distance >= personalSpace) return;
            var primary = distance > .01f ? away / distance : Vector2.down;
            var step = Mathf.Min(speed * 1.25f * Time.deltaTime, personalSpace - distance + .02f);
            foreach (var escapeAngle in new[] { 0f, 50f, -50f, 90f, -90f })
            {
                var direction = Rotate(primary, escapeAngle);
                var target = current + direction * step;
                if (!ClearStep(current, target)) continue;
                transform.position = target;
                return;
            }
        }

        private static Vector2 Rotate(Vector2 vector, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            var sin = Mathf.Sin(radians);
            var cos = Mathf.Cos(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
        }
        public void Reveal() { gameObject.SetActive(true); RuntimeAudio.PlayOneShot("15_poe_soft_call", .45f); State = PoeState.Reveal; }
        public void BeginFollowing() { State = PoeState.Following; }
        public void SetStoryState(PoeState state) { if (state != PoeState.Disabled) State = state; }
        public void Lead() { State = PoeState.Leading; }
        public void Inspect() { State = PoeState.Inspecting; }
        public void Refuse() { State = PoeState.Refusing; }
        public void Frighten() { State = PoeState.Frightened; }
        public void Mirror() { State = PoeState.Mirrored; }
        public void Dissolve() { State = PoeState.Dissolving; }
        public void ReactToEnvironment(string eventId)
        {
            if (string.IsNullOrEmpty(eventId) || State == PoeState.Disabled) return;
            switch (eventId)
            {
                case "FalseDoor": Refuse(); break;
                case "CrescentPetal": Lead(); break;
                case "ReflectedSky": Inspect(); break;
                case "MirrorAbsent": Refuse(); break;
                case "MirrorDouble": Mirror(); break;
                case "SigilError": Frighten(); break;
                case "ToyEcho": Dissolve(); break;
                default: Inspect(); break;
            }
            if (State != PoeState.Dissolving)
                StartCoroutine(ResumeFollowingAfter(1.5f));
        }
        public void ReactToError(bool sigilError)
        {
            State = sigilError ? PoeState.Frightened : PoeState.Inspecting;
            StartCoroutine(ResumeFollowingAfter(1.25f));
        }
        public void HintAt(Vector3 point)
        {
            if (eventRoutine != null) StopCoroutine(eventRoutine);
            eventRoutine = StartCoroutine(HintRoutine(point));
        }
        public void MoveTo(PoeEventPoint point)
        {
            if (point == null) return;
            if (eventRoutine != null) StopCoroutine(eventRoutine);
            eventRoutine = StartCoroutine(MoveRoutine(point));
        }
        private IEnumerator MoveRoutine(PoeEventPoint point)
        {
            State = PoeState.MovingToEventPoint;
            var elapsed = 0f;
            while (Vector2.Distance(transform.position, point.transform.position) > 0.05f && elapsed < eventTimeout)
            {
                MoveOnFloor(point.transform.position);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (elapsed >= eventTimeout && IsOutsideCamera(point.transform.position)) transform.position = point.transform.position;
            Face(point.LookDirection);
            State = PoeState.Waiting;
            yield return new WaitForSeconds(point.WaitDuration);
            State = point.ResumeFollow ? PoeState.Following : PoeState.Waiting;
            eventRoutine = null;
        }
        private IEnumerator HintRoutine(Vector3 point)
        {
            State = PoeState.Leading;
            yield return MoveToPoint(point, eventTimeout * 1.5f);
            State = PoeState.Inspecting;
            yield return new WaitForSeconds(1.5f);
            State = PoeState.Following;
            eventRoutine = null;
        }
        private IEnumerator ResumeFollowingAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (State != PoeState.Disabled) State = PoeState.Following;
        }
        private IEnumerator MoveToPoint(Vector3 point, float timeout)
        {
            var elapsed = 0f;
            while (Vector2.Distance(transform.position, point) > .05f && elapsed < timeout)
            {
                MoveOnFloor(point);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (elapsed >= timeout && IsOutsideCamera(point)) transform.position = point;
        }
        // Keep the companion on the authored floor and route around solid scenery.
        // Trigger volumes and the followed player do not obstruct the cat.
        private bool ClearStep(Vector2 from, Vector2 to)
        {
            var delta = to - from;
            var steps = Mathf.Max(1, Mathf.CeilToInt(delta.magnitude / .1f));
            for (var i = 0; i <= steps; i++)
                if (!HasFloorClearance(Vector2.Lerp(from, to, i / (float)steps))) return false;
            // A short cast can begin at contact. Also reject an occupied destination explicitly.
            foreach (var obstacle in Physics2D.OverlapCircleAll(to, .18f))
                if (obstacle != null && !obstacle.isTrigger && !obstacle.transform.IsChildOf(transform)
                    && (player == null || !obstacle.transform.IsChildOf(player))) return false;
            foreach (var hit in Physics2D.CircleCastAll(from, .18f, delta.normalized, delta.magnitude))
                if (hit.collider != null && !hit.collider.isTrigger && !hit.transform.IsChildOf(transform)
                    && (player == null || !hit.transform.IsChildOf(player))) return false;
            return true;
        }

        private bool HasFloorClearance(Vector2 point)
        {
            if (GardenWaterZone.Contains(point)) return false;
            if(navigationFloor==null)return true;
            // The companion occupies an area, not a point. Avoid clipping diagonal corners.
            foreach(var offset in FloorSamples)
                if(!navigationFloor.HasTile(navigationFloor.WorldToCell(point+offset)))return false;
            return true;
        }
        private void MoveOnFloor(Vector2 destination)
        {
            if (navigationFloor == null)
            {
                var map = GameObject.Find("V3MapArt/Floor");
                if (map != null) navigationFloor = map.GetComponent<Tilemap>();
            }
            var current = (Vector2)transform.position;
            if (ClearStep(current, destination))
            {
                navigationRoute.Clear();
                var next=Vector2.MoveTowards(current,destination,speed*Time.deltaTime);
                if(HasFloorClearance(next))transform.position=next;
                return;
            }
            if (Time.time >= nextRouteSearch && (navigationRoute.Count == 0 || Vector2.Distance(destination, routeDestination) > 1f))
            {
                nextRouteSearch = Time.time + .5f;
                routeDestination = destination;
                navigationRoute.Clear();
                // Match the fine authored navigation floor: one-unit cells can miss
                // a legitimate narrow route that Melantha can walk beside a facade.
                var start = Vector2Int.FloorToInt(current / RouteCellSize);
                var destinationCell = Vector2Int.FloorToInt(destination / RouteCellSize);
                Vector2 CellPoint(Vector2Int cell) => ((Vector2)cell + Vector2.one * .5f) * RouteCellSize;
                int Heuristic(Vector2Int cell) => Mathf.Abs(cell.x-destinationCell.x)+Mathf.Abs(cell.y-destinationCell.y);
                var frontier = new SortedSet<(int score, int serial, Vector2Int cell)>(
                    Comparer<(int score, int serial, Vector2Int cell)>.Create((a,b)=>
                    {var order=a.score.CompareTo(b.score);return order!=0?order:a.serial.CompareTo(b.serial);}));
                var previous = new Dictionary<Vector2Int, Vector2Int>();
                var costs = new Dictionary<Vector2Int,int>{{start,0}};
                var visited = new HashSet<Vector2Int>();
                int serial=0;
                frontier.Add((Heuristic(start),serial++,start)); previous[start] = start;
                Vector2Int? goal = null;
                while (frontier.Count > 0 && visited.Count < 16000)
                {
                    var entry=frontier.Min;frontier.Remove(entry);
                    var cell = entry.cell;
                    if(!visited.Add(cell))continue;
                    var point = CellPoint(cell);
                    if (Vector2.Distance(point, destination) < 1.5f && ClearStep(point, destination)) { goal = cell; break; }
                    foreach (var direction in RouteDirections)
                    {
                        var next = cell + direction;
                        var nextCost=costs[cell]+1;
                        if (visited.Contains(next) || costs.TryGetValue(next,out var knownCost)&&knownCost<=nextCost)continue;
                        var nextPoint = CellPoint(next);
                        if (!ClearStep(cell == start ? current : point, nextPoint)) continue;
                        costs[next]=nextCost;previous[next] = cell;
                        frontier.Add((nextCost+Heuristic(next),serial++,next));
                    }
                }
                if (goal.HasValue)
                {
                    var reverse = new List<Vector2> { destination };
                    for (var cell = goal.Value; cell != start; cell = previous[cell]) reverse.Add(CellPoint(cell));
                    reverse.Reverse();
                    foreach (var point in reverse) navigationRoute.Enqueue(point);
                }
            }
            while (navigationRoute.Count > 0 && Vector2.Distance(current, navigationRoute.Peek()) < .06f) navigationRoute.Dequeue();
            if (navigationRoute.Count == 0) return;
            var waypoint = navigationRoute.Peek();
            if (!ClearStep(current, waypoint)) { navigationRoute.Clear(); return; }
            var nextWaypoint=Vector2.MoveTowards(current,waypoint,speed*Time.deltaTime);
            if(HasFloorClearance(nextWaypoint))transform.position=nextWaypoint;
        }

        private void Face(Vector2 direction)
        {
            if (direction.sqrMagnitude < .01f) return;
            visualDirection = Mathf.Abs(direction.x) > Mathf.Abs(direction.y) ? "side" : direction.y > 0 ? "back" : "front";
            if (visual != null) visual.flipX = visualDirection == "side" && direction.x < 0f;
        }
        private static bool IsOutsideCamera(Vector3 position)
        {
            if (Camera.main == null) return true;
            var viewport = Camera.main.WorldToViewportPoint(position);
            return viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f || viewport.z <= 0f;
        }
        public void DisablePoe() { if (eventRoutine != null) StopCoroutine(eventRoutine); State = PoeState.Disabled; }

        public IEnumerator NarrativeVanishAndReappear(Transform reappearPoint, float vanishDuration = .8f)
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            var colliders = GetComponentsInChildren<Collider2D>(true);
            var previousState = State;
            State = PoeState.Dissolving;
            RuntimeAudio.PlayOneShot("13_fragment_collect", .45f);
            // Let the six official dissolve frames finish before hiding the renderer.
            if (animator != null && animator.runtimeAnimatorController != null)
                yield return new WaitForSeconds(1f);
            foreach (var renderer in renderers) renderer.enabled = false;
            foreach (var collider in colliders) collider.enabled = false;
            yield return new WaitForSeconds(vanishDuration);
            if (reappearPoint != null) transform.position = reappearPoint.position;
            foreach (var renderer in renderers) renderer.enabled = true;
            foreach (var collider in colliders) collider.enabled = true;
            State = previousState == PoeState.Disabled ? PoeState.Waiting : previousState;
        }
    }
}

