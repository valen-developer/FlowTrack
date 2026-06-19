var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .ConfigureHttpClient(
        (context, handler) =>
        {
            handler.PooledConnectionLifetime = TimeSpan.FromSeconds(5);
            handler.PooledConnectionIdleTimeout = TimeSpan.FromSeconds(3);
        }
    );

var app = builder.Build();

app.MapReverseProxy();

app.Run();
