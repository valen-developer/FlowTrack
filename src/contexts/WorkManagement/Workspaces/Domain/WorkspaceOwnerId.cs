using FlowTrack.Shared.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Domain
{
    internal record WorkspaceOwnerId(string Value) : Uuid(Value, new InvalidWorkspaceOwnerId());
}
