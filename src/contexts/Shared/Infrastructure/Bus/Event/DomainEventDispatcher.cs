using System.Reflection;
using FlowTrack.Shared.Domain;

namespace FlowTrack.Shared.Infrastructure;

public class DomainEventDispatcher(DomainEventSubscriberScanner scanner)
{
    private readonly List<DomainEventSubscriberInfo> _subscribers = [];

    public void RegisterSubscribers(params Assembly[] assemblies)
    {
        var subscribers = scanner.Scan(assemblies);
        _subscribers.AddRange(subscribers);
    }

    public async Task DispatchAsync(DomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();
        var handlers = _subscribers.Where(s => s.EventType == eventType);

        foreach (var handler in handlers)
        {
            var instance = Activator.CreateInstance(handler.SubscriberType);
            var result = handler.HandlerMethod.Invoke(instance, new object[] { domainEvent });

            if (result is Task taskResult)
            {
                await taskResult;
            }
        }
    }
}
