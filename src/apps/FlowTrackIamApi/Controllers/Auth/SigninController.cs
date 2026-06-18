using FlowTrack.Iam.Auth.Application;
using FlowTrack.Iam.Auth.Domain;
using FlowTrack.Shared.Domain.Bus.Query;
using FlowTrackIamApi.Schemas;
using FlowTrackIamApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowTrackIamApi.Controllers.Auth;

public class SigninController(IQueryBus queryBus, AuthCookieSetter cookieSetter) : AuthController
{
    [AllowAnonymous]
    [HttpPost("signin")]
    public async Task<IActionResult> Execute([FromBody] SigninRequestDto requestDto)
    {
        var signinSucces = await queryBus.Ask<SigninQry, SigninSuccess>(
            new SigninQry(requestDto.Email, requestDto.Password)
        );

        cookieSetter.SetAuthCookies(signinSucces);

        return StatusCode(200);
    }
}
