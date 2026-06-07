using System.Reflection;
using FlowTrack.Shared.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.Shared.Domain;

public static class ServicesDiscoverCollectionExtensions
{
    public static IServiceCollection DiscoverServices(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        var types = assemblies.SelectMany(a => a.GetTypes());

        DiscoverServices(services, types);
        DiscoverProviders(services, types);
        services.DiscoverCommands(assemblies);
        services.DiscoverQueries(assemblies);
        services.DiscoverDomainEventSubscribers(assemblies);

        return services;
    }

    /// <summary>
    /// Discovers and registers services and providers from assemblies in the application's
    /// base directory whose file names match the specified patterns. Matching assemblies are
    /// loaded and scanned for types decorated with <c>[Service]</c> and <c>[Provider]</c>
    /// attributes, which are then registered in the dependency injection container with the
    /// appropriate lifetimes.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="stringMatch">
    /// One or more assembly search patterns using wildcards (e.g., <c>"Assembly*.dll"</c>).
    /// These patterns are passed directly to <see cref="Directory.GetFiles(string, string)"/>.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> instance.</returns>
    /// <remarks>
    /// <code>
    /// builder.Services.DiscoverServices(["Assembly*.dll"]);
    /// builder.Services.DiscoverServices(["Assembly*.dll", "MyApp.Infrastructure*.dll"]);
    /// </code>
    /// </remarks>
    public static IServiceCollection DiscoverServices(
        this IServiceCollection services,
        params string[] stringMatch
    )
    {
        var assemblies = stringMatch
            .Select(s => Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, s))
            .SelectMany(f => f)
            .Select(Assembly.LoadFrom)
            .Where(a => !a.IsDynamic)
            .ToArray();

        services.DiscoverServices(assemblies);

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
