namespace FlowTrack.Iam.Auth.Application;

[Service(Lifetime.Scoped)]
internal sealed class AuthValidator(IJWTService jwtService, IEnvStore env)
{
    public AuthValidation ValidateAccessToken(string token)
    {
        var accessTokenEnvKey = IamEnvironmentKeysEnum.ACCESS_TOKEN_SECRET.ToString();
        var secret = env.Get(accessTokenEnvKey) ?? throw new EnvVariableMissed(accessTokenEnvKey);

        var validationResult = jwtService.Verify(token, secret);
        if (!validationResult)
        {
            return AuthValidation.Unauthenticated;
        }

        var payload = jwtService.Decode(token);
        if (payload == null)
        {
            return AuthValidation.Unauthenticated;
        }

        var userIdClaim = payload.Claims.FirstOrDefault(c => c.Key == "id");
        if (userIdClaim.Key == null)
        {
            return AuthValidation.Unauthenticated;
        }

        var userId = userIdClaim.Value;
        return new AuthValidation(true, userId);
    }
}
