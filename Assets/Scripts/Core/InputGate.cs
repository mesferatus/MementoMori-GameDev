using System.Collections.Generic;
using UnityEngine;

namespace MementoMori.Core
{
    /// <summary>Blocks gameplay input while one or more named systems hold a gate.</summary>
    public sealed class InputGate : MonoBehaviour
    {
        public static InputGate Instance { get; private set; }

        private readonly HashSet<string> activeReasons = new();

        public bool IsBlocked => activeReasons.Count > 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Block(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                Debug.LogWarning("InputGate requires a non-empty reason.", this);
                return;
            }

            activeReasons.Add(reason);
        }

        public void Release(string reason) => activeReasons.Remove(reason);

        public void ClearAll() => activeReasons.Clear();
    }
}
