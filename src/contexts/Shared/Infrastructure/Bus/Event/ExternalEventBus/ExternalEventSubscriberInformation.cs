namespace FlowTrack.Shared.Infrastructure.Bus.Event
{
    public sealed class ExternalEventSubscriberInformation(
        ExternalEventSubscriberInfo[] Subscribers
    )
    {
        public IReadOnlyList<ExternalEventSubscriberInfo> Subscribers { get; } = Subscribers;

        public ExternalEventSubscriberInfo[] GetSubscribersForEvent(Type eventType)
        {
            return [.. Subscribers.Where(s => s.EventType == eventType)];
        }
    }
}
