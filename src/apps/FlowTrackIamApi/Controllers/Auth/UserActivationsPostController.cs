using FlowTrack.Iam.Auth.Application;
using FlowTrack.Shared.Domain.Bus.Command;
using FlowTrack.Shared.Infrastructure;
using FlowTrackIamApi.Schemas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowTrackIamApi.Controllers.Auth;

[Route("user-activations")]
public sealed class UserActivationsPostController(ICommandBus commandBus) : IController
{
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] UserActivationByTokenRequest request)
    {
        var command = new ActivateUserByTokenCmd(request.Token);
        await commandBus.Dispatch(command);
        return Created();
    }
}
