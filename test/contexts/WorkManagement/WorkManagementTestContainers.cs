using System.Text;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using FlowTrack.WorkManagement.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.Elasticsearch;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace FlowTrack.WorkManagement.Test;

public class WorkManagementTestContainers : IAsyncDisposable
{
    private readonly PostgreSqlContainer _postgres;
    private readonly RabbitMqContainer _rabbitMq;
    private readonly ElasticsearchContainer _elasticsearch;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public WorkManagementTestContainers()
    {
        _postgres = new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase("flowtrack-workmanagement")
            .WithUsername("postgres")
            .WithPassword("password")
            .Build();

        _rabbitMq = new RabbitMqBuilder("rabbitmq:3.11-management-alpine")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();

        _elasticsearch = new ElasticsearchBuilder("elasticsearch:9.4.2")
            .WithEnvironment("discovery.type", "single-node")
            .WithEnvironment("xpack.security.enabled", "false")
            .WithEnvironment("ES_JAVA_OPTS", "-Xms512m -Xmx512m")
            .WithPortBinding(9200, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(9200))
            .Build();
    }

    // ── Connection info ──────────────────────────────────────────────

    public string PostgresConnectionString => _postgres.GetConnectionString();

    public string ElasticsearchUrl =>
        $"http://{_elasticsearch.Hostname}:{_elasticsearch.GetMappedPublicPort(9200)}";

    public string RabbitMqHost => _rabbitMq.Hostname;
    public int RabbitMqPort => _rabbitMq.GetMappedPublicPort(5672);

    // ── Lifecycle ────────────────────────────────────────────────────

    public async Task StartAsync()
    {
        await _postgres.StartAsync();
        await WaitForPostgresql(PostgresConnectionString);

        await _elasticsearch.StartAsync();
        await WaitForElasticSearch();

        await _rabbitMq.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _elasticsearch.DisposeAsync();
        await _rabbitMq.DisposeAsync();
    }

    // ── PostgreSQL helpers ───────────────────────────────────────────

    public async Task<List<T>> ExecuteQueryAsync<T>(string sqlQuery)
        where T : class
    {
        var optionsBuilder = new DbContextOptionsBuilder<WorkManagementDbContext>();
        optionsBuilder.UseNpgsql(PostgresConnectionString);
        using var dbContext = new WorkManagementDbContext(optionsBuilder.Options);
        return await dbContext.Set<T>().FromSqlRaw(sqlQuery).AsNoTracking().ToListAsync();
    }

    // ── Elasticsearch helpers ────────────────────────────────────────

    public async Task IndexDocs(string indexName, IEnumerable<object> documents)
    {
        using var client = new HttpClient { BaseAddress = new Uri(ElasticsearchUrl) };

        foreach (var doc in documents)
        {
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(doc, JsonOptions),
                Encoding.UTF8,
                "application/json"
            );
            var response = await client.PostAsync($"/{indexName}/_doc", jsonContent);
            response.EnsureSuccessStatusCode();
        }

        var refreshResponse = await client.PostAsync($"/{indexName}/_refresh", null);
        refreshResponse.EnsureSuccessStatusCode();
    }

    public async Task<List<T>> ExecuteQueryOnSearchEngine<T>(string indexName, object query)
    {
        using var client = new HttpClient { BaseAddress = new Uri(ElasticsearchUrl) };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(query),
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.PostAsync($"/{indexName}/_search", jsonContent);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonDocument.Parse(responseContent);
        var hits = responseJson.RootElement.GetProperty("hits").GetProperty("hits");

        var results = new List<T>();
        foreach (var hit in hits.EnumerateArray())
        {
            var source = hit.GetProperty("_source").GetRawText();
            var item = JsonSerializer.Deserialize<T>(source, JsonOptions);
            if (item is not null)
                results.Add(item);
        }

        return results;
    }

    // ── Wait helpers ─────────────────────────────────────────────────

    private async Task WaitForElasticSearch()
    {
        await WaitForAsync(
            async () =>
            {
                try
                {
                    using var client = new HttpClient { BaseAddress = new Uri(ElasticsearchUrl) };
                    var response = await client.GetAsync("/_cluster/health");
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
}
