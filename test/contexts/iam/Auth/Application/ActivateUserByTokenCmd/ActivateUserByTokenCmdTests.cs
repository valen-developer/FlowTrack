using FlowTrack.Iam.Application;
using FlowTrack.Iam.Domain;
using FlowTrack.Shared;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.Iam.Test;

public class ActivateUserByTokenCmdTests
{
    private readonly ActivateUserByTokenCmdHandler _handler;
    private readonly Mock<IJWTService> _jwtServiceMock = new();
    private readonly Mock<IEnvStore> _envStoreMock = new();
    private readonly Mock<IQueryBus> _queryBusMock = new();

    public ActivateUserByTokenCmdTests()
    {
        var service = new ServiceCollection();
        service.AddSingleton(_jwtServiceMock.Object);
        service.AddSingleton(_envStoreMock.Object);
        service.AddSingleton(_queryBusMock.Object);

        service.AddScoped<ActivateUserByTokenCmdHandler>();

        _handler = service
            .BuildServiceProvider()
            .GetRequiredService<ActivateUserByTokenCmdHandler>();
    }

    [Fact]
    public async Task Should_Validate_The_Token()
    {
        var token = "valid_token";
        var secret = "secret_key";
        var userId = Guid.NewGuid();

        _envStoreMock
            .Setup(s => s.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()))
            .Returns(secret);

        var cmd = new ActivateUserByTokenCmd(token);
        await _handler.Handle(cmd);

        _jwtServiceMock.Verify(s => s.Verify(token, secret), Times.Once);
    }

    [Fact]
    public async Task Should_Find_The_User()
    {
        var userId = Guid.NewGuid().ToString();

        _jwtServiceMock.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtServiceMock
            .Setup(s => s.Decode(It.IsAny<string>()))
            .Returns(new JWTPayload(new Dictionary<string, string> { { "id", userId } }));

        var cmd = new ActivateUserByTokenCmd("valid_token");
        await _handler.Handle(cmd);

        _queryBusMock.Verify(
            q => q.Ask<FindUserByIdQry, User>(It.Is<FindUserByIdQry>(qry => qry.Id == userId)),
            Times.Once
        );
    }

    [Fact]
    public async Task Should_Activate_User()
    {
        var user = UserMother.Inactive();

        _jwtServiceMock.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtServiceMock
            .Setup(s => s.Decode(It.IsAny<string>()))
            .Returns(new JWTPayload(new Dictionary<string, string> { { "id", user.Id.Value } }));

        _queryBusMock
            .Setup(q => q.Ask<FindUserByIdQry, User>(It.IsAny<FindUserByIdQry>()))
            .ReturnsAsync(user);

        var cmd = new ActivateUserByTokenCmd("valid_token");
        await _handler.Handle(cmd);

        Assert.True(user.IsActive);
        Assert.IsType<UserActivated>(
            user.PullDomainEvents().FirstOrDefault(e => e is UserActivated)
        );
    }
}
