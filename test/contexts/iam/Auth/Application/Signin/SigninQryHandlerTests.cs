using System.Collections.Immutable;
using FlowTrack.Iam.Auth.Application.Signin;
using FlowTrack.Iam.Auth.Domain;
using FlowTrack.Iam.Test.User;
using FlowTrack.Iam.User.Domain;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Exception;
using Moq;
using DomainUser = FlowTrack.Iam.User.Domain.User;

namespace FlowTrack.Iam.Test.Auth.Application.Signin;

public class SigninQryHandlerTests
{
    private const string ACCESS_JWT_EXPIRE_MINUTES_KEY = "ACCESS_TOKEN_EXPIRE_MINUTES";
    private const string ACCESS_JWT_SECRET_KEY = "ACCESS_TOKEN_SECRET";
    private const string REFRESH_JWT_SECRET_KEY = "REFRESH_TOKEN_SECRET";
    private const string REFRESH_JWT_EXPIRE_MINUTES_KEY = "REFRESH_TOKEN_EXPIRE_MINUTES";

    private readonly SigninQryHandler handler;
    private readonly Mock<IUserRepository> userRepositoryMock = new();
    private readonly Mock<IBcrypt> bcryptMock = new();
    private readonly Mock<IEnvStore> envStoreMock = new();
    private readonly Mock<IJWTService> jwtServiceMock = new();

    public SigninQryHandlerTests()
    {
        handler = new SigninQryHandler(
            userRepositoryMock.Object,
            bcryptMock.Object,
            envStoreMock.Object,
            jwtServiceMock.Object
        );
    }

