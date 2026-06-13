using FlowTrack.Shared.Test;
using FlowTrack.WorkManagement.Workspaces.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Test;

public class WorkspaceMother : ObjectMother
{
    private static WorkspaceId RandomWorkspaceId() => new(Guid.NewGuid().ToString());

    private static WorkspaceOwnerId RandomWorkspaceOwnerId() => new(Guid.NewGuid().ToString());

    private static WorkspaceName RandomWorkspaceName() => new(faker.Words(1));

    internal static Workspace WithNameAndOwner(string name, string ownerId)
    {
        return new Workspace(
            RandomWorkspaceId(),
            new WorkspaceOwnerId(ownerId),
            new WorkspaceName(name)
        );
    }
}
