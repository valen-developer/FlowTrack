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

        services.AddSingleton<IBcrypt, Bcrypt>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IJWTService, JWTService>();
        services.AddSingleton<IEnvStore, EnvStore>();
        services.AddSingleton(queryHandlerInformation);
        services.AddSingleton(commandHandlerInformation);
        services.AddScoped<IQueryBus, InMemoryQueryBus>();
        services.AddScoped<ICommandBus, InMemoryCommandBus>();

        services.AddScoped<DomainEventSubscriberScanner, DomainEventSubscriberScanner>();
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
        services.AddScoped<IDomainEventBus, InMemoryDomainEventBus>();

        return services;
    }
}
