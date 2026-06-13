using FlowTrack.Shared.Domain.Bus.Command;

namespace FlowTrack.Iam.Auth.Application;

internal sealed record ActivateUserByTokenCmd(string Token) : ICommand;
