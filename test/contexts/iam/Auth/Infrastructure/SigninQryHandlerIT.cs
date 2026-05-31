using System.Net.Sockets;
using FlowTrack.Iam;
using FlowTrack.Iam.Application;
using FlowTrack.Iam.Domain;
using FlowTrack.Iam.Infrastructure;
using FlowTrack.Iam.Test;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Infrastructure;
using FlowTrack.Shared.Test;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace FlowTrack.Test.Iam.Infrastructure;

public class SigninQryHandlerIT : IntegrationTestCase, IAsyncLifetime
{
    private static readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder(
        "postgres:18-alpine"
    )
        .WithDatabase("flowtrack-iam")
        .WithUsername("postgres")
        .WithPassword("password")
        .Build();

    public SigninQryHandlerIT()
        : base()
    {
        AddScoped<UserDao, UserDao>();
        AddScoped<IJWTService, JWTService>();
        AddScoped<IEnvStore, EnvStore>();
        AddScoped<SigninQryHandler, SigninQryHandler>();
        AddScoped<IUserRepository, EfUserRepository>();
        AddScoped<AuthTokenGenerator, AuthTokenGenerator>();
        AddScoped<IBcrypt, Bcrypt>();
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

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
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

    [Fact]
    public async Task Should_Signin_User()
    {
        var user = UserMother.Random();

        var userDao = GetService<UserDao>();
        var userEntity = UserEntity.FromDomain(user);
        userEntity.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        await userDao.Insert(userEntity);

        var jwtService = GetService<IJWTService>();
        var envStore = GetService<IEnvStore>();

        var accessTokenSecret =
            envStore.Get(IamEnvironmentKeysEnum.ACCESS_TOKEN_SECRET.ToString())
            ?? throw new Exception($"{IamEnvironmentKeysEnum.ACCESS_TOKEN_SECRET} is not set");
        var accessTokenExpirationMinutes = 60;

        var refreshTokenSecret =
            envStore.Get(IamEnvironmentKeysEnum.REFRESH_TOKEN_SECRET.ToString())
            ?? throw new Exception("REFRESH_TOKEN_SECRET is not set");
        var refreshTokenExpirationMinutes = 60 * 24 * 30;

        var payload = new JWTPayload(
            new Dictionary<string, string> { ["id"] = user.Id.ToString() }
        );

        var accessTokenOptions = new JWTOptions(accessTokenSecret, accessTokenExpirationMinutes);
        var refreshTokenOptions = new JWTOptions(refreshTokenSecret, refreshTokenExpirationMinutes);

        var expectedAccessToken = jwtService.Generate(payload, accessTokenOptions);
        var expectedRefreshToken = jwtService.Generate(payload, refreshTokenOptions);
        var expectedResult = new SigninSuccess(expectedAccessToken, expectedRefreshToken);

        var handler = GetService<SigninQryHandler>();
        var qry = new SigninQry(user.Email, user.Password);

        var result = await handler.Handle(qry);

        Assert.Equivalent(expectedResult, result);
    }
}
