namespace FlowTrack.Iam.Users.Domain;

internal record UserPassword(string Value) : ValueObject<string>(Value) { }
