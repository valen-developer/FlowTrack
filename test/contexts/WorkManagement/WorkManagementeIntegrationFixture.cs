using DotNet.Testcontainers.Builders;
using FlowTrack.Shared.Domain.Contexts;
using FlowTrack.Shared.Infrastructure.Bus.Event;
using FlowTrack.Shared.Infrastructure.Bus.Event.ExternalEventBus;
using FlowTrack.Shared.Infrastructure.Transactions;
using FlowTrack.Shared.Test;
using FlowTrack.WorkManagement.Shared.Domain;
using FlowTrack.WorkManagement.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Testcontainers.Elasticsearch;
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

        private readonly ElasticsearchContainer _elasticsearchContainer = new ElasticsearchBuilder(
            "elasticsearch:9.4.2"
        )
            .WithEnvironment("discovery.type", "single-node")
            .WithEnvironment("xpack.security.enabled", "false")
            .WithEnvironment("ES_JAVA_OPTS", "-Xms512m -Xmx512m")
            .WithPortBinding(9200, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(9200))
            .Build();

        public WorkManagementIntegrationFixture()
            : base(env: [])
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
            await _elasticsearchContainer.DisposeAsync();
            await _rabbitMqContainer.DisposeAsync();
        }

        public async Task InitializeAsync()
        {
            await _postgresContainer.StartAsync();
            var connectionString = _postgresContainer.GetConnectionString();
            await WaitForPostgresql(connectionString);

            await _elasticsearchContainer.StartAsync();
            await WaitForElasticSearch();
            Environment.SetEnvironmentVariable(
                WorkManagementEnvironmentKeysEnum.WORK_MANAGEMENT_ELASTICSEARCH_URL.ToString(),
                $"http://{_elasticsearchContainer.Hostname}:{_elasticsearchContainer.GetMappedPublicPort(9200)}"
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
            serviceCollection.AddHostedService<DomainEventSubscribeBackground>();

            using var provider = serviceCollection.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WorkManagementDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        private async Task WaitForElasticSearch()
        {
            await WaitForAsync(
                async () =>
                {
                    try
                    {
                        using var client = new HttpClient
                        {
                            BaseAddress = new Uri(
                                $"http://{_elasticsearchContainer.Hostname}:{_elasticsearchContainer.GetMappedPublicPort(9200)}"
                            ),
                        };
                        var response = await client.GetAsync("/_cluster/health");

                        Console.WriteLine(
                            $"Elasticsearch health check response: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}, IsSuccessStatusCode: {response.IsSuccessStatusCode}"
                        );
                        return response.IsSuccessStatusCode;
                    }
                    catch
                    {
                        return false;
                    }
                },
                timeoutMs: 60 * 1000,
                pollIntervalMs: 250
            );
        }

        private static async Task WaitForPostgresql(string connectionString)
        {
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

        public async Task<List<T>> ExecuteQueryOnSearchEngine<T>(string indexName, object query)
        {
            var elasticsearchUri = new Uri(
                $"http://{_elasticsearchContainer.Hostname}:{_elasticsearchContainer.GetMappedPublicPort(9200)}"
            );
            using var client = new HttpClient { BaseAddress = elasticsearchUri };

            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            };

            var jsonContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(query),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync($"/{indexName}/_search", jsonContent);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = System.Text.Json.JsonDocument.Parse(responseContent);
            var hits = responseJson.RootElement.GetProperty("hits").GetProperty("hits");

            var results = new List<T>();
            foreach (var hit in hits.EnumerateArray())
            {
                var source = hit.GetProperty("_source").GetRawText();
                var item = System.Text.Json.JsonSerializer.Deserialize<T>(source, jsonOptions);
                if (item is not null)
                    results.Add(item);
            }

            return results;
        }
    }
}
