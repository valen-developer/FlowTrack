using FlowTrack.WorkManagement.Workspaces.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Infrastructure;

internal record WorkspaceSearchDocument(string Id, string OwnerId, string Name)
{
    public string Id { get; } = Id;
    public string OwnerId { get; } = OwnerId;
    public string Name { get; } = Name;

    public static WorkspaceSearchDocument FromDomain(Workspace workspace)
    {
        return new WorkspaceSearchDocument(
            Id: workspace.Id.Value,
            OwnerId: workspace.OwnerId.Value,
            Name: workspace.Name.Value
        );
    }

    public Workspace ToDomain()
    {
        return new Workspace(
            Id: new WorkspaceId(Id),
            OwnerId: new WorkspaceOwnerId(OwnerId),
            Name: new WorkspaceName(Name)
        );
    }
}
