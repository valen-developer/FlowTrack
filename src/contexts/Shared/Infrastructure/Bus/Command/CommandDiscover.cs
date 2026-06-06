using System.Reflection;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Bus.Command;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.Shared.Infrastructure;

public static class DicoverCommandsServiceCollectionExtensions
{
    internal static IServiceCollection DiscoverCommands(
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

        services.DiscoverCommands(assemblies);

        return services;
    }

    internal static IServiceCollection DiscoverCommands(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        CommandHandlerInformation commandHandlerInfo = new();

        var commandHandlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t =>
                t.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
                    )
            )
            .ToArray();

        foreach (var handlerType in commandHandlerTypes)
        {
            var commandType = handlerType
                .GetInterfaces()
                .First(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
                )
                .GetGenericArguments()[0];

            commandHandlerInfo.Add(commandType, handlerType);
        }

        services.AddSingleton(commandHandlerInfo);

        return services;
    }
}
