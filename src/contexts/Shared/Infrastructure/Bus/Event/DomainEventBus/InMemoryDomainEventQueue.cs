namespace FlowTrack.Shared.Infrastructure.Bus.Event;

[Service(Lifetime.Singleton)]
public class InMemoryDomainEventQueue
{
    private readonly List<DomainEvent> _events = [];

    public void Enqueue(IEnumerable<DomainEvent> events)
    {
        _events.AddRange(events);
    }

    public List<DomainEvent> DequeueAll()
    {
        var events = _events.ToList();
        _events.Clear();
        return events;
    }
}
