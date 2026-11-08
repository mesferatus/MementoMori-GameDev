using UnityEngine;

namespace MementoMori.Interaction
{
    public readonly struct InteractionContext
    {
        public InteractionContext(GameObject interactor) => Interactor = interactor;
        public GameObject Interactor { get; }
    }
}
