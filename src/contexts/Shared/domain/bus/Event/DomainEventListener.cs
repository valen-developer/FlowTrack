namespace FlowTrack.Shared.Domain;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class DomainEventListenerAttribute : Attribute { }
