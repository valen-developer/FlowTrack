using FlowTrack.Shared.Domain;
using FlowTrack.WorkManagement.Workspaces.Domain.Exceptions;

namespace FlowTrack.WorkManagement.Workspaces.Domain;

internal record WorkspaceId(string Value) : Uuid(Value, new InvalidWorkspaceId()) { }
