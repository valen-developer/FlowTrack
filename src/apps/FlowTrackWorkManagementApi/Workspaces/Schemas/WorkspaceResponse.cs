using FlowTrack.WorkManagement.Workspaces.Domain;

namespace FlowTrackWorkManagementApi.Workspaces.Schemas;

public record WorkspaceResponse(string Id, string OwnerId, string Name)
{
    internal static WorkspaceResponse FromWorkspace(Workspace workspace)
    {
        return new WorkspaceResponse(
            workspace.Id.Value,
            workspace.OwnerId.Value,
            workspace.Name.Value
        );
    }
}
