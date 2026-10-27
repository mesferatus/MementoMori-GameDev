namespace MementoMori.Interaction
{
    public interface IInteractable
    {
        string InteractionVerb { get; }
        int InteractionPriority { get; }
        bool CanInteract(InteractionContext context);
        void Interact(InteractionContext context);
    }
}
