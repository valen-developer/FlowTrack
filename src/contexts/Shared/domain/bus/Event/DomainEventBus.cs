namespace FlowTrack.Shared.Domain;

public interface IDomainEventBus
{
    Task Publish<T>(T @event)
        where T : DomainEvent;

    Task Publish<T>(IEnumerable<T> events)
        where T : DomainEvent;
}
