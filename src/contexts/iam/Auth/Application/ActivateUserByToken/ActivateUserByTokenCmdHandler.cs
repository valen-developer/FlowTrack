using FlowTrack.Iam.Domain;
using FlowTrack.Shared;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Bus.Command;

namespace FlowTrack.Iam.Application;

public sealed class ActivateUserByTokenCmdHandler(
    IEnvStore envStore,
    IJWTService jwtService,
    IQueryBus queryBus,
    EventBus eventBus,
    IUserRepository userRepository
) : ICommandHandler<ActivateUserByTokenCmd>
{
    public async Task Handle(ActivateUserByTokenCmd command)
    {
        var secretKey = IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString();

        var secret = envStore.Get(secretKey) ?? throw new EnvVariableMissed(secretKey);
        var isValid = jwtService.Verify(command.Token, secret);

        if (!isValid)
        {
            throw new UnAuthenticatedException();
        }

        var decoded = jwtService.Decode(command.Token);
        var userId = decoded?.Claims["id"];

        var user = await queryBus.Ask<FindUserByIdQry, User>(new FindUserByIdQry(userId!));

        user.Activate();

        await userRepository.Update(user);

        await eventBus.Publish(user.PullDomainEvents());
    }
}
