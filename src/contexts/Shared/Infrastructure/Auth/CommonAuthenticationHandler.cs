using System.Security.Claims;
using FlowTrack.Shared.Domain.Exception;
using FlowTrack.Shared.Infrastructure.HttpErrorResponses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace FlowTrack.Shared.Infrastructure.Auth;

[Service]
public sealed class CommonAuthenticationHandler(AuthValidator authValidator)
{
    public Task<AuthenticateResult> HandleAuthenticateWithToken(
        string accessToken,
        AuthenticationScheme scheme
    )
    {
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

        var identity = new ClaimsIdentity(claims, scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    public Task HandleChallengeAsync(HttpResponse response)
    {
        UnAuthenticatedException exception = new();
        var (statusCode, httpReponse) = DomainToHttpExceptionMapper.Map(exception);

        response.StatusCode = statusCode;
        return response.WriteAsJsonAsync(httpReponse);
    }
}
