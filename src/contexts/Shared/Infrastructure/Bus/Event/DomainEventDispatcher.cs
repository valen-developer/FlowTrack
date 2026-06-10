using System.Reflection;
using FlowTrack.Shared.Domain;

namespace FlowTrack.Shared.Infrastructure;

[Service]
public class DomainEventDispatcher(
    DomainEventSubscriberInformation subscriberInformation,
    ExternalEventSubscriberInformation externalSubscriberInformation,
    IServiceProvider serviceProvider
)
{
    public async Task DispatchAsync(DomainEvent domainEvent)
    {
        if (domainEvent.IsExternal())
        {
            await DispatchExternalEvent(domainEvent);
            return;
        }

        await DispatchDomainEvent(domainEvent);
    }

    private async Task DispatchDomainEvent(DomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();
        var handlers = subscriberInformation.GetSubscribersForEvent(eventType);

        foreach (var handler in handlers)
        {
            var instance =
                serviceProvider.GetService(handler.SubscriberType)
                ?? throw new InvalidOperationException(
                    $"Unable to resolve an instance of {handler.SubscriberType.FullName} for handling domain event {eventType.FullName}."
                );
            var result = handler.HandlerMethod.Invoke(instance, [domainEvent]);

            if (result is Task taskResult)
            {
                await taskResult;
            }
        }
    }

    private async Task DispatchExternalEvent(DomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();
        var handlers = externalSubscriberInformation.GetSubscribersForEvent(eventType);

        foreach (var handler in handlers)
        {
            var instance =
                serviceProvider.GetService(handler.SubscriberType)
                ?? throw new InvalidOperationException(
                    $"Unable to resolve an instance of {handler.SubscriberType.FullName} for handling external event {eventType.FullName}."
                );
            var result = handler.HandlerMethod.Invoke(instance, [domainEvent]);

            if (result is Task taskResult)
            {
                await taskResult;
            }
        }
    }
}
