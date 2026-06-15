namespace FlowTrack.Shared.Domain.Bus.Event
{
    [Service]
    public sealed class EventBus(IDomainEventBus domainEventBus, IExternalEventBus externalEventBus)
    {
        public async Task Publish<T>(T @event)
            where T : DomainEvent
        {
            await Publish([@event]);
        }

        public async Task Publish<T>(IEnumerable<T> events)
            where T : DomainEvent
        {
            var externalEvents = events.Where(e => e.IsExternal());
            var domainEvents = events.Where(e => !e.IsExternal());

            await domainEventBus.Publish(domainEvents);

            foreach (var externalEvent in externalEvents)
            {
                await externalEventBus.Publish(externalEvent);
            }
        }
    }
}
