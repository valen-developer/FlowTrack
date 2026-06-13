using FlowTrack.Shared.Domain.ValueObjects;

namespace FlowTrack.WorkManagement.Workspaces.Domain;

internal record WorkspaceName(string Value) : ValueObject<string>(Value) { }
