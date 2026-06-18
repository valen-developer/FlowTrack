using FlowTrack.Shared.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FlowTrackIamApi.Controllers.Users;

[ApiController]
[Route("user")]
public abstract class UserController : IController { }
