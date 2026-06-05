using FlowTrack.Shared.Domain.Bus.Command;

namespace FlowTrack.Iam.Application;

public record SignupCmd(string Id, string Email, string Password) : ICommand;
