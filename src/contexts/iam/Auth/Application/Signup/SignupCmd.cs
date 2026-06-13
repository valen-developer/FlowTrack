using FlowTrack.Shared.Domain.Bus.Command;

namespace FlowTrack.Iam.Auth.Application;

public record SignupCmd(string Id, string Email, string Password) : ICommand;
