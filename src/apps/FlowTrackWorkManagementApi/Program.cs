using FlowTrack.Shared.Infrastructure;
using FlowTrack.Shared.Infrastructure.Auth;
using FlowTrack.Shared.Infrastructure.DotEnv;
using FlowTrack.WorkManagement;
using FlowTrack.WorkManagement.Shared.Infrastructure;
using Serilog;
using ApplicationBuilder = FlowTrack.Shared.Infrastructure.ApplicationBuilder;

try
{
    new DotEnvCharger().Load(["../../../.env"]);

    var app = new ApplicationBuilder("FlowTrackWorkManagementApi", args)
        .AddLogger(logFilePath: "./logs/flowtrack-work-management-.json")
        .AddAuthentication<CookieAuthenticationHandler>("Cookie")
        .AddContext<WorkManagementDbContext>()
        .AddContext<WorkManagementDbContext>("WORK_MANAGEMENT")
        .DiscoverServices(["FlowTrack*.dll"])
        .ProvideWorkManagement()
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
