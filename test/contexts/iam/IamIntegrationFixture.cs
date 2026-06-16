using FlowTrack.Shared.Domain.Iam.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace FlowTrack.Iam.Test
{
    public class IamIntegrationFixture : IntegrationTestCase, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder(
            "postgres:18-alpine"
        )
            .WithDatabase("flowtrack-iam")
            .WithUsername("postgres")
            .WithPassword("password")
            .Build();

        private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder(
            "rabbitmq:3.11-management-alpine"
        )
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();

        public IamIntegrationFixture()
            : base(
                env: new Dictionary<string, string>()
                {
                    [IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()] =
                        "activate_token_secret_super_ultra_mega_strong",
                    [IamEnvironmentKeysEnum.IAM_URL_OF_ACTIVATION.ToString()] =
                        "http://localhost:5000/activate",
                    [IamEnvironmentKeysEnum.ACCESS_TOKEN_SECRET.ToString()] =
                        "access_token_secret_super_ultra_mega_strong",
                    [IamEnvironmentKeysEnum.REFRESH_TOKEN_SECRET.ToString()] =
                        "refresh_token_secret_super_ultra_mega_strong",
                }
            )
        {
            serviceCollection.AddHostedService<ExternalEventSubscribeBackground>();
            serviceCollection.AddHostedService<DomainEventSubscribeBackground>();

            serviceCollection.AddKeyedScoped(
                "IAM",
                (sp, _) =>
                {
                    var dbContext = sp.GetRequiredService<IamDbContext>();
                    var transaction = new EfCoreTransaction(dbContext);
                    return new Context(transaction);
                }
            );
        }

        public async Task InitializeAsync()
        {
            await _postgresContainer.StartAsync();

            var connectionString = _postgresContainer.GetConnectionString();

            await WaitForAsync(
                async () =>
                {
                    try
                    {
                        await using var connection = new NpgsqlConnection(connectionString);
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

            serviceCollection.AddDbContext<IamDbContext>(options =>
                options.UseNpgsql(connectionString)
            );

            await _rabbitMqContainer.StartAsync();
            Environment.SetEnvironmentVariable("RABBITMQ_HOST", _rabbitMqContainer.Hostname);
            Environment.SetEnvironmentVariable(
                "RABBITMQ_PORT",
                _rabbitMqContainer.GetMappedPublicPort(5672).ToString()
            );
            Environment.SetEnvironmentVariable("RABBITMQ_USERNAME", "guest");
            Environment.SetEnvironmentVariable("RABBITMQ_PASSWORD", "guest");
            Environment.SetEnvironmentVariable("EXTERNAL_EVENT_EXCHANGE_NAME", "domain_events");

            serviceCollection.AddLogging(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Warning)
            );
            serviceCollection.AddSingleton<JsonToDomainEventMapper>(
                _ => new TestJsonToDomainEventMapper()
            );
            serviceCollection.AddHostedService<ExternalEventSubscribeBackground>();
            serviceCollection.AddHostedService<DomainEventSubscribeBackground>();

            using var provider = serviceCollection.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IamDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            if (_hostedServicesStarted)
            {
                var hostedServices = serviceProvider!.GetServices<IHostedService>().Reverse();
                foreach (var hs in hostedServices)
                    await hs.StopAsync(CancellationToken.None);
            }

            if (serviceProvider is not null)
            {
                serviceScope?.Dispose();
                await serviceProvider.DisposeAsync();
            }

            await _postgresContainer.DisposeAsync();
        }

        public async Task EnsureServicesAsync()
        {
            if (serviceProvider is null)
            {
                serviceProvider = serviceCollection.BuildServiceProvider();
                serviceScope = serviceProvider.CreateScope();
            }

            if (!_hostedServicesStarted)
            {
                _hostedServicesStarted = true;
                var hostedServices = serviceProvider.GetServices<IHostedService>();
                foreach (var hs in hostedServices)
                    await hs.StartAsync(CancellationToken.None);
                await Task.Delay(100);
            }
        }

        private bool _hostedServicesStarted;

        private sealed class TestJsonToDomainEventMapper : JsonToDomainEventMapper
        {
            public override DomainEvent? Map(string json)
            {
                var code = GetCode(json);
                return code == UserCreated.Code ? Serialize<UserCreated>(json) : null;
            }
        }

        public async Task<List<T>> ExecuteQueryAsync<T>(string sqlQuery)
            where T : class
        {
            using var provider = serviceCollection.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IamDbContext>();

            return await dbContext.Set<T>().FromSqlRaw(sqlQuery).AsNoTracking().ToListAsync();
        }
    }
}
