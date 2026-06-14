using FlowTrack.Shared.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Domain;

internal class Workspace(WorkspaceId Id, WorkspaceOwnerId OwnerId, WorkspaceName Name)
    : AggregatedRoot
{
    public static readonly string DefaultName = "Default";

    public WorkspaceId Id { get; } = Id;
    public WorkspaceOwnerId OwnerId { get; } = OwnerId;
    public WorkspaceName Name { get; } = Name;

    public static Workspace Create(WorkspaceId id, WorkspaceOwnerId ownerId, WorkspaceName name)
    {
        var workspace = new Workspace(id, ownerId, name);
        var workspaceCreatedEvent = new WorkspaceCreated(id.Value, ownerId.Value, name.Value);

        workspace.Record(workspaceCreatedEvent);

        return workspace;
    }

    public static Workspace CreateDefault(WorkspaceId Id, WorkspaceOwnerId workspaceOwnerId)
    {
        return Create(Id, workspaceOwnerId, new WorkspaceName(DefaultName));
    }
}
