using System.Security.Claims;
using FlowTrack.Shared.Domain.Bus.Query;
using FlowTrack.Shared.Domain.Exception;
using FlowTrack.WorkManagement.Workspaces.Application;
using FlowTrack.WorkManagement.Workspaces.Domain;
using FlowTrackWorkManagementApi.Workspaces.Schemas;
using Microsoft.AspNetCore.Mvc;

namespace FlowTrackWorkManagementApi.Workspaces.Controllers;

public sealed class WorkspaceGETController(IQueryBus queryBus) : WorkspaceController
{
    [HttpGet()]
    public async Task<ActionResult<List<WorkspaceResponse>>> GetWorkspaces()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnAuthenticatedException();

        var workspaces = await queryBus.Ask<FindWorkspacesByOwnerQry, List<Workspace>>(
            new FindWorkspacesByOwnerQry(userId)
        );

        return Ok(workspaces.Select(WorkspaceResponse.FromWorkspace).ToList());
    }
}
