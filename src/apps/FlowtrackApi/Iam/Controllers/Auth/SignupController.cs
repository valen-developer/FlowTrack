using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowTrack.Iam.Controllers;

public class SignupController : AuthController
{
    [AllowAnonymous]
    [HttpPost("signup")]
    public async Task<IActionResult> Execute()
    {
        return StatusCode(201);
    }
}