    [Fact]
    public async Task Should_Find_User_In_Repository()
    {
        var query = new SigninQry("testuser", "password123");
        var user = UserMother.Random();

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));

        await handler.Handle(query);

        userRepositoryMock.Verify(r => r.FindByEmail(query.Email), Times.Once);
    }

    [Fact]
    public async Task Should_Compare_Password()
    {
        var query = new SigninQry("testuser", "password123");
        var user = UserMother.Random();

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));

        await handler.Handle(query);

        bcryptMock.Verify(b => b.Compare(query.Password, user.Password), Times.Once);
    }

    [Fact]
    public async Task Should_Extract_Access_Token_Secret_From_Env()
    {
        var query = new SigninQry("testuser", "password123");
        var user = UserMother.Random();

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));
        bcryptMock.Setup(b => b.Compare(query.Password, user.Password)).Returns(true);

        await handler.Handle(query);

        envStoreMock.Verify(e => e.Get(ACCESS_JWT_SECRET_KEY), Times.Once);
    }

    [Fact]
    public async Task Should_Extract_Access_Token_ExpireMinutes_From_Env()
    {
        var query = new SigninQry("testuser", "password123");
        var user = UserMother.Random();

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));
        bcryptMock.Setup(b => b.Compare(query.Password, user.Password)).Returns(true);

        await handler.Handle(query);

        envStoreMock.Verify(e => e.Get(ACCESS_JWT_EXPIRE_MINUTES_KEY), Times.Once);
    }

    [Fact]
    public async Task Should_Generate_Access_Token()
    {
        var accessTokenSecret = "secret";
        var accessTokenExpireMinutes = "60";
        var query = new SigninQry("testuser", "password123");
        var user = UserMother.Random();

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));
        bcryptMock.Setup(b => b.Compare(query.Password, user.Password)).Returns(true);
        envStoreMock.Setup(e => e.Get(ACCESS_JWT_SECRET_KEY)).Returns(accessTokenSecret);
        envStoreMock
            .Setup(e => e.Get(ACCESS_JWT_EXPIRE_MINUTES_KEY))
            .Returns(accessTokenExpireMinutes);

        await handler.Handle(query);

        var expectedPayload = ExpectedPayload(user);
        var expectedAccessJwtOptions = new JWTOptions(
            accessTokenSecret,
            int.Parse(accessTokenExpireMinutes)
        );

        jwtServiceMock.Verify(
            j =>
                j.Generate(
                    It.Is<JWTPayload>(p => HaveSameJwtPayload(p, expectedPayload)),
                    It.Is<JWTOptions>(o => HaveSameJWTOptions(o, expectedAccessJwtOptions))
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Should_Extract_Refresh_Token_Secret_From_Env()
    {
        var query = new SigninQry("testuser", "password123");
        var user = UserMother.Random();

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));
        bcryptMock.Setup(b => b.Compare(query.Password, user.Password)).Returns(true);

        await handler.Handle(query);

        envStoreMock.Verify(e => e.Get(REFRESH_JWT_SECRET_KEY), Times.Once);
    }

    [Fact]
    public async Task Should_Extract_Refresh_Token_ExpireMinutes_From_Env()
    {
        var query = new SigninQry("testuser", "password123");
        var user = UserMother.Random();

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));
        bcryptMock.Setup(b => b.Compare(query.Password, user.Password)).Returns(true);

        await handler.Handle(query);

        envStoreMock.Verify(e => e.Get(REFRESH_JWT_EXPIRE_MINUTES_KEY), Times.Once);
    }

    [Fact]
    public async Task Should_Generate_Refresh_Token()
    {
        var refreshTokenSecret = "refreshSecret";
        var refreshTokenExpireMinutes = "120";
        var query = new SigninQry("testuser", "password123");
        var user = UserMother.Random();

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));
        bcryptMock.Setup(b => b.Compare(query.Password, user.Password)).Returns(true);
        envStoreMock.Setup(e => e.Get(REFRESH_JWT_SECRET_KEY)).Returns(refreshTokenSecret);
        envStoreMock
            .Setup(e => e.Get(REFRESH_JWT_EXPIRE_MINUTES_KEY))
            .Returns(refreshTokenExpireMinutes);

        await handler.Handle(query);

        var expectedPayload = ExpectedPayload(user);
        var expectedRefreshJwtOptions = new JWTOptions(
            refreshTokenSecret,
            int.Parse(refreshTokenExpireMinutes)
        );

        jwtServiceMock.Verify(
            j =>
                j.Generate(
                    It.Is<JWTPayload>(p => HaveSameJwtPayload(p, expectedPayload)),
                    It.Is<JWTOptions>(o => HaveSameJWTOptions(o, expectedRefreshJwtOptions))
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Should_Return_Singin_Successfully()
    {
        var accessToken = "accessToken";
        var refreshToken = "refreshToken";

        var accessTokenSecret = "secret";
        var accessTokenExpireMinutes = "60";

        var refreshTokenSecret = "refreshSecret";
        var refreshTokenExpireMinutes = "120";

        var query = new SigninQry("testuser", "password123");
        var user = UserMother.Random();

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));
        bcryptMock.Setup(b => b.Compare(query.Password, user.Password)).Returns(true);
        envStoreMock.Setup(e => e.Get(ACCESS_JWT_SECRET_KEY)).Returns(accessTokenSecret);
        envStoreMock
            .Setup(e => e.Get(ACCESS_JWT_EXPIRE_MINUTES_KEY))
            .Returns(accessTokenExpireMinutes);
        envStoreMock.Setup(e => e.Get(REFRESH_JWT_SECRET_KEY)).Returns(refreshTokenSecret);
        envStoreMock
            .Setup(e => e.Get(REFRESH_JWT_EXPIRE_MINUTES_KEY))
            .Returns(refreshTokenExpireMinutes);

        jwtServiceMock
            .Setup(j =>
                j.Generate(
                    It.IsAny<JWTPayload>(),
                    It.Is<JWTOptions>(o =>
                        HaveSameJWTOptions(
                            o,
                            new JWTOptions(accessTokenSecret, int.Parse(accessTokenExpireMinutes))
                        )
                    )
                )
            )
            .Returns(accessToken);

        jwtServiceMock
            .Setup(j =>
                j.Generate(
                    It.IsAny<JWTPayload>(),
                    It.Is<JWTOptions>(o =>
                        HaveSameJWTOptions(
                            o,
                            new JWTOptions(refreshTokenSecret, int.Parse(refreshTokenExpireMinutes))
                        )
                    )
                )
            )
            .Returns(refreshToken);

        var expectedResult = new SigninSuccess(accessToken, refreshToken);

        var result = await handler.Handle(query);

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task Should_Throw_Exception_When_User_Not_Found()
    {
        var query = new SigninQry("nonexistentuser", "password123");

        userRepositoryMock
            .Setup(r => r.FindByEmail(query.Email))
            .Returns(Task.FromResult<DomainUser?>(null));

        var exception = await Assert.ThrowsAsync<SigninFailed>(() => handler.Handle(query));
        Assert.True(IsSigninFailedException(exception));
    }

    [Fact]
    public async Task Should_Throw_Exception_When_Password_Does_Not_Match()
    {
        var query = new SigninQry("testuser", "wrongpassword");
        var user = UserMother.Random();

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));
        bcryptMock.Setup(b => b.Compare(query.Password, user.Password)).Returns(false);

        var exception = await Assert.ThrowsAsync<SigninFailed>(() => handler.Handle(query));
        Assert.True(IsSigninFailedException(exception));
    }

    [Fact]
    public async Task Should_Throw_Exception_When_Access_Token_Secret_Not_Found_In_Env()
    {
        var query = new SigninQry("testuser", "password123");
        var user = UserMother.Random();

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));
        bcryptMock.Setup(b => b.Compare(query.Password, user.Password)).Returns(true);
        envStoreMock.Setup(e => e.Get(ACCESS_JWT_SECRET_KEY)).Returns<string?>(null);

        var exception = await Assert.ThrowsAsync<EnvVariableMissed>(() => handler.Handle(query));
        Assert.IsType<InternalException>(exception, exactMatch: false);
        Assert.Equal("exception.internal.env_variable_missed", exception.Code);
        Assert.Equal(
            $"Environment variable {ACCESS_JWT_SECRET_KEY} is required",
            exception.Message
        );
    }

    private static Boolean IsSigninFailedException(Exception ex) =>
        ex is SigninFailed signinFailed
        && signinFailed.Code == "exception.iam.auth.signin_failed"
        && signinFailed.Message == "Invalid Signin credentials.";

    private static bool HaveSameJWTOptions(JWTOptions actual, JWTOptions expected)
    {
        return actual.Secret == expected.Secret
            && actual.ExpirationMinutes == expected.ExpirationMinutes;
    }

    private static bool HaveSameJwtPayload(JWTPayload actual, JWTPayload expected) =>
        actual.Claims.Count == expected.Claims.Count
        && actual.Claims.All(kvp =>
            expected.Claims.TryGetValue(kvp.Key, out var value) && value == kvp.Value
        );

    private JWTPayload ExpectedPayload(DomainUser user) =>
        new(
            new Dictionary<string, string> { { "id", user.Id.ToString() } }.ToImmutableDictionary()
        );
}
