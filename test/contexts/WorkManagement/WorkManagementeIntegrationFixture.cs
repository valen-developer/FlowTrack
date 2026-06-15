using FlowTrack.Shared.Domain.Contexts;
using FlowTrack.Shared.Infrastructure.Bus.Event.ExternalEventBus;
using FlowTrack.Shared.Infrastructure.Transactions;
using FlowTrack.Shared.Test;
using FlowTrack.WorkManagement.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace FlowTrack.WorkManagement.Test
{
    public class WorkManagementIntegrationFixture : IntegrationTestCase, IAsyncLifetime
    {
        private bool _hostedServicesStarted;

        private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder(
            "postgres:18-alpine"
        )
            .WithDatabase("flowtrack-workmanagement")
            .WithUsername("postgres")
            .WithPassword("password")
            .Build();

        private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder(
            "rabbitmq:3.11-management-alpine"
        )
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();

        public WorkManagementIntegrationFixture()
            : base(env: new([]))
        {
            serviceCollection.AddKeyedScoped(
                "WORK_MANAGEMENT",
                (sp, _) =>
                {
                    var dbContext = sp.GetRequiredService<WorkManagementDbContext>();
                    var transaction = new EfCoreTransaction(dbContext);
                    return new Context(transaction);
                }
            );
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

            serviceCollection.AddDbContext<WorkManagementDbContext>(options =>
                options.UseNpgsql(connectionString)
            );

            serviceCollection.AddLogging(builder => builder.AddConsole());

            await _rabbitMqContainer.StartAsync();
            Environment.SetEnvironmentVariable("RABBITMQ_HOST", _rabbitMqContainer.Hostname);
            Environment.SetEnvironmentVariable(
                "RABBITMQ_PORT",
                _rabbitMqContainer.GetMappedPublicPort(5672).ToString()
            );
            Environment.SetEnvironmentVariable("RABBITMQ_USERNAME", "guest");
            Environment.SetEnvironmentVariable("RABBITMQ_PASSWORD", "guest");
            Environment.SetEnvironmentVariable("EXTERNAL_EVENT_EXCHANGE_NAME", "domain_events");

            serviceCollection.AddHostedService<ExternalEventSubscribeBackground>();

            using var provider = serviceCollection.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WorkManagementDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
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

        public async Task<List<T>> ExecuteQueryAsync<T>(string sqlQuery)
            where T : class
        {
            using var provider = serviceCollection.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WorkManagementDbContext>();

            return await dbContext.Set<T>().FromSqlRaw(sqlQuery).AsNoTracking().ToListAsync();
        }
    }
}
