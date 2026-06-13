namespace FlowTrack.Iam.Auth.Application;

internal sealed record ActivateUserByTokenCmd(string Token) : ICommand;
