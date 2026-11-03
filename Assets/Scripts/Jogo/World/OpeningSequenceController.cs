using System.Collections;
using MementoMori.Core;
using MementoMori.Dialogue;
using UnityEngine;

namespace MementoMori.World
{
    public sealed class OpeningSequenceController : MonoBehaviour
    {
        private IEnumerator Start()
        {
            var state = GameState.Instance;
            if (state != null && state.GetCounter("event.C02") > 0) yield break;
            InputGate.Instance?.Block("OpeningSequence");
            try
            {
                foreach (var id in new[] { "C01", "C02" })
                {
                    if (state != null && state.GetCounter("event." + id) > 0) continue;
                    var manager = DialogueManager.Instance;
                    if (manager == null) yield break;
                    while (manager != null && manager.IsOpen) yield return null;
                    if (manager == null) yield break;
                    var data = V61EventCatalog.CreateDialogue(id);
                    var completed = false;
                    System.Action onComplete = () => completed = true;
                    manager.OnDialogueCompleted += onComplete;
                    try
                    {
                        if (!manager.TryStartDialogue(data)) yield break;
                        while (manager != null && manager.IsOpen) yield return null;
                        if (!completed) yield break;
                        state?.SetCounter("event." + id, 1);
                    }
                    finally
                    {
                        if (manager != null) manager.OnDialogueCompleted -= onComplete;
                        if (data != null) Destroy(data);
                    }
                }
            }
            finally { InputGate.Instance?.Release("OpeningSequence"); }
        }

        private void OnDisable() => InputGate.Instance?.Release("OpeningSequence");
    }
}
