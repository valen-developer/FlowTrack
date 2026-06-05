namespace FlowTrack.Shared.Domain;

public abstract record DomainEvent(string code)
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
    public string Code { get; } = code;
}
