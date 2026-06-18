using FlowTrack.Iam;
using FlowTrack.Iam.Shared.Infrastructure;
using FlowTrack.Shared.Infrastructure;
using FlowTrack.Shared.Infrastructure.Auth;
using FlowTrack.Shared.Infrastructure.DotEnv;
using Serilog;
using ApplicationBuilder = FlowTrack.Shared.Infrastructure.ApplicationBuilder;

try
{
    new DotEnvCharger().Load(["../../../.env"]);

    var app = new ApplicationBuilder("FlowTrackIamApi", args)
        .AddLogger(logFilePath: "./logs/flowtrack-iam-.json")
        .AddAuthentication<CookieAuthenticationHandler>("Cookie")
        .AddContext<IamDbContext>()
        .DiscoverServices(["FlowTrack*.dll"])
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
