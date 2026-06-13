using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace FlowTrack.Iam.Test;

public class IamIntegrationFixture : IntegrationTestCase, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder(
        "postgres:18-alpine"
    )
        .WithDatabase("flowtrack-iam")
        .WithUsername("postgres")
        .WithPassword("password")
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
        AddScoped<IJWTService, JWTService>();
        AddScoped<IEnvStore, EnvStore>();
        AddScoped<IBcrypt, Bcrypt>();
        AddScoped<IUserRepository, EfUserRepository>();
        AddScoped<UserDao, UserDao>();

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

    public async Task<List<T>> ExecuteQueryAsync<T>(string sqlQuery)
        where T : class
    {
        using var provider = serviceCollection.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IamDbContext>();

        return await dbContext.Set<T>().FromSqlRaw(sqlQuery).AsNoTracking().ToListAsync();
    }
}
