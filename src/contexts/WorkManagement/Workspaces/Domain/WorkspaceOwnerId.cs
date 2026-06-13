using FlowTrack.Shared.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Domain;

public record WorkspaceOwnerId(string Value) : Uuid(Value, new InvalidWorkspaceOwnerId());
