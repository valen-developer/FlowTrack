using FlowTrack.Iam.Application;
using FlowTrack.Shared.Domain;
using FlowtrackApi.Iam.Schemas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowTrack.Iam.Controllers;

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
