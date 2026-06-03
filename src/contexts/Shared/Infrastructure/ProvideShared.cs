using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Bus.Query;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.Shared.Infrastructure;

public static class SharedServiceCollectionExtensions
{
    public static IServiceCollection ProvideShared(this IServiceCollection services)
    {
        var queryHandlerInformation = new QueryHandlerInformation();

        services.AddSingleton<IBcrypt, Bcrypt>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IJWTService, JWTService>();
        services.AddSingleton<IEnvStore, EnvStore>();
        services.AddSingleton(queryHandlerInformation);
        services.AddScoped<IQueryBus, InMemoryQueryBus>();

        return services;
    }
}
