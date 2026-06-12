using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Bus.Command;

namespace FlowTrack.Iam.Application;

public sealed class ActivateUserByTokenCmdHandler(IEnvStore envStore, IJWTService jwtService)
    : ICommandHandler<ActivateUserByTokenCmd>
{
    public Task Handle(ActivateUserByTokenCmd command)
    {
        var secret = envStore.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString());
        var isValid = jwtService.Verify(command.Token, secret);

        return Task.CompletedTask;
    }
}
