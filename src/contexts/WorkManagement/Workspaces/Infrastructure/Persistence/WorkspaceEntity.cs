using FlowTrack.WorkManagement.Workspaces.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Infrastructure.Persistence;

internal class WorkspaceEntity
{
    public required Guid Id { get; set; }
    public required Guid OwnerId { get; set; }
    public required string Name { get; set; }

    public static WorkspaceEntity FromDomain(Workspace workspace)
    {
        return new WorkspaceEntity()
        {
            Id = new Guid(workspace.Id.Value),
            OwnerId = new Guid(workspace.OwnerId.Value),
            Name = workspace.Name.Value,
        };
    }

    public Workspace ToDomain()
    {
        return new Workspace(
            new WorkspaceId(Id.ToString()),
            new WorkspaceOwnerId(OwnerId.ToString()),
            new WorkspaceName(Name)
        );
    }
}
