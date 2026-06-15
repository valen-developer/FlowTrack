using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.WorkManagement.Workspaces.Domain
{
    internal class InvalidWorkspaceOwnerId()
        : InvalidException(
            "Invalid workspace owner ID.",
            "exception.workspace.owner.id.invalid"
        ) { }
}
