using System.Security.Claims;
using FlowTrack.Iam.Users.Application;
using FlowTrack.Iam.Users.Domain;
using FlowTrack.Shared.Domain.Bus.Query;
using FlowTrack.Shared.Domain.Exception;
using FlowTrackIamApi.Schemas;
using Microsoft.AspNetCore.Mvc;

namespace FlowTrackIamApi.Controllers.Users;

public sealed class UserMeController(IQueryBus queryBus) : UserController
{
    [HttpGet("me")]
    public async Task<ActionResult<UserMeResponse>> Execute()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnAuthenticatedException();

        User user = await queryBus.Ask<FindUserByIdQry, User>(new FindUserByIdQry(userId));

        return Ok(UserMeResponse.FromUser(user));
    }
}
