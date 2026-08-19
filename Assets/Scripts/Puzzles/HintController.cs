using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace MementoMori.Puzzles
{
    public sealed class HintController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float firstHintAfterSeconds = 45f;
        [SerializeField, Min(1)] private int secondHintAfterErrors = 2;
        [SerializeField, Min(1)] private int finalHintAfterErrors = 4;
        [SerializeField] private UnityEvent onFirstHint;
        [SerializeField] private UnityEvent onSecondHint;
        [SerializeField] private UnityEvent onFinalHint;
        private bool firstShown;
        private bool secondShown;
        private bool finalShown;

        public void Configure(float firstDelay, int secondErrors, int finalErrors, UnityAction first, UnityAction second, UnityAction final)
        {
            firstHintAfterSeconds = Mathf.Max(0f, firstDelay);
            secondHintAfterErrors = Mathf.Max(1, secondErrors);
            finalHintAfterErrors = Mathf.Max(1, finalErrors);
            onFirstHint = new UnityEvent();
            onSecondHint = new UnityEvent();
            onFinalHint = new UnityEvent();
            if (first != null) onFirstHint.AddListener(first);
            if (second != null) onSecondHint.AddListener(second);
            if (final != null) onFinalHint.AddListener(final);
        }

        public void Begin()
        {
            StopAllCoroutines();
            StartCoroutine(FirstHintRoutine());
        }

        public void RegisterError(int errors)
        {
            if (!secondShown && errors >= secondHintAfterErrors) { secondShown = true; onSecondHint?.Invoke(); }
            if (!finalShown && errors >= finalHintAfterErrors) { finalShown = true; onFinalHint?.Invoke(); }
        }

        private IEnumerator FirstHintRoutine()
        {
            yield return new WaitForSeconds(firstHintAfterSeconds);
            if (!firstShown) { firstShown = true; onFirstHint?.Invoke(); }
        }
    }
}
