using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using FlowTrack.Iam.Application;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowTrack.Iam.Infrastructure;

public sealed class IamAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    AuthValidator authValidator
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Cookies.TryGetValue("ACCESS_TOKEN", out var accessToken))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var validationResult = authValidator.ValidateAccessToken(accessToken);
        if (!validationResult.IsAuthenticated)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid access token"));
        }

        var userId = validationResult.UserId;
        if (!Guid.TryParse(userId, out _))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid user ID in access token"));
        }

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        UnAuthenticatedException exception = new();
        var (statusCode, httpReponse) = DomainToHttpExceptionMapper.Map(exception);

        Response.StatusCode = statusCode;
        return Response.WriteAsJsonAsync(httpReponse);
    }
}
