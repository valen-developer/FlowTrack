using FlowTrack.Shared.Domain.Bus.Command;

namespace FlowTrack.Iam.Auth.Application;

public sealed record ActivateUserByTokenCmd(string Token) : ICommand;
