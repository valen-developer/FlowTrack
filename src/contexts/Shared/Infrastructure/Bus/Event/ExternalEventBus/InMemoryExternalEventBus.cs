using FlowTrack.Shared.Infrastructure;

namespace FlowTrack.Shared.Domain;

[Provider(typeof(IExternalEventBus))]
public class InMemoryExternalEventBus(DomainEventDispatcher dispatcher) : IExternalEventBus
{
    public async Task Publish<T>(T @event)
        where T : DomainEvent
    {
        await dispatcher.DispatchAsync(@event);
    }
}
