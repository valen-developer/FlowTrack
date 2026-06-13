using FlowTrack.Shared.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Domain;

internal class Workspace(WorkspaceId Id, WorkspaceOwnerId OwnerId, WorkspaceName Name)
    : AggregatedRoot
{
    private static readonly string DEFAULT_WORKSPACE_NAME = "Default";

    public WorkspaceId Id { get; } = Id;
    public WorkspaceOwnerId OwnerId { get; } = OwnerId;
    public WorkspaceName Name { get; } = Name;

    public static Workspace Create(WorkspaceId id, WorkspaceOwnerId ownerId, WorkspaceName name)
    {
        var workspace = new Workspace(id, ownerId, name);
        var workspaceCreatedEvent = new WorkspaceCreated(id, ownerId, name);

        workspace.Record(workspaceCreatedEvent);

        return workspace;
    }

    public static Workspace CreateDefault(WorkspaceId Id, WorkspaceOwnerId workspaceOwnerId)
    {
        return Create(Id, workspaceOwnerId, new WorkspaceName(DEFAULT_WORKSPACE_NAME));
    }
}
