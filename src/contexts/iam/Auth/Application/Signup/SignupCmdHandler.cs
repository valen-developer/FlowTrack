using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Bus.Command;

namespace FlowTrack.Iam.Application;

public sealed class SignupCmdHandler(IUserRepository repository) : ICommandHandler<SignupCmd>
{
    public async Task Handle(SignupCmd command)
    {
        Password password = Password.EnsurePassword(command.Password);
        Email email = new(command.Email);

        var user = User.Create(command.Id, email.Value, password.Value, false);
        await repository.Create(user);
    }
}
