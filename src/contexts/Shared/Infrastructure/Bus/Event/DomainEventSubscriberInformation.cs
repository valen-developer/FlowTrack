namespace FlowTrack.Shared.Infrastructure;

public class DomainEventSubscriberInformation(DomainEventSubscriberInfo[] Subscribers)
{
    public IReadOnlyList<DomainEventSubscriberInfo> Subscribers { get; } = Subscribers;

    public DomainEventSubscriberInfo[] GetSubscribersForEvent(Type eventType)
    {
        return [.. Subscribers.Where(s => s.EventType == eventType)];
    }
}
