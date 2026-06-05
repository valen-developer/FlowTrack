namespace FlowTrack.Shared.Domain;

public interface IDomainEventBus
{
    void Publish<T>(T @event)
        where T : DomainEvent;

    void Publish<T>(IEnumerable<T> @events)
        where T : DomainEvent;
}
