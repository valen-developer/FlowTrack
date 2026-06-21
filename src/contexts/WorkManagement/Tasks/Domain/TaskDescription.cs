using FlowTrack.Shared.Domain.ValueObjects;

namespace FlowTrack.WorkManagement.Tasks.Domain;

internal record TaskDescription(string? Value) : ValueObject<string>(Value ?? "");
