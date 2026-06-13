using System.Reflection;

namespace FlowTrack.Shared.Infrastructure.Bus.Event;

public sealed record ExternalEventSubscriberInfo
{
    public string QueueName { get; private set; }
    public string EventCode { get; private set; }
    public Type SubscriberType { get; private set; }
    public MethodInfo HandlerMethod { get; private set; }
    public Type EventType { get; private set; }

    public ExternalEventSubscriberInfo(Type subscriberType, MethodInfo handlerMethod)
    {
        var attribute =
            subscriberType.GetCustomAttribute<DomainEventSubscriberAttribute>()
            ?? throw new InvalidOperationException(
                $"Type {subscriberType.FullName} is marked as a domain event subscriber but does not have the DomainEventSubscriberAttribute."
            );
        var eventType = attribute.EventType;
        EnsureEventType(eventType);

        QueueName = ExtractQueueName(subscriberType);
        EventCode = ExtractEventCode(eventType);

        SubscriberType = subscriberType;
        EventType = eventType;
        HandlerMethod = handlerMethod;
    }

    private string ExtractQueueName(Type subscriberType)
    {
        // get the name of the class and add - before a capital letter (except the first one) and convert to lower case
        var className = subscriberType.Name;
        var queueName = System
            .Text.RegularExpressions.Regex.Replace(className, "(?<!^)([A-Z])", "-$1")
            .ToLower();

        return queueName;
    }

    private string ExtractEventCode(Type eventType)
    {
        var eventCodeProperty = eventType.GetProperty(
            "Code",
            BindingFlags.Public | BindingFlags.Static
        );

        if (eventCodeProperty == null)
        {
            throw new InvalidOperationException(
                $"The event type {eventType.FullName} must have a public static Code property."
            );
        }

        var eventCodeValue = eventCodeProperty.GetValue(null) as string;

        if (string.IsNullOrEmpty(eventCodeValue))
        {
            throw new InvalidOperationException(
                $"The EventCode property of {eventType.FullName} cannot be null or empty."
            );
        }

        return eventCodeValue;
    }

    private void EnsureEventType(Type eventType)
    {
        if (!typeof(DomainEvent).IsAssignableFrom(eventType))
        {
            throw new InvalidOperationException(
                $"The event type {eventType.FullName} must inherit from DomainEvent."
            );
        }
    }
}
