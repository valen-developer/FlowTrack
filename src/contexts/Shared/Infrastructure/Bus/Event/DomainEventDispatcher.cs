namespace FlowTrack.Shared.Infrastructure.Bus.Event;

[Service]
public class DomainEventDispatcher(
    DomainEventSubscriberInformation subscriberInformation,
    IServiceProvider serviceProvider,
    IDomainLogger logger
)
{
    public async Task DispatchAsync(DomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType().Name;
        var handlers = subscriberInformation.GetSubscribersForEvent(domainEvent.GetType());

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

                LogEventDispatch(domainEvent, handler.SubscriberType, "[Dispatching internal]");
            }
            catch (Exception ex)
            {
                LogError(domainEvent, handler.SubscriberType, ex);
                throw;
            }
        }
    }

    public async Task DispatchExternal(ExternalEventSubscriberInfo info, DomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType().Name;

        try
        {
            LogEventDispatch(domainEvent, info.SubscriberType, "[Dispatching external]");

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
        }
        catch (Exception ex)
        {
            LogError(domainEvent, info.SubscriberType, ex);
            throw;
        }
    }

    private void LogEventDispatch(DomainEvent domainEvent, Type subscriberType, string prefix = "")
    {
        var eventType = domainEvent.GetType().Name;
        logger.Info(
            new LogMessage(
                Action: "Event dispatched",
                Message: $"{prefix} {eventType} dispatched to subscriber {subscriberType.Name}".Trim(),
                Attributes: new
                {
                    EventType = eventType,
                    EventContent = new Dictionary<string, object> { [eventType] = domainEvent },
                    Subscriber = subscriberType.Name,
                }
            )
        );
    }

    private void LogError(DomainEvent domainEvent, Type subscriberType, Exception ex)
    {
        var eventType = domainEvent.GetType().Name;
        logger.Error(
            new LogMessage(
                Action: "Event dispatched",
                Message: $"{eventType} failed in subscriber {subscriberType.Name}",
                Attributes: new { EventType = eventType, Subscriber = subscriberType.Name }
            ),
            ex
        );
    }
}
