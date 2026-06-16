namespace FlowTrack.Shared.Infrastructure.Bus.Event
{
    [Provider(typeof(IDomainEventBus))]
    public class InMemoryDomainEventBus(InMemoryDomainEventQueue queue) : IDomainEventBus
    {
        public async Task Publish<T>(T @event)
            where T : DomainEvent
        {
            await Publish([@event]);
        }

        public async Task Publish<T>(IEnumerable<T> events)
            where T : DomainEvent
        {
            queue.Enqueue(events);
        }
    }
}
