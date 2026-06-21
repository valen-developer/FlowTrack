using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace FlowTrackIamApi.Test;

public class FlowTrackIamApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private DateTime mockedDateTime = DateTime.UtcNow;

    private PostgreSqlContainer? _postgresContainer;
    private RabbitMqContainer? _rabbitMqContainer;
    public HttpClient HttpClient { get; private set; } = null!;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();

    public async Task InitializeAsync()
    {
        await RunPostgreSqlContainer();
        await SetConnectionStrings();
        await RunRabbitMqContainer();

        ChargeEnv();

        var httpClientOptions = new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"),
            HandleCookies = false,
        };

        HttpClient = CreateClient(httpClientOptions);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _dateTimeProviderMock.SetupGet(m => m.Now).Returns(mockedDateTime);

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

        if (_rabbitMqContainer is not null)
            await _rabbitMqContainer.DisposeAsync();

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
            .WithDatabase($"flowtrack-iam-api-{Guid.NewGuid():N}")
            .WithUsername(Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres")
            .WithPassword(Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "password")
            .Build();

        await _postgresContainer.StartAsync();
        await WaitForAsync(
            async () =>
            {
                try
                {
                    await using var connection = new NpgsqlConnection(
                        _postgresContainer.GetConnectionString()
                    );
                    await connection.OpenAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            },
            timeoutMs: 5000,
            pollIntervalMs: 250
        );
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

    private async Task RunRabbitMqContainer()
    {
        _rabbitMqContainer = new RabbitMqBuilder("rabbitmq:3.11-management-alpine")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();

        await _rabbitMqContainer.StartAsync();

        Environment.SetEnvironmentVariable("RABBITMQ_HOST", _rabbitMqContainer.Hostname);
        Environment.SetEnvironmentVariable(
            "RABBITMQ_PORT",
            _rabbitMqContainer.GetMappedPublicPort(5672).ToString()
        );
        Environment.SetEnvironmentVariable("RABBITMQ_USERNAME", "guest");
        Environment.SetEnvironmentVariable("RABBITMQ_PASSWORD", "guest");
        Environment.SetEnvironmentVariable("EXTERNAL_EVENT_EXCHANGE_NAME", "domain_events");
    }

    public async Task<List<T>> ExecuteQueryAsync<T>(string sqlQuery)
        where T : class
    {
        var dbContext = Services.GetRequiredService<IamDbContext>();

        return await dbContext.Set<T>().FromSqlRaw(sqlQuery).AsNoTracking().ToListAsync();
    }
}
