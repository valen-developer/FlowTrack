using FlowTrack.Shared.Domain.Bus.Event;

namespace FlowTrack.Shared.Infrastructure.Bus.Event.ExternalEventBus;

public class InMemoryExternalEventBus(DomainEventDispatcher dispatcher) : IExternalEventBus
{
    public async Task Publish<T>(T @event)
        where T : DomainEvent
    {
        await dispatcher.DispatchAsync(@event);
    }
}
