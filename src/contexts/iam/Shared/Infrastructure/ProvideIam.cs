using FlowTrack.Iam.Application;
using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain;
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

        services.AddScoped<IUserRepository, EfUserRepository>();

        AddQueries(services);
        AddCommands(services);

        return services;
    }

    private static void AddQueries(IServiceCollection services)
    {
        var queryHandlerInformation =
            services
                .FirstOrDefault(service => service.ServiceType == typeof(QueryHandlerInformation))
                ?.ImplementationInstance as QueryHandlerInformation
            ?? throw new InvalidOperationException("QueryHandlerInformation service not found");

        queryHandlerInformation.Add<SigninQry, SigninQryHandler, SigninSuccess>();
        queryHandlerInformation.Add<FindUserByIdQry, FindUserByIdQryHandler, User>();
    }

    private static void AddCommands(IServiceCollection services)
    {
        // services.AddScoped<SignupCmdHandler>();

        var commandHandlerInformation =
            services
                .FirstOrDefault(service => service.ServiceType == typeof(CommandHandlerInformation))
                ?.ImplementationInstance as CommandHandlerInformation
            ?? throw new InvalidOperationException("CommandHandlerInformation service not found");

        commandHandlerInformation.Add<SignupCmd, SignupCmdHandler>();
    }
}
