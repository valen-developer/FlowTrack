using FlowTrack.Iam.Application;
using FlowTrack.Iam.Domain;
using FlowTrack.Iam.Schemas;
using FlowTrack.Iam.Services;
using FlowTrack.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FlowTrack.Iam.Controllers;

public class SigninController(IQueryBus queryBus, AuthCookieSetter cookieSetter) : AuthController
{
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
