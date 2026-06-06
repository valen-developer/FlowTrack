using FlowTrack.Shared.Domain;

namespace FlowTrack.Shared.Infrastructure;

public class InMemoryDomainEventBus(DomainEventDispatcher dispatcher) : IDomainEventBus
{
    public async Task Publish<T>(T @event)
        where T : DomainEvent
    {
        await Publish([@event]);
    }

    public async Task Publish<T>(IEnumerable<T> events)
        where T : DomainEvent
    {
        foreach (var @event in events)
        {
            await dispatcher.DispatchAsync(@event);
        }
    }
}
