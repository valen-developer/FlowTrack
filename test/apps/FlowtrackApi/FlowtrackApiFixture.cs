using System.Net.Sockets;
using dotenv.net;
using FlowTrack.Iam;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.PostgreSql;

namespace FlowtrackApi;

public class FlowtrackApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    public HttpClient HttpClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        ChargeEnv();
        await RunPostgreSqlContainer();
        await SetConnectionStrings();

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
        if (_postgresContainer is not null)
            await _postgresContainer.DisposeAsync();

        await base.DisposeAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    private async Task RunPostgreSqlContainer()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase("flowtrack-api")
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

    private static void ChargeEnv()
    {
        string? envPath = FindEnvFilePath();

        if (envPath is null)
            return;

        DotEnvOptions options = new(envFilePaths: [envPath]);
        DotEnv.Load(options);
    }

    private static string? FindEnvFilePath()
    {
        foreach (string startPath in GetCandidateStartPaths())
        {
            string? envPath = FindEnvFrom(startPath);

            if (envPath is not null)
                return envPath;
        }

        return null;
    }

    private static IEnumerable<string> GetCandidateStartPaths()
    {
        yield return Directory.GetCurrentDirectory();
        yield return AppContext.BaseDirectory;
    }

    private static string? FindEnvFrom(string startPath)
    {
        DirectoryInfo? currentDirectory = new(startPath);

        while (currentDirectory is not null)
        {
            string envPath = Path.Combine(currentDirectory.FullName, ".env");

            if (File.Exists(envPath))
                return envPath;

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }
}
