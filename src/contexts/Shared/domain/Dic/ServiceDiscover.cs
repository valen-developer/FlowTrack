using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.Shared.Domain;

public static class SharedServiceCollectionExtensions
{
    public static IServiceCollection Discover(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        var types = assemblies.SelectMany(a => a.GetTypes());

        DiscoverServices(services, types);
        DiscoverProviders(services, types);

        return services;
    }

    private static void DiscoverProviders(IServiceCollection services, IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            if (!type.IsClass || type.IsAbstract)
            {
                continue;
            }

            var providerAttribute = type.GetCustomAttribute<ProviderAttribute>();
            if (providerAttribute is null)
                continue;

            switch (providerAttribute.Lifetime)
            {
                case Lifetime.Singleton:
                    services.AddSingleton(providerAttribute.ServiceType, type);
                    break;
                case Lifetime.Scoped:
                    services.AddScoped(providerAttribute.ServiceType, type);
                    break;
                case Lifetime.Transient:
                    services.AddTransient(providerAttribute.ServiceType, type);
                    break;
            }
        }
    }

    private static void DiscoverServices(IServiceCollection services, IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            if (!type.IsClass || type.IsAbstract)
            {
                continue;
            }

            var serviceAttribute = type.GetCustomAttribute<ServiceAttribute>();
            if (serviceAttribute is null)
                continue;

            switch (serviceAttribute.Lifetime)
            {
                case Lifetime.Singleton:
                    services.AddSingleton(type);
                    break;
                case Lifetime.Scoped:
                    services.AddScoped(type);
                    break;
                case Lifetime.Transient:
                    services.AddTransient(type);
                    break;
            }
        }
    }
}
