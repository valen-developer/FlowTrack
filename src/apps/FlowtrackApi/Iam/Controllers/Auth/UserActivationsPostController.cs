using FlowtrackApi.Iam.Schemas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowtrackApi.Iam.Controllers.Auth
{
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
}
