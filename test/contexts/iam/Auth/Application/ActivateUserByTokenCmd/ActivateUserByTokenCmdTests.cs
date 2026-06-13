using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.Iam.Test.Auth.Application;

public class ActivateUserByTokenCmdTests
{
    private readonly ActivateUserByTokenCmdHandler _handler;
    private readonly Mock<IJWTService> _jwtServiceMock = new();
    private readonly Mock<IEnvStore> _envStoreMock = new();
    private readonly Mock<IQueryBus> _queryBusMock = new();
    private readonly Mock<IDomainEventBus> _eventBusMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();

    public ActivateUserByTokenCmdTests()
    {
        var service = new ServiceCollection();
        service.AddSingleton(_jwtServiceMock.Object);
        service.AddSingleton(_envStoreMock.Object);
        service.AddSingleton(_queryBusMock.Object);
        service.AddSingleton(_eventBusMock.Object);
        service.AddSingleton(_userRepositoryMock.Object);

        var context = new Context(new DummyTransaction());
        service.AddKeyedScoped("IAM", (_, _) => context);

        service.AddSingleton(Mock.Of<IExternalEventBus>());
        service.AddSingleton<EventBus>();
        service.AddScoped<ActivateUserByTokenCmdHandler>();

        _handler = service
            .BuildServiceProvider()
            .GetRequiredService<ActivateUserByTokenCmdHandler>();
    }

    [Fact]
    public async Task Should_Get_The_Secret_From_EnvStore()
    {
        _envStoreMock
            .Setup(s => s.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()))
            .Returns("secret_key");

        _jwtServiceMock.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        _queryBusMock
            .Setup(q => q.Ask<FindUserByIdQry, User>(It.IsAny<FindUserByIdQry>()))
            .ReturnsAsync(UserMother.Inactive());

        var cmd = new ActivateUserByTokenCmd("valid_token");
        await _handler.Handle(cmd);

        _envStoreMock.Verify(
            s => s.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()),
            Times.Once
        );
    }

    [Fact]
    public async Task Should_Throw_Env_Variable_Missed_Exception()
    {
        _envStoreMock
            .Setup(s => s.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()))
            .Returns<string?>(null);

        var cmd = new ActivateUserByTokenCmd("valid_token");
        await Assert.ThrowsAsync<EnvVariableMissed>(() => _handler.Handle(cmd));
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

        _jwtServiceMock.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        _queryBusMock
            .Setup(q => q.Ask<FindUserByIdQry, User>(It.IsAny<FindUserByIdQry>()))
            .ReturnsAsync(UserMother.Inactive());

        var cmd = new ActivateUserByTokenCmd(token);
        await _handler.Handle(cmd);

        _jwtServiceMock.Verify(s => s.Verify(token, secret), Times.Once);
    }

    [Fact]
    public async Task Should_Throw_Unauthorized_Exception()
    {
        _envStoreMock
            .Setup(s => s.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()))
            .Returns("secret_key");

        _jwtServiceMock.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var cmd = new ActivateUserByTokenCmd("invalid_token");
        await Assert.ThrowsAsync<UnAuthenticatedException>(() => _handler.Handle(cmd));
    }

    [Fact]
    public async Task Should_Find_The_User()
    {
        var userId = Guid.NewGuid().ToString();

        _envStoreMock
            .Setup(s => s.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()))
            .Returns("secret_key");
        _jwtServiceMock.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtServiceMock
            .Setup(s => s.Decode(It.IsAny<string>()))
            .Returns(new JWTPayload(new Dictionary<string, string> { { "id", userId } }));

        _queryBusMock
            .Setup(q => q.Ask<FindUserByIdQry, User>(It.IsAny<FindUserByIdQry>()))
            .ReturnsAsync(UserMother.Inactive());

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
        _envStoreMock
            .Setup(s => s.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()))
            .Returns("secret_key");

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
    }

    [Fact]
    public async Task Should_Publish_UserActivated_Event()
    {
        var user = UserMother.Inactive();

        _envStoreMock
            .Setup(s => s.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()))
            .Returns("secret_key");

        _jwtServiceMock.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtServiceMock
            .Setup(s => s.Decode(It.IsAny<string>()))
            .Returns(new JWTPayload(new Dictionary<string, string> { { "id", user.Id.Value } }));

        _queryBusMock
            .Setup(q => q.Ask<FindUserByIdQry, User>(It.IsAny<FindUserByIdQry>()))
            .ReturnsAsync(user);

        var cmd = new ActivateUserByTokenCmd("valid_token");
        await _handler.Handle(cmd);

        _eventBusMock.Verify(
            e =>
                e.Publish(
                    It.Is<IEnumerable<DomainEvent>>(events => events.Any(e => e is UserActivated))
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Should_Save_User()
    {
        var user = UserMother.Inactive();

        _envStoreMock
            .Setup(s => s.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()))
            .Returns("secret_key");

        _jwtServiceMock.Setup(s => s.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtServiceMock
            .Setup(s => s.Decode(It.IsAny<string>()))
            .Returns(new JWTPayload(new Dictionary<string, string> { { "id", user.Id.Value } }));

        _queryBusMock
            .Setup(q => q.Ask<FindUserByIdQry, User>(It.IsAny<FindUserByIdQry>()))
            .ReturnsAsync(user);

        var cmd = new ActivateUserByTokenCmd("valid_token");
        await _handler.Handle(cmd);

        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
    }
}
