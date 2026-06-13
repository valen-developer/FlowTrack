using System.Reflection;

namespace FlowTrack.Shared.Infrastructure.Bus.Event;

public sealed record DomainEventSubscriberInfo(
    Type SubscriberType,
    MethodInfo HandlerMethod,
    Type EventType
);
