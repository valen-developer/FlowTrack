using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Application;

[Service]
public sealed class ActivationEmailGenerator
{
    private const string Subject = "Activate your account";

    public async Task<Mail> Generate(ActivationEmailGeneratorParams @params)
    {
        var mail = new Mail(
            to: @params.To.Value,
            subject: Subject,
            body: GenerateBody(@params.Token, @params.ActivationLinkBaseUrl)
        );

        return mail;
    }

    private string GenerateBody(string token, string baseUrl)
    {
        return $"<a href=\"{baseUrl}?token={token}\">Activate your account</a>";
    }
}
