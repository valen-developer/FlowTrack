using FlowTrack.Shared.Domain.Contexts;
using FlowTrack.Shared.Infrastructure.Bus.Event.ExternalEventBus;
using FlowTrack.Shared.Infrastructure.Transactions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace FlowTrack.Shared.Infrastructure;

public class ApplicationBuilder
{
    private readonly string _applicationName;
    private WebApplicationBuilder _builder;
    public IServiceCollection Services => _builder.Services;

    private bool _withLogger = false;
    private bool _withAuthentication = false;

    public ApplicationBuilder(string applicationName, string[] args)
    {
        _applicationName = applicationName;
        _builder = WebApplication.CreateBuilder(args);
        ConfigBuilder();
    }

    private void ConfigBuilder()
    {
        _builder.Services.AddHttpContextAccessor();
        _builder.Services.AddControllers();
        _builder.Services.AddOpenApi();

        _builder.Services.AddHostedService<ExternalEventSubscribeBackground>();
        _builder.Services.AddHostedService<DomainEventSubscribeBackground>();
    }

    public ApplicationBuilder AddContext<T>(string? key = null)
        where T : DbContext
    {
        if (key is not null)
        {
            _builder.Services.AddKeyedScoped<Context>(
                key,
                (sp, _) =>
                {
                    var dbContext = sp.GetRequiredService<T>();
                    var transaction = new EfCoreTransaction(dbContext);
                    return new Context(transaction);
                }
            );

            return this;
        }

        _builder.Services.AddScoped<Context>(sp =>
        {
            var dbContext = sp.GetRequiredService<T>();
            var transaction = new EfCoreTransaction(dbContext);
            return new Context(transaction);
        });

        return this;
    }

    public ApplicationBuilder AddAuthentication<T>(string schemeName)
        where T : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        _builder
            .Services.AddAuthentication(schemeName)
            .AddScheme<AuthenticationSchemeOptions, T>(schemeName, options => { });

        _builder
            .Services.AddAuthorizationBuilder()
            .SetFallbackPolicy(
                new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(schemeName)
                    .RequireAuthenticatedUser()
                    .Build()
            );

        _withAuthentication = true;

        return this;
    }

    public ApplicationBuilder AddLogger(string logFilePath)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
        _builder.Host.UseSerilog(
            (context, services, configuration) =>
            {
                var formatter = new Serilog.Formatting.Elasticsearch.ElasticsearchJsonFormatter(
                    inlineFields: true,
                    renderMessageTemplate: false
                );

                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.WithProperty("Application", _applicationName)
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentName()
                    .Enrich.FromLogContext()
                    .WriteTo.Console(formatter);

                if (context.HostingEnvironment.IsDevelopment())
                {
                    configuration.WriteTo.File(
                        path: logFilePath,
                        rollingInterval: RollingInterval.Day,
                        formatter: formatter
                    );
                }
            }
        );

        _withLogger = true;

        return this;
    }

    public Application Build()
    {
        var app = _builder.Build();

        return new Application(
            app,
            new ApplicationOptions(_applicationName, _withLogger, _withAuthentication)
        );
    }
}
