using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowTrack.Shared.Infrastructure.Auth;

public sealed class BearerAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    CommonAuthenticationHandler commonAuthenticationHandler
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var accessToken = authorizationHeader.ToString().Replace("Bearer ", string.Empty);

        return commonAuthenticationHandler.HandleAuthenticateWithToken(accessToken, Scheme);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        return commonAuthenticationHandler.HandleChallengeAsync(Response);
    }
}
