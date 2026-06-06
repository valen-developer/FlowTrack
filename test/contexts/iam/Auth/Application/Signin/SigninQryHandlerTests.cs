using System.Collections.Immutable;
using FlowTrack.Iam.Application;
using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Test;

public class SigninQryHandlerTests
{
    [Fact]
    public async Task Should_Generate_Access_Token()
    {
        var accessTokenSecret = "secret";
        var accessTokenExpireMinutes = "60";
        var query = new SigninQry("testuser", "password123");
        var user = UserMother.Active();

        var to = new SigninQryHandlerTestObject()
            .DefaultMocks()
            .WithUserByEmail(query.Email, user)
            .WithAccessTokenEnv(accessTokenSecret)
            .WithAccessTokenExpirationMinutesEnv(int.Parse(accessTokenExpireMinutes));

        var handler = to.handler;

        await handler.Handle(query);

        var expectedPayload = ExpectedPayload(user);
        var expectedAccessJwtOptions = new JWTOptions(
            accessTokenSecret,
            int.Parse(accessTokenExpireMinutes)
        );

        to.AssertJWTServiceCalledWith(
            p => HaveSameJwtPayload(p, expectedPayload),
            o => HaveSameJWTOptions(o, expectedAccessJwtOptions)
        );
    }

    [Fact]
    public async Task Should_Generate_Refresh_Token()
    {
        var refreshTokenSecret = "refreshSecret";
        var refreshTokenExpireMinutes = "120";
        var user = UserMother.Active();

        var query = new SigninQry("testuser", "password123");

        var to = new SigninQryHandlerTestObject()
            .DefaultMocks()
            .WithUserByEmail(query.Email, user)
            .WithRefreshTokenSecretEnv(refreshTokenSecret)
            .WithRefreshTokenExpirationMinutesEnv(int.Parse(refreshTokenExpireMinutes));

        var handler = to.handler;
        await handler.Handle(query);

        var expectedPayload = ExpectedPayload(user);
        var expectedRefreshJwtOptions = new JWTOptions(
            refreshTokenSecret,
            int.Parse(refreshTokenExpireMinutes)
        );

        to.AssertJWTServiceCalledWith(
            p => HaveSameJwtPayload(p, expectedPayload),
            o => HaveSameJWTOptions(o, expectedRefreshJwtOptions)
        );
    }

    [Fact]
    public async Task Should_Return_Singin_Successfully()
    {
        var accessToken = "accessToken";
        var refreshToken = "refreshToken";

        var query = new SigninQry("testuser", "password123");

        var to = new SigninQryHandlerTestObject()
            .DefaultMocks()
            .WithAccessToken(accessToken)
            .WithRefreshToken(refreshToken);

        var handler = to.handler;

        var expectedResult = new SigninSuccess(accessToken, refreshToken);
        var result = await handler.Handle(query);

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task Should_Throw_Exception_When_User_Not_Found()
    {
        var query = new SigninQry("nonexistentuser", "password123");

        var to = new SigninQryHandlerTestObject().DefaultMocks().WithUserByEmail(query.Email, null);
        var handler = to.handler;

        var exception = await Assert.ThrowsAsync<SigninFailed>(() => handler.Handle(query));
        to.AssertIsSigninFailed(exception);
    }

    [Fact]
    public async Task Should_Throw_Exception_When_Password_Does_Not_Match()
    {
        var query = new SigninQry("testuser", "wrongpassword");

        var to = new SigninQryHandlerTestObject()
            .DefaultMocks()
            .WithInvalidPassword(query.Password);

        var handler = to.handler;

        var exception = await Assert.ThrowsAsync<SigninFailed>(() => handler.Handle(query));
        to.AssertIsSigninFailed(exception);
    }

    [Fact]
    public async Task Should_Throw_Exception_When_Access_Token_Secret_Not_Found_In_Env()
    {
        var query = new SigninQry("testuser", "password123");

        var to = new SigninQryHandlerTestObject().DefaultMocks().WithAccessTokenEnv(null);
        var handler = to.handler;

        var exception = await Assert.ThrowsAsync<EnvVariableMissed>(() => handler.Handle(query));
        Assert.IsType<InternalException>(exception, exactMatch: false);
        Assert.Equal("exception.internal.env_variable_missed", exception.Code);
        Assert.Equal(
            $"Environment variable {SigninQryHandlerTestObject.ACCESS_JWT_SECRET_KEY} is required",
            exception.Message
        );
    }

    [Fact]
    public async Task Should_Throw_Exception_When_Refresh_Token_Secret_Not_Found_In_Env()
    {
        var query = new SigninQry("testuser", "password123");

        var to = new SigninQryHandlerTestObject().DefaultMocks().WithRefreshTokenSecretEnv(null);

        var user = UserMother.Active();
        var handler = to.handler;

        var exception = await Assert.ThrowsAsync<EnvVariableMissed>(() => handler.Handle(query));
        Assert.IsType<InternalException>(exception, exactMatch: false);
        Assert.Equal("exception.internal.env_variable_missed", exception.Code);
        Assert.Equal(
            $"Environment variable {SigninQryHandlerTestObject.REFRESH_JWT_SECRET_KEY} is required",
            exception.Message
        );
    }

    [Fact]
    public async Task Should_Set_Default_Access_Token_Expiration_Time()
    {
        var expectedAccessTokenExpireMinutes = 60;

        var to = new SigninQryHandlerTestObject()
            .DefaultMocks()
            .WithAccessTokenExpirationMinutesEnv(null);

        var query = new SigninQry("testuser", "password123");
        var handler = to.handler;

        await handler.Handle(query);

        to.AssertJWTServiceCalledWith(
            _ => true,
            options => options.ExpirationMinutes == expectedAccessTokenExpireMinutes
        );
    }

    [Fact]
    public async Task Should_Set_Default_Refresh_Token_Expiration_Time()
    {
        var expectedRefreshTokenExpireMinutes = 60 * 24 * 30;
        var query = new SigninQry("testuser", "password123");

        var to = new SigninQryHandlerTestObject()
            .DefaultMocks()
            .WithRefreshTokenExpirationMinutesEnv(null);

        var handler = to.handler;

        await handler.Handle(query);

        to.AssertJWTServiceCalledWith(
            _ => true,
            options => options.ExpirationMinutes == expectedRefreshTokenExpireMinutes
        );
    }

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

    private JWTPayload ExpectedPayload(User user) =>
        new(
            new Dictionary<string, string> { { "id", user.Id.ToString() } }.ToImmutableDictionary()
        );
}
