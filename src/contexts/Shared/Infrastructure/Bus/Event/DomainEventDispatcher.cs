namespace FlowTrack.Shared.Infrastructure.Bus.Event
{
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

            if (handlers.Length == 0)
            {
                logger.Info(
                    new LogMessage(
                        Action: "Event dispatched",
                        Message: $"{eventType} has no subscribers"
                    )
                );
                return;
            }

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

                    logger.Info(
                        new LogMessage(
                            Action: "Event dispatched",
                            Message: $"{eventType} dispatched to subscriber {handler.SubscriberType.Name}",
                            Attributes: new
                            {
                                EventType = eventType,
                                EventContent = domainEvent,
                                Subscriber = handler.SubscriberType.Name,
                            }
                        )
                    );
                }
                catch (Exception ex)
                {
                    logger.Error(
                        new LogMessage(
                            Action: "Event dispatched",
                            Message: $"{eventType} failed in subscriber {handler.SubscriberType.Name}",
                            Attributes: new
                            {
                                EventType = eventType,
                                Subscriber = handler.SubscriberType.Name,
                            }
                        ),
                        ex
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
                logger.Info(
                    new LogMessage(
                        Action: "Event consumed",
                        Message: $"Dispatching external {eventType} to {info.SubscriberType.Name}",
                        Attributes: new
                        {
                            EventType = eventType,
                            EventContent = domainEvent,
                            Subscriber = info.SubscriberType.Name,
                        }
                    )
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
            }
            catch (Exception ex)
            {
                logger.Error(
                    new LogMessage(
                        Action: "Event consumed",
                        Message: $"External {eventType} failed in subscriber {info.SubscriberType.Name}",
                        Attributes: new
                        {
                            EventType = eventType,
                            Subscriber = info.SubscriberType.Name,
                        }
                    ),
                    ex
                );
                throw;
            }
        }
    }
}
