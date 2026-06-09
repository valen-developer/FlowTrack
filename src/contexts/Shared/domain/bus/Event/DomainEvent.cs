namespace FlowTrack.Shared.Domain;

public abstract record DomainEvent(string code)
{
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
    public string Code { get; } = code;

    public abstract bool IsExternal();
}
