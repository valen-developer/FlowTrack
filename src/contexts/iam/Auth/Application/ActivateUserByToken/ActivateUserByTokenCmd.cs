using FlowTrack.Shared.Domain.Bus.Command;

namespace FlowTrack.Iam.Application;

public sealed record ActivateUserByTokenCmd(string Token) : ICommand;
