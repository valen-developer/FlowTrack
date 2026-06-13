using System.Collections.Immutable;

namespace FlowTrack.Iam.Auth.Application;

[Service(Lifetime.Scoped)]
internal sealed class AuthTokenGenerator(IEnvStore envStore, IJWTService jwtService)
{
    private const int DefaultAccessTokenExpireInMinutes = 60;
    private const int DefaultRefreshTokenExpireInMinutes = 60 * 24 * 30;

    public SigninSuccess Generate(User user)
    {
        var payload = GeneratePayload(user);
        var accessTokenOptions = GenerateAccessTokenOptions();
        var refreshTokenOptions = GenerateRefreshTokenOptions();

        var accessToken = jwtService.Generate(payload, accessTokenOptions);
        var refreshToken = jwtService.Generate(payload, refreshTokenOptions);

        return new SigninSuccess(accessToken, refreshToken);
    }

    private JWTPayload GeneratePayload(User user)
    {
        return new JWTPayload(
            new Dictionary<string, string> { { "id", user.Id.Value } }.ToImmutableDictionary()
        );
    }

    private JWTOptions GenerateAccessTokenOptions()
    {
        var accessTokenSecret =
            envStore.Get(IamEnvironmentKeysEnum.ACCESS_TOKEN_SECRET.ToString())
            ?? throw new EnvVariableMissed(IamEnvironmentKeysEnum.ACCESS_TOKEN_SECRET.ToString());

        var accessTokenExpireIn =
            envStore.Get(IamEnvironmentKeysEnum.ACCESS_TOKEN_EXPIRE_MINUTES.ToString())
            ?? DefaultAccessTokenExpireInMinutes.ToString();

        return new JWTOptions(accessTokenSecret, int.Parse(accessTokenExpireIn));
    }

    private JWTOptions GenerateRefreshTokenOptions()
    {
        var refreshTokenSecret =
            envStore.Get(IamEnvironmentKeysEnum.REFRESH_TOKEN_SECRET.ToString())
            ?? throw new EnvVariableMissed(IamEnvironmentKeysEnum.REFRESH_TOKEN_SECRET.ToString());

        var refreshTokenExpireIn =
            envStore.Get(IamEnvironmentKeysEnum.REFRESH_TOKEN_EXPIRE_MINUTES.ToString())
            ?? DefaultRefreshTokenExpireInMinutes.ToString();

        return new JWTOptions(refreshTokenSecret, int.Parse(refreshTokenExpireIn));
    }
}
