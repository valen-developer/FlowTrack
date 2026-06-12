using FlowTrack.Iam.Domain;
using FlowTrack.Shared;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Bus.Command;

namespace FlowTrack.Iam.Application;

public sealed class ActivateUserByTokenCmdHandler(
    IEnvStore envStore,
    IJWTService jwtService,
    IQueryBus queryBus
) : ICommandHandler<ActivateUserByTokenCmd>
{
    public async Task Handle(ActivateUserByTokenCmd command)
    {
        var secret = envStore.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString());
        var isValid = jwtService.Verify(command.Token, secret);

        var decoded = jwtService.Decode(command.Token);
        var userId = decoded?.Claims["id"];

        var user = await queryBus.Ask<FindUserByIdQry, User>(new FindUserByIdQry(userId));

        user?.Activate();
    }
}
