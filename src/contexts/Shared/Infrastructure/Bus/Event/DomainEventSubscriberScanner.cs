using System.Reflection;
using FlowTrack.Shared.Domain;

namespace FlowTrack.Shared.Infrastructure;

public sealed record DomainEventSubscriberInfo(
    Type SubscriberType,
    MethodInfo HandlerMethod,
    Type EventType
);

public class DomainEventSubscriberScanner
{
    public IReadOnlyList<DomainEventSubscriberInfo> Scan(params Assembly[] assemblies)
    {
        var subscriptions = new List<DomainEventSubscriberInfo>();

        foreach (var assembly in assemblies)
        {
            var subscriberTypes = assembly
                .GetTypes()
                .Where(t =>
                    t.IsClass
                    && !t.IsAbstract
                    && t.GetCustomAttribute<DomainEventSubscriberAttribute>() != null
                );

            foreach (var subscriberType in subscriberTypes)
            {
                var handlerMethods = subscriberType
                    .GetMethods(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    )
                    .Where(m =>
                        m.GetParameters().Length == 1
                        && typeof(DomainEvent).IsAssignableFrom(m.GetParameters()[0].ParameterType)
                        && m.IsDefined(typeof(DomainEventListenerAttribute), inherit: false)
                        && m.IsStatic == false
                        && m.ContainsGenericParameters == false
                        && (m.ReturnType == typeof(void) || m.ReturnType == typeof(Task))
                    );

                foreach (var handlerMethod in handlerMethods)
                {
                    var eventType = handlerMethod.GetParameters()[0].ParameterType;
                    subscriptions.Add(
                        new DomainEventSubscriberInfo(subscriberType, handlerMethod, eventType)
                    );
                }
            }
        }

        return subscriptions;
    }
}
