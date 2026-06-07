using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Application;

public sealed class ActivationEmailSender(IJWTService jwtService, IEnvStore env)
{
    public async Task Send(ActivationEmailSenderParams @params)
    {
        var tokenSecret = env.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString());
        var tokenOptions = new JWTOptions(secret: tokenSecret!, expirationMinutes: 60 * 24 * 7);
        var tokenPayload = new JWTPayload(
            new Dictionary<string, string> { { "id", @params.UserId.ToString() } }
        );

        jwtService.Generate(payload: tokenPayload, options: tokenOptions);
    }
}
