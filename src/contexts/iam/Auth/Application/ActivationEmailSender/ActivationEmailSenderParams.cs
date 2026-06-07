namespace FlowTrack.Iam.Application;

public sealed record ActivationEmailSenderParams(Guid UserId, string Email);
