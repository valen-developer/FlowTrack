using FlowTrack.Shared.Domain;

namespace FlowTrack.WorkManagement.Tasks.Domain;

internal sealed record TaskId(string Value) : Uuid(Value, new InvalidTaskId());
