namespace FlowTrack.Iam.Test.Auth.Infrastructure;

public class SigninQryHandlerIT : IamIntegrationTestCase
{
    public SigninQryHandlerIT(IamIntegrationFixture fixture)
        : base(fixture)
    {
        fixture.AddScoped<IJWTService, JWTService>();
        fixture.AddScoped<IEnvStore, EnvStore>();
        fixture.AddScoped<IBcrypt, Bcrypt>();
        fixture.AddScoped<IUserRepository, EfUserRepository>();
        fixture.AddScoped<UserDao, UserDao>();

        fixture.AddScoped<AuthTokenGenerator, AuthTokenGenerator>();
        fixture.AddScoped<SigninQryHandler, SigninQryHandler>();
    }

    [Fact]
    public async Task Should_Signin_User()
    {
        var user = UserMother.Active();
        await AddUserToDatabase(user);

        var jwtService = _fixture.GetService<IJWTService>();
        var envStore = _fixture.GetService<IEnvStore>();

        var accessTokenSecret =
            envStore.Get(IamEnvironmentKeysEnum.ACCESS_TOKEN_SECRET.ToString())
            ?? throw new Exception($"{IamEnvironmentKeysEnum.ACCESS_TOKEN_SECRET} is not set");
        var accessTokenExpirationMinutes = 60;

        var refreshTokenSecret =
            envStore.Get(IamEnvironmentKeysEnum.REFRESH_TOKEN_SECRET.ToString())
            ?? throw new Exception("REFRESH_TOKEN_SECRET is not set");
        var refreshTokenExpirationMinutes = 60 * 24 * 30;

        var payload = new JWTPayload(new Dictionary<string, string> { ["id"] = user.Id.Value });

        var accessTokenOptions = new JWTOptions(accessTokenSecret, accessTokenExpirationMinutes);
        var refreshTokenOptions = new JWTOptions(refreshTokenSecret, refreshTokenExpirationMinutes);

        var expectedAccessToken = jwtService.Generate(payload, accessTokenOptions);
        var expectedRefreshToken = jwtService.Generate(payload, refreshTokenOptions);
        var expectedResult = new SigninSuccess(expectedAccessToken, expectedRefreshToken);

        var handler = _fixture.GetService<SigninQryHandler>();
        var qry = new SigninQry(user.Email.Value, user.Password.Value);

        var result = await handler.Handle(qry);

        Assert.Equivalent(expectedResult, result);
    }
}
