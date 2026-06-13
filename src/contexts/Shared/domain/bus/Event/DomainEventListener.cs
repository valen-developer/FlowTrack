namespace FlowTrack.Shared.Domain.Bus.Event;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class DomainEventListenerAttribute : Attribute { }
