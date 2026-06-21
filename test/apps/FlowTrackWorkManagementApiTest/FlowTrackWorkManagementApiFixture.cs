using FlowTrack.WorkManagement.Test;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FlowTrackWorkManagementApiTest;

public class FlowTrackWorkManagementApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly WorkManagementTestContainers _containers = new();

    public HttpClient HttpClient { get; private set; } = null!;
    public WorkManagementTestContainers Containers => _containers;

    public async Task InitializeAsync()
    {
        await _containers.StartAsync();

        // Set env vars so the real Program.cs can connect to the containers
        Environment.SetEnvironmentVariable(
            "WORK_MANAGEMENT_DB_CONNECTION_STRING",
            _containers.PostgresConnectionString
        );
        Environment.SetEnvironmentVariable(
            "WORK_MANAGEMENT_ELASTICSEARCH_URL",
            _containers.ElasticsearchUrl
        );
        Environment.SetEnvironmentVariable("RABBITMQ_HOST", _containers.RabbitMqHost);
        Environment.SetEnvironmentVariable("RABBITMQ_PORT", _containers.RabbitMqPort.ToString());
        Environment.SetEnvironmentVariable("RABBITMQ_USERNAME", "guest");
        Environment.SetEnvironmentVariable("RABBITMQ_PASSWORD", "guest");
        Environment.SetEnvironmentVariable("EXTERNAL_EVENT_EXCHANGE_NAME", "domain_events");

        var httpClientOptions = new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"),
            HandleCookies = true,
        };

        HttpClient = CreateClient(httpClientOptions);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    public override async ValueTask DisposeAsync()
    {
        await _containers.DisposeAsync();
        await base.DisposeAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();
}
