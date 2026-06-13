using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.Iam.Test.Auth.Application;

public class SignupCmdHandlerTests
{
    private readonly SignupCmdHandler _handler;
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IBcrypt> _bcrypt = new();
    private readonly Mock<IExternalEventBus> _externalEventBus = new();

    public SignupCmdHandlerTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton(_externalEventBus.Object);
        services.AddSingleton(Mock.Of<IDomainEventBus>());

        services.AddSingleton(_userRepositoryMock.Object);
        services.AddSingleton(_bcrypt.Object);

        services.AddScoped<EventBus>();
        services.AddScoped<SignupCmdHandler>();

        _handler = services.BuildServiceProvider().GetRequiredService<SignupCmdHandler>();
    }

    [Fact]
    public async Task Should_Save_An_Inactive_User()
    {
        User user = UserMother.Inactive();
        string expectedHashedPassword = "hashed-password";
        User? expectedUser = null;

        _bcrypt.Setup(x => x.Hash(It.IsAny<string>())).Returns(expectedHashedPassword);

        _userRepositoryMock
            .Setup(x => x.Create(It.IsAny<User>()))
            .Callback<User>(u => expectedUser = u);

        var cmd = new SignupCmd(user.Id.Value, user.Email.Value, user.Password.Value);
        await _handler.Handle(cmd);

        _userRepositoryMock.Verify(x => x.Create(It.IsAny<User>()), Times.Once);
        Assert.Equal(user.Id.Value, expectedUser!.Id.Value);
        Assert.Equal(user.Email.Value, expectedUser.Email.Value);
        Assert.Equal(expectedHashedPassword, expectedUser.Password.Value);
        Assert.False(expectedUser!.IsActive);
    }

    [Fact]
    public async Task Should_Emit_User_Created_Event()
    {
        DomainEvent? capturedEvent = null;

        _externalEventBus
            .Setup(x => x.Publish(It.IsAny<DomainEvent>()))
            .Callback<DomainEvent>(@event => capturedEvent = @event)
            .Returns(Task.CompletedTask);

        User user = UserMother.Inactive();
        SignupCmd cmd = new(
            Id: user.Id.Value,
            Email: user.Email.Value,
            Password: user.Password.Value
        );

        UserCreated expectedEvent = new(
            UserId: user.Id.Value,
            Email: user.Email.Value,
            IsActive: user.IsActive
        );

        await _handler.Handle(cmd);

        UserCreated userCreatedEvent = (UserCreated)capturedEvent!;
        Assert.NotNull(userCreatedEvent);
        Assert.IsType<UserCreated>(userCreatedEvent);
        Assert.Equal(expectedEvent.UserId, userCreatedEvent.UserId);
        Assert.Equal(expectedEvent.Email, userCreatedEvent.Email);
    }

    [Theory]
    [InlineData("Short12")]
    [InlineData("Nonumber")]
    [InlineData("nouppercase1")]
    [InlineData("NOLOWERCASE1")]
    public async Task Should_Throw_Invalid_Password_Exception(string invalidPassword)
    {
        var cmd = new SignupCmd(Guid.NewGuid().ToString(), "valid@email.com", invalidPassword);

        await Assert.ThrowsAsync<InvalidPassword>(() => _handler.Handle(cmd));
    }

    [Fact]
    public async Task Should_Throw_Invalid_Email_Exception()
    {
        var cmd = new SignupCmd(Guid.NewGuid().ToString(), "invalid-email", "ValidPass123");

        await Assert.ThrowsAsync<InvalidEmail>(() => _handler.Handle(cmd));
    }

    [Fact]
    public async Task Should_Not_Throw_For_Already_Existing_Email()
    {
        User user = UserMother.Inactive();

        _userRepositoryMock.Setup(x => x.FindByEmail(It.IsAny<string>())).ReturnsAsync(user);

        var cmd = new SignupCmd(Guid.NewGuid().ToString(), user.Email.Value, "ValidPass123");

        var exception = await Record.ExceptionAsync(() => _handler.Handle(cmd));
        Assert.Null(exception);

        _userRepositoryMock.Verify(x => x.Create(It.IsAny<User>()), Times.Never);
    }
}
