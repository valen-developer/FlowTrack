using FlowTrack.Shared.Domain;

namespace FlowTrack.WorkManagement.Tasks.Domain;

internal record TaskOwnerId(string Value) : Uuid(Value, new InvalidTaskOwnerId());
