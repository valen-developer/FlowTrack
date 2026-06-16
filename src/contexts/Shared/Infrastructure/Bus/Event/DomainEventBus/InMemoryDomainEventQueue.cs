using FlowTrack.Shared.Infrastructure.Bus.Event.DomainEventBus;

namespace FlowTrack.Shared.Infrastructure.Bus.Event;

[Service(Lifetime.Singleton)]
public class InMemoryDomainEventQueue
{
    private readonly List<DomainEventQueueItem> _events = [];

    public void Enqueue(IEnumerable<DomainEventQueueItem> events)
    {
        _events.AddRange(events);
    }

    public List<DomainEventQueueItem> DequeueAll()
    {
        var events = _events.ToList();
        _events.Clear();
        return events;
    }
}
