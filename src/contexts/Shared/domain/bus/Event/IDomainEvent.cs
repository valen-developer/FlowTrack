namespace FlowTrack.Shared.Domain.Bus.Event;

public interface IDomainEvent
{
    public static abstract bool External { get; }
    public static abstract string Code { get; }
}
