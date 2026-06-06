using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Bus.Query;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.Shared.Infrastructure;

public static class SharedServiceCollectionExtensions
{
    public static IServiceCollection ProvideShared(this IServiceCollection services)
    {
        var queryHandlerInformation = new QueryHandlerInformation();
        var commandHandlerInformation = new CommandHandlerInformation();

        services.AddSingleton(queryHandlerInformation);
        services.AddSingleton(commandHandlerInformation);

        services.AddScoped(sp =>
        {
            var scanner = sp.GetRequiredService<DomainEventSubscriberScanner>();
            var dispatcher = new DomainEventDispatcher(scanner, sp);

            var assemblies = AppDomain
                .CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Where(a => a.GetName().Name?.StartsWith("FlowTrack") == true)
                .ToArray();

            dispatcher.RegisterSubscribers(assemblies);

            return dispatcher;
        });

        return services;
    }
}
