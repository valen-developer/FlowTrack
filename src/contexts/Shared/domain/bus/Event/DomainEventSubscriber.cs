namespace FlowTrack.Shared.Domain;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DomainEventSubscriberAttribute : Attribute { }
