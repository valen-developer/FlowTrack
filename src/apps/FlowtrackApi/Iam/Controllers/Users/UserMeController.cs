using System.Security.Claims;
using FlowtrackApi.Iam.Schemas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowtrackApi.Iam.Controllers.Users
{
    public sealed class UserMeController(IQueryBus queryBus) : UserController
    {
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserMeResponse>> Execute()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnAuthenticatedException();

            User user = await queryBus.Ask<FindUserByIdQry, User>(new FindUserByIdQry(userId));

            return Ok(UserMeResponse.FromUser(user));
        }
    }
}
