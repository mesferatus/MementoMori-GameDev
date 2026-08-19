using System.Collections;
using UnityEngine;
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
                transform.position += delta.normalized * speed * Time.deltaTime;
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
                transform.position = Vector2.MoveTowards(transform.position, point.transform.position, speed * Time.deltaTime);
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
                transform.position = Vector2.MoveTowards(transform.position, point, speed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (elapsed >= timeout && IsOutsideCamera(point)) transform.position = point;
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
    }
}
