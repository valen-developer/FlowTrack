using FlowTrack.Shared.Domain.ValueObjects;

namespace FlowTrack.Iam.Users.Domain;

internal record UserEmail(string Value) : Email(Value) { }
