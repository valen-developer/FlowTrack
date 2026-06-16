using FlowTrack.Shared.Infrastructure.Bus.Event;
using FlowTrack.WorkManagement.Shared.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using FlowtrackApi.Shared;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.WithProperty("Application", "FlowTrack")
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(new Serilog.Formatting.Elasticsearch.ElasticsearchJsonFormatter());
    });

    new DotEnvCharger().Load(["../../../.env"]);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllers();

    builder.Services.AddOpenApi();

    builder.Services.ProvideIam();
    builder.Services.ProvideWorkManagement();
    builder.Services.DiscoverServices(["FlowTrack*.dll"]);
    builder.Services.AddKeyedScoped<Context>(
        "IAM",
        (sp, _) =>
        {
            var dbContext = sp.GetRequiredService<IamDbContext>();
            var transaction = new EfCoreTransaction(dbContext);
            return new Context(transaction);
        }
    );

    builder.Services.AddKeyedScoped<Context>(
        "WORK_MANAGEMENT",
        (sp, _) =>
        {
            var dbContext = sp.GetRequiredService<WorkManagementDbContext>();
            var transaction = new EfCoreTransaction(dbContext);
            return new Context(transaction);
        }
    );

    builder
        .Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "IamCookie";
            options.DefaultChallengeScheme = "IamCookie";
        })
        .AddScheme<AuthenticationSchemeOptions, IamAuthenticationHandler>("IamCookie", options => { });

    builder
        .Services.AddAuthorizationBuilder()
        .SetFallbackPolicy(
            new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes("IamCookie")
                .RequireAuthenticatedUser()
                .Build()
        );

    builder.Services.AddHostedService<ExternalEventSubscribeBackground>();
    builder.Services.AddHostedService<DomainEventSubscribeBackground>();

    var app = builder.Build();

    // Middleware de logging — debe ir al principio del pipeline
    app.UseMiddleware<RequestLoggingMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseExceptionHandler(handler =>
    {
        handler.Run(async context =>
        {
            var feature = context.Features.Get<IExceptionHandlerFeature>();
            var exception = feature?.Error;
            if (exception == null)
                return;

            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}",
                context.Request.Method, context.Request.Path);

            if (exception is ActionHandlerExecutionException actionHandlerExecutionException)
            {
                var cause = actionHandlerExecutionException.GetCause();
                if (cause is DomainException ex)
                {
                    var (statusCode, httpErrorException) = DomainToHttpExceptionMapper.Map(ex);
                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsJsonAsync(httpErrorException);
                    return;
                }
            }

            if (exception is DomainException domainException)
            {
                var (statusCode, httpErrorException) = DomainToHttpExceptionMapper.Map(domainException);
                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsJsonAsync(httpErrorException);
                return;
            }

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(
                new HttpErrorResponse("Internal Server Error", "exception.internal.server")
            );
        });
    });

    app.MapControllers();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
