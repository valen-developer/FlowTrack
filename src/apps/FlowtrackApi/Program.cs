using FlowTrack.Shared.Infrastructure.Auth;
using FlowTrack.WorkManagement.Shared.Infrastructure;
using Serilog;
using ApplicationBuilder = FlowTrack.Shared.Infrastructure.ApplicationBuilder;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    new DotEnvCharger().Load(["../../../.env"]);

    var app = new ApplicationBuilder("FlowTrackApi", args)
        .AddLogger("./logs/flowtrack-.json")
        .AddAuthentication<CookieAuthenticationHandler>("Cookie")
        .AddContext<IamDbContext>("IAM")
        .AddContext<WorkManagementDbContext>("WORK_MANAGEMENT")
        .DiscoverServices(["FlowTrack*.dll"])
        .ProvideWorkManagement()
        .ProvideIam()
        .Build();

    await app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
