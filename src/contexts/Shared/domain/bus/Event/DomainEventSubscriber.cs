namespace FlowTrack.Shared.Domain;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DomainEventSubscriberAttribute(Type eventType) : Attribute
{
    public Type EventType { get; } = eventType;
}
