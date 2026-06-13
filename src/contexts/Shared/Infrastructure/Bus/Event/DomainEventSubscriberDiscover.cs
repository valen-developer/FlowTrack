using System.Reflection;
using FlowTrack.Shared.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.Shared.Infrastructure.Bus.Event;

public static class DomainEventSubscriberDiscoverServiceCollectionExtensions
{
    public static IServiceCollection DiscoverDomainEventSubscribers(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        var subscriptions = new List<DomainEventSubscriberInfo>();
        var externalSubscriptions = new List<ExternalEventSubscriberInfo>();

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
                var subscriberAttribute =
                    subscriberType.GetCustomAttribute<DomainEventSubscriberAttribute>()
                    ?? throw new InvalidOperationException(
                        $"Type {subscriberType.FullName} is marked as a domain event subscriber but does not have the DomainEventSubscriberAttribute."
                    );

                var handlerMethods = subscriberType
                    .GetMethods(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    )
                    .Where(m =>
                        m.GetParameters().Length == 1
                        && typeof(DomainEvent).IsAssignableFrom(m.GetParameters()[0].ParameterType)
                        && subscriberAttribute.EventType.IsAssignableFrom(
                            m.GetParameters()[0].ParameterType
                        )
                        && m.IsDefined(typeof(DomainEventListenerAttribute), inherit: false)
                        && m.IsStatic == false
                        && m.ContainsGenericParameters == false
                        && (m.ReturnType == typeof(void) || m.ReturnType == typeof(Task))
                    );

                var firstHandlerMethod = handlerMethods.FirstOrDefault();
                if (firstHandlerMethod == null)
                {
                    throw new InvalidOperationException(
                        $"Type {subscriberType.FullName} is marked as a domain event subscriber but does not have any valid handler methods."
                    );
                }

                var eventType = firstHandlerMethod.GetParameters()[0].ParameterType;
                var isExternal =
                    eventType.GetProperty("External")?.GetValue(null) as bool? ?? false;

                if (isExternal)
                {
                    externalSubscriptions.Add(
                        new ExternalEventSubscriberInfo(subscriberType, firstHandlerMethod)
                    );

                    continue;
                }

                subscriptions.Add(
                    new DomainEventSubscriberInfo(subscriberType, firstHandlerMethod, eventType)
                );
            }
        }

        var domainEventSubscriberInformation = new DomainEventSubscriberInformation([
            .. subscriptions,
        ]);
        var externalEventSubscriberInformation = new ExternalEventSubscriberInformation([
            .. externalSubscriptions,
        ]);
        services.AddSingleton(domainEventSubscriberInformation);
        services.AddSingleton(externalEventSubscriberInformation);

        return services;
    }
}
