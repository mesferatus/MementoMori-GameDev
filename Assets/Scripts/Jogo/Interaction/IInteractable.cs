namespace MementoMori.Interaction
{
    public interface IInteractable
    {
        string InteractionVerb { get; }
        int InteractionPriority { get; }
        bool CanInteract(InteractionContext context);
        void Interact(InteractionContext context);
    }

    /// <summary>Optional: limits how far (centre to centre, world units) an interactable may be offered,
    /// for small props whose trigger would otherwise light up the prompt from across the room.</summary>
    public interface IInteractionReach
    {
        float MaxReach { get; }
    }

    /// <summary>Optional: lets a prop whose solid body lives in another hierarchy (visual foundation)
    /// tell the detector that this collider is its own surface, not an obstacle in front of it.</summary>
    public interface IInteractionSurface
    {
        bool IsOwnSurface(UnityEngine.Collider2D collider);
    }
}
