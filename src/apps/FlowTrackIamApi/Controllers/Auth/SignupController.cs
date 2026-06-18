using FlowTrack.Iam.Auth.Application;
using FlowTrack.Shared.Domain.Bus.Command;
using FlowTrack.Shared.Domain.Contexts;
using FlowTrackIamApi.Schemas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowTrackIamApi.Controllers.Auth;

public class SignupController([FromKeyedServices("IAM")] Context context, ICommandBus commandBus)
    : AuthController
{
    [AllowAnonymous]
    [HttpPost("signup")]
    public async Task<IActionResult> Execute([FromBody] SignupRequest request)
    {
        return await context.Transaction.RunInTransaction(async () =>
        {
            SignupCmd cmd = new(request.Id, request.Email, request.Password);
            await commandBus.Dispatch(cmd);

            return StatusCode(201);
        });
    }
}
