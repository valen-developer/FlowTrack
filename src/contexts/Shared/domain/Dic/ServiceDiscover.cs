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

        return services;
    }
}
