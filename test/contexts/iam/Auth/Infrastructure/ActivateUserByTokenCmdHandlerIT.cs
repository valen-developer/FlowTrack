using FlowTrack.Iam.Application;
using FlowTrack.Iam.Infrastructure;
using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Test.Infrastructure;

public class ActivateUserByTokenCmdHandlerIT : IamIntegrationTestCase
{
    public ActivateUserByTokenCmdHandlerIT(IamIntegrationFixture fixture)
        : base(fixture)
    {
        fixture.serviceCollection.DiscoverServices(["FlowTrack*.dll"]);
    }

    [Fact]
    public async Task Should_Save_User_As_Active()
    {
        var user = UserMother.Inactive();
        await AddUserToDatabase(user);

        var jwtService = _fixture.GetService<IJWTService>();
        var envStore = _fixture.GetService<IEnvStore>();

        var activeTokenSecret =
            envStore.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString())
            ?? throw new Exception($"{IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET} is not set");

        var activeTokenExpirationMinutes = 60 * 24;
        var payload = new JWTPayload(new Dictionary<string, string> { ["id"] = user.Id.Value });
        var activeTokenOptions = new JWTOptions(activeTokenSecret, activeTokenExpirationMinutes);
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
}
