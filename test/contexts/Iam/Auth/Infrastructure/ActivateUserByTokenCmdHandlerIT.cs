namespace FlowTrack.Iam.Test.Auth.Infrastructure
{
    [Service(Lifetime.Singleton)]
    [DomainEventSubscriber(typeof(UserActivated))]
    internal sealed class UserActivatedEventSubscriber
    {
        public UserActivated? CapturedEvent { get; private set; }

        [DomainEventListener]
        public async Task On(UserActivated domainEvent)
        {
            CapturedEvent = domainEvent;
        }
    }

    public class ActivateUserByTokenCmdHandlerIT : IamIntegrationTestCase
    {
        public ActivateUserByTokenCmdHandlerIT(IamIntegrationFixture fixture)
            : base(fixture) { }

        [Fact]
        public async Task Should_Save_User_As_Active()
        {
            await _fixture.EnsureServicesAsync();

            var user = UserMother.Inactive();
            await AddUserToDatabase(user);

            var jwtService = _fixture.GetService<IJWTService>();
            var envStore = _fixture.GetService<IEnvStore>();

            var activeTokenSecret =
                envStore.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString())
                ?? throw new Exception(
                    $"{IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET} is not set"
                );

            var activeTokenExpirationMinutes = 60 * 24;
            var payload = new JWTPayload(new Dictionary<string, string> { ["id"] = user.Id.Value });
            var activeTokenOptions = new JWTOptions(
                activeTokenSecret,
                activeTokenExpirationMinutes
            );
            var activeToken = jwtService.Generate(payload, activeTokenOptions);

            var cmd = new ActivateUserByTokenCmd(activeToken);
            var handler = _fixture.GetService<ActivateUserByTokenCmdHandler>();

            await handler.Handle(cmd);

            var sqlResult = await _fixture.ExecuteQueryAsync<UserEntity>(
                $"SELECT * FROM \"users\" WHERE \"Id\" = '{user.Id.Value}'"
            );

            var userEntity = sqlResult.FirstOrDefault();

            Assert.NotNull(userEntity);
            Assert.True(userEntity!.IsActive);
        }

        [Fact]
        public async Task Should_Publish_UserActivated_Event()
        {
            await _fixture.EnsureServicesAsync();

            var user = UserMother.Inactive();
            await AddUserToDatabase(user);

            var jwtService = _fixture.GetService<IJWTService>();
            var envStore = _fixture.GetService<IEnvStore>();

            var activeTokenSecret =
                envStore.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString())
                ?? throw new Exception(
                    $"{IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET} is not set"
                );

            var activeTokenExpirationMinutes = 60 * 24;
            var payload = new JWTPayload(new Dictionary<string, string> { ["id"] = user.Id.Value });
            var activeTokenOptions = new JWTOptions(
                activeTokenSecret,
                activeTokenExpirationMinutes
            );
            var activeToken = jwtService.Generate(payload, activeTokenOptions);

            var cmd = new ActivateUserByTokenCmd(activeToken);
            var handler = _fixture.GetService<ActivateUserByTokenCmdHandler>();
            var eventSubscriber = _fixture.GetService<UserActivatedEventSubscriber>();

            await handler.Handle(cmd);

            await WaitForAsync(async () =>
            {
                if (eventSubscriber.CapturedEvent is null)
                {
                    return false;
                }

                return true;
            });

            Assert.NotNull(eventSubscriber.CapturedEvent);
            Assert.Equal(user.Id.Value, eventSubscriber.CapturedEvent!.Id);
        }
    }
}
