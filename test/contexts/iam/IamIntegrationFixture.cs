using System.Net.Sockets;
using FlowTrack.Iam.Domain;
using FlowTrack.Iam.Infrastructure;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Infrastructure;
using FlowTrack.Shared.Test;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace FlowTrack.Iam.Test;

public class IamIntegrationFixture : IntegrationTestCase, IAsyncLifetime
{
    private static readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder(
        "postgres:18-alpine"
    )
        .WithDatabase("flowtrack-iam")
        .WithUsername("postgres")
        .WithPassword("password")
        .Build();

    public IamIntegrationFixture()
        : base()
    {
        AddScoped<IJWTService, JWTService>();
        AddScoped<IEnvStore, EnvStore>();
        AddScoped<IBcrypt, Bcrypt>();
        AddScoped<IUserRepository, EfUserRepository>();
        AddScoped<UserDao, UserDao>();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var connectionString = _postgresContainer.GetConnectionString();

        await WaitUntilPostgresIsReady(connectionString);

        serviceCollection.AddDbContext<IamDbContext>(options =>
            options.UseNpgsql(connectionString)
        );

        using var provider = serviceCollection.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IamDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
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

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}
