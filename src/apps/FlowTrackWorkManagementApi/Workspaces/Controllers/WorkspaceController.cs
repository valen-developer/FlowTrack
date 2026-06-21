using FlowTrack.Shared.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FlowTrackWorkManagementApi.Workspaces.Controllers;

[ApiController]
[Route("workspace")]
public abstract class WorkspaceController : IController { }
