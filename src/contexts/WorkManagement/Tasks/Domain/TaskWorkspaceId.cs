using FlowTrack.Shared.Domain;

namespace FlowTrack.WorkManagement.Tasks.Domain;

internal record TaskWorkspaceId(string Value) : Uuid(Value, new InvalidTaskWorkspaceId()) { }
