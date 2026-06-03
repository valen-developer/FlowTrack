using FlowTrack.Iam.Application;
using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain.Bus.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.Iam.Infrastructure;

public static class IamServiceCollectionExtensions
{
    public static IServiceCollection ProvideIam(this IServiceCollection services)
    {
        var iamDbContext = new IamDbContextFactory().CreateDbContext([]);
        iamDbContext.Database.Migrate();
        services.AddDbContext<IamDbContext>(options =>
            options.UseNpgsql(iamDbContext.Database.GetConnectionString())
        );

        services.AddScoped<UserDao>();
        services.AddScoped<IUserRepository, EfUserRepository>();

        services.AddScoped<AuthTokenGenerator>();

        services.AddScoped<SigninQryHandler>();

        var queryHandlerInformation =
            services
                .FirstOrDefault(service => service.ServiceType == typeof(QueryHandlerInformation))
                ?.ImplementationInstance as QueryHandlerInformation
            ?? throw new InvalidOperationException("QueryHandlerInformation service not found");

        queryHandlerInformation.Add<SigninQry, SigninQryHandler, SigninSuccess>();

        return services;
    }
}
