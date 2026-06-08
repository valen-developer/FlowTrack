using System.Net.Sockets;
using FlowTrack.Iam;
using FlowTrack.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Npgsql;
using Testcontainers.PostgreSql;

namespace FlowtrackApi.Test;

public class FlowtrackApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    public HttpClient HttpClient { get; private set; } = null!;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();

    public async Task InitializeAsync()
    {
        await RunPostgreSqlContainer();
        await SetConnectionStrings();
        ChargeEnv();

        var httpClientOptions = new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"),
            HandleCookies = true,
        };

        HttpClient = CreateClient(httpClientOptions);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _dateTimeProviderMock.SetupGet(m => m.Now).Returns(DateTime.UtcNow);

        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(_dateTimeProviderMock.Object);
        });
    }

    public override async ValueTask DisposeAsync()
    {
        if (_postgresContainer is not null)
            await _postgresContainer.DisposeAsync();

        await base.DisposeAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private static void ChargeEnv()
    {
        var env = new Dictionary<string, string>()
        {
            [IamEnvironmentKeysEnum.IAM_URL_OF_ACTIVATION.ToString()] = "http://localhost/activate",
            [IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()] =
                "access_token_secret_super_ultra_mega_strong",
            [IamEnvironmentKeysEnum.ACCESS_TOKEN_SECRET.ToString()] =
                "access_token_secret_super_ultra_mega_strong",
            [IamEnvironmentKeysEnum.REFRESH_TOKEN_SECRET.ToString()] =
                "access_token_secret_super_ultra_mega_strong",
        };

        foreach (var item in env)
        {
            Environment.SetEnvironmentVariable(item.Key, item.Value);
        }
    }

    private async Task RunPostgreSqlContainer()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase($"flowtrack-api-{Guid.NewGuid():N}")
            .WithUsername(Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres")
            .WithPassword(Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "password")
            .Build();

        await _postgresContainer.StartAsync();
        await WaitUntilPostgresIsReady(_postgresContainer.GetConnectionString());
    }

    private Task SetConnectionStrings()
    {
        if (_postgresContainer is null)
            throw new InvalidOperationException("PostgreSQL container is not initialized");

        var connectionString = _postgresContainer.GetConnectionString();
        Environment.SetEnvironmentVariable(
            IamEnvironmentKeysEnum.IAM_DB_CONNECTION_STRING.ToString(),
            connectionString
        );

        return Task.CompletedTask;
    }

    private static async Task WaitUntilPostgresIsReady(string connectionString)
    {
        const int maxAttempts = 20;
        const int delayMs = 250;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                return;
            }
            catch (NpgsqlException) when (attempt < maxAttempts)
            {
                await Task.Delay(delayMs);
            }
            catch (SocketException) when (attempt < maxAttempts)
            {
                await Task.Delay(delayMs);
            }
        }

        throw new InvalidOperationException(
            $"PostgreSQL container was not ready after {maxAttempts * delayMs} ms"
        );
    }
}
