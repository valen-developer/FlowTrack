namespace FlowTrack.Shared.Domain.Bus.Event;

public abstract record DomainEvent
{
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;

    public abstract bool IsExternal();

    public abstract string GetCode();
}
