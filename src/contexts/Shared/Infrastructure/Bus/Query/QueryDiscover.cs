using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.Shared.Infrastructure.Bus.Query;

public static class DicoverQueriesServiceCollectionExtensions
{
    internal static IServiceCollection DiscoverQueries(
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

        services.DiscoverQueries(assemblies);

        return services;
    }

    internal static IServiceCollection DiscoverQueries(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        QueryHandlerInformation queryHandlerInfo = new();

        var queryHandlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t =>
                t.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                    )
            )
            .ToArray();

        foreach (var handlerType in queryHandlerTypes)
        {
            var queryType = handlerType
                .GetInterfaces()
                .First(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                )
                .GetGenericArguments()[0];

            queryHandlerInfo.Add(queryType, handlerType);
        }

        services.AddSingleton(queryHandlerInfo);

        return services;
    }
}
