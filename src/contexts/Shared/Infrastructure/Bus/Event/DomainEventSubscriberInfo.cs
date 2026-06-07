using System.Reflection;

namespace FlowTrack.Shared.Infrastructure;

public sealed record DomainEventSubscriberInfo(
    Type SubscriberType,
    MethodInfo HandlerMethod,
    Type EventType
);
