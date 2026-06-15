namespace FlowTrack.Shared.Infrastructure.Bus.Event
{
    [Service]
    public class DomainEventDispatcher(
        DomainEventSubscriberInformation subscriberInformation,
        IServiceProvider serviceProvider
    )
    {
        public async Task DispatchAsync(DomainEvent domainEvent)
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

        public async Task DispatchExternal(
            ExternalEventSubscriberInfo info,
            DomainEvent domainEvent
        )
        {
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
    }
}
