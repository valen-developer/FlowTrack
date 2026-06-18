using FlowTrack.Shared.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FlowTrackIamApi.Users.Controllers;

[ApiController]
[Route("user")]
public abstract class UserController : IController { }
