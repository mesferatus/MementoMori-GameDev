using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MementoMori.Audio;

namespace MementoMori.Poe
{
    public enum PoeState { Hidden, Reveal, Waiting, Following, Leading, Inspecting, Refusing, Frightened, Mirrored, Dissolving, MovingToEventPoint, Disabled }

    public sealed class PoeFollower : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField, Min(0.1f)] private float speed = 3f;
        [SerializeField, Min(0f)] private float minimumDistance = 1.2f;
        [SerializeField, Min(0.1f)] private float eventTimeout = 5f;
        public PoeState State { get; private set; } = PoeState.Hidden;
        private Coroutine eventRoutine;
        private Animator animator;
        private SpriteRenderer visual;
        private Vector3 lastVisualPosition;
        private string visualDirection = "front";
        private string animationState;
        private Tilemap navigationFloor;
        private readonly Queue<Vector2> navigationRoute = new();
        private Vector2 routeDestination;
        private float nextRouteSearch;

        private void OnEnable()
        {
            animator = GetComponentInChildren<Animator>();
            visual = animator == null ? null : animator.GetComponent<SpriteRenderer>();
            lastVisualPosition = transform.position;
            animationState = null;
        }

        private void LateUpdate()
        {
            var delta = transform.position - lastVisualPosition;
            lastVisualPosition = transform.position;
            if (animator == null || animator.runtimeAnimatorController == null) return;
            var moving = delta.sqrMagnitude > .000001f;
            if (moving)
            {
                visualDirection = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? "side" : delta.y > 0 ? "back" : "front";
                if (visual != null && visualDirection == "side") visual.flipX = delta.x < 0;
            }
            var next = State switch
            {
                PoeState.Dissolving => "poe_dissolve",
                PoeState.Frightened => "poe_scared_arch",
                PoeState.Inspecting => "poe_clue_look",
                PoeState.Refusing => "poe_alert",
                _ => "poe_" + (moving ? "walk_" : "idle_") + visualDirection
            };
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

        private void Update()
        {
            if (State != PoeState.Following || player == null) return;
            var delta = player.position - transform.position;
            if (delta.sqrMagnitude > minimumDistance * minimumDistance)
                MoveOnFloor(player.position);
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
            var steps = Mathf.Max(1, Mathf.CeilToInt(delta.magnitude / .25f));
            for (var i = 0; i <= steps; i++)
                if (navigationFloor != null && !navigationFloor.HasTile(navigationFloor.WorldToCell(Vector2.Lerp(from, to, i / (float)steps)))) return false;
            foreach (var hit in Physics2D.CircleCastAll(from, .18f, delta.normalized, delta.magnitude))
                if (hit.collider != null && !hit.collider.isTrigger && !hit.transform.IsChildOf(transform)
                    && (player == null || !hit.transform.IsChildOf(player))) return false;
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
                transform.position = Vector2.MoveTowards(current, destination, speed * Time.deltaTime);
                return;
            }
            if (Time.time >= nextRouteSearch && (navigationRoute.Count == 0 || Vector2.Distance(destination, routeDestination) > 1f))
            {
                nextRouteSearch = Time.time + .5f;
                routeDestination = destination;
                navigationRoute.Clear();
                var start = Vector2Int.FloorToInt(current);
                var frontier = new Queue<Vector2Int>();
                var previous = new Dictionary<Vector2Int, Vector2Int>();
                frontier.Enqueue(start); previous[start] = start;
                Vector2Int? goal = null;
                var directions = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
                while (frontier.Count > 0 && previous.Count < 16000)
                {
                    var cell = frontier.Dequeue();
                    var point = (Vector2)cell + Vector2.one * .5f;
                    if (Vector2.Distance(point, destination) < 1.5f && ClearStep(point, destination)) { goal = cell; break; }
                    foreach (var direction in directions)
                    {
                        var next = cell + direction;
                        var nextPoint = (Vector2)next + Vector2.one * .5f;
                        if (previous.ContainsKey(next) || !ClearStep(cell == start ? current : point, nextPoint)) continue;
                        previous[next] = cell; frontier.Enqueue(next);
                    }
                }
                if (goal.HasValue)
                {
                    var reverse = new List<Vector2> { destination };
                    for (var cell = goal.Value; cell != start; cell = previous[cell]) reverse.Add((Vector2)cell + Vector2.one * .5f);
                    reverse.Reverse();
                    foreach (var point in reverse) navigationRoute.Enqueue(point);
                }
            }
            while (navigationRoute.Count > 0 && Vector2.Distance(current, navigationRoute.Peek()) < .06f) navigationRoute.Dequeue();
            if (navigationRoute.Count == 0) return;
            var waypoint = navigationRoute.Peek();
            if (!ClearStep(current, waypoint)) { navigationRoute.Clear(); return; }
            transform.position = Vector2.MoveTowards(current, waypoint, speed * Time.deltaTime);
        }

        private void Face(Vector2 direction)
        {
            if (direction.sqrMagnitude < .01f) return;
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer != null && Mathf.Abs(direction.x) > .01f) renderer.flipX = direction.x < 0f;
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
