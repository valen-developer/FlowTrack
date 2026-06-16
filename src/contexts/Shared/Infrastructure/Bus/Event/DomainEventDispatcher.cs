using Microsoft.Extensions.Logging;

namespace FlowTrack.Shared.Infrastructure.Bus.Event
{
    [Service]
    public class DomainEventDispatcher(
        DomainEventSubscriberInformation subscriberInformation,
        IServiceProvider serviceProvider,
        ILogger<DomainEventDispatcher> logger
    )
    {
        public async Task DispatchAsync(DomainEvent domainEvent)
        {
            var eventType = domainEvent.GetType().Name;
            var handlers = subscriberInformation.GetSubscribersForEvent(domainEvent.GetType());

            if (handlers.Length == 0)
            {
                logger.LogDebug("No subscribers for domain event {EventType}", eventType);
                return;
            }

            logger.LogDebug(
                "Dispatching domain event {EventType} to {SubscriberCount} subscriber(s)",
                eventType,
                handlers.Length
            );

            foreach (var handler in handlers)
            {
                try
                {
                    var instance =
                        serviceProvider.GetService(handler.SubscriberType)
                        ?? throw new InvalidOperationException(
                            $"Unable to resolve an instance of {handler.SubscriberType.FullName} for handling domain event {eventType}."
                        );
                    var result = handler.HandlerMethod.Invoke(instance, [domainEvent]);

                    if (result is Task taskResult)
                    {
                        await taskResult;
                    }

                    logger.LogDebug(
                        "Domain event {EventType} handled by {Subscriber}",
                        eventType,
                        handler.SubscriberType.Name
                    );
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Domain event {EventType} failed in subscriber {Subscriber}",
                        eventType,
                        handler.SubscriberType.Name
                    );
                }
            }
        }

        public async Task DispatchExternal(
            ExternalEventSubscriberInfo info,
            DomainEvent domainEvent
        )
        {
            var eventType = domainEvent.GetType().Name;

            try
            {
                logger.LogDebug(
                    "Dispatching external event {EventType} to {Subscriber}",
                    eventType,
                    info.SubscriberType.Name
                );

                var instance =
                    serviceProvider.GetService(info.SubscriberType)
                    ?? throw new InvalidOperationException(
                        $"Unable to resolve an instance of {info.SubscriberType.FullName} for handling external event."
                    );
                var result = info.HandlerMethod.Invoke(instance, [domainEvent]);

                if (result is Task taskResult)
                {
                    await taskResult;
                }

                logger.LogInformation(
                    "External event {EventType} handled by {Subscriber}",
                    eventType,
                    info.SubscriberType.Name
                );
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "External event {EventType} failed in subscriber {Subscriber}",
                    eventType,
                    info.SubscriberType.Name
                );
                throw;
            }
        }
    }
}
