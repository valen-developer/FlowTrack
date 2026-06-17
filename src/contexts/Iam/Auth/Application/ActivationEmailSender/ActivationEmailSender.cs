namespace FlowTrack.Iam.Auth.Application
{
    [Provider(typeof(IActivationEmailSender))]
    internal sealed class ActivationEmailSender(
        IJWTService jwtService,
        IEnvStore env,
        IMailer mailer,
        ActivationEmailGenerator mailGenerator
    ) : IActivationEmailSender
    {
        public async Task Send(ActivationEmailSenderParams @params)
        {
            var tokenSecret =
                env.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString())
                ?? throw new EnvVariableMissed(
                    IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()
                );

            var urlOfActivation =
                env.Get(IamEnvironmentKeysEnum.IAM_URL_OF_ACTIVATION.ToString())
                ?? throw new EnvVariableMissed(
                    IamEnvironmentKeysEnum.IAM_URL_OF_ACTIVATION.ToString()
                );

            var tokenOptions = new JWTOptions(secret: tokenSecret!, expirationMinutes: 60 * 24 * 7);
            var tokenPayload = new JWTPayload(
                new Dictionary<string, string> { { "id", @params.UserId.ToString() } }
            );

            var token = jwtService.Generate(payload: tokenPayload, options: tokenOptions);

            var mail = await mailGenerator.Generate(
                new(new Email(@params.Email), token, urlOfActivation)
            );

            await mailer.Send(mail);
        }
    }
}
