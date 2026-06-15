using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.Iam.Auth.Application
{
    [Service]
    internal sealed class ActivateUserByTokenCmdHandler(
        [FromKeyedServices("IAM")] Context context,
        IEnvStore envStore,
        IJWTService jwtService,
        IQueryBus queryBus,
        EventBus eventBus,
        IUserRepository userRepository
    ) : ICommandHandler<ActivateUserByTokenCmd>
    {
        public async Task Handle(ActivateUserByTokenCmd command)
        {
            await context.Transaction.RunInTransaction(async () =>
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
                return true;
            });
        }
    }
}
