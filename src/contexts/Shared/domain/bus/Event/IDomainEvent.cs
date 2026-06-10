namespace FlowTrack.Shared.Domain;

public interface IDomainEvent
{
    public static abstract bool External { get; }
    public static abstract string Code { get; }
}
