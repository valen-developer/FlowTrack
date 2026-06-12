using System.Security.Claims;
using FlowTrack.Iam.Application;
using FlowTrack.Iam.Domain;
using FlowTrack.Iam.Schemas;
using FlowTrack.Shared;
using FlowTrack.Shared.Domain;
using FlowtrackApi;
using FlowtrackApi.Iam.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowTrack.Iam.Controllers;

public sealed class UserMeController(IQueryBus queryBus) : UserController
{
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserMeResponse>> Execute()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnAuthenticatedException();

        User user = await queryBus.Ask<FindUserByIdQry, User>(new FindUserByIdQry(userId));

        return Ok(UserMeResponse.FromUser(user));
    }
}
