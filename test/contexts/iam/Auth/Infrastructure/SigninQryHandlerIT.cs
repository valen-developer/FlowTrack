using FlowTrack.Iam.Application;
using FlowTrack.Iam.Domain;
using FlowTrack.Iam.Infrastructure;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Infrastructure;

namespace FlowTrack.Iam.Test.Infrastructure;

public class SigninQryHandlerIT : IamIntegrationFixture
{
    public SigninQryHandlerIT()
        : base()
    {
        AddScoped<IJWTService, JWTService>();
        AddScoped<IEnvStore, EnvStore>();
        AddScoped<IBcrypt, Bcrypt>();
        AddScoped<IUserRepository, EfUserRepository>();
        AddScoped<UserDao, UserDao>();

        AddScoped<AuthTokenGenerator, AuthTokenGenerator>();
        AddScoped<SigninQryHandler, SigninQryHandler>();
    }

    [Fact]
    public async Task Should_Signin_User()
    {
        var user = UserMother.Active();

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
