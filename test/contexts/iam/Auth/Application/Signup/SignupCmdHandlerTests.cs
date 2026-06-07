using FlowTrack.Iam.Application;
using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.Iam.Test;

public class SignupCmdHandlerTests
{
    private readonly SignupCmdHandler _handler;
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IBcrypt> _bcrypt = new();
    private readonly Mock<IDomainEventBus> _eventBus = new();

    public SignupCmdHandlerTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton(_userRepositoryMock.Object);
        services.AddSingleton(_eventBus.Object);
        services.AddSingleton(_bcrypt.Object);
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

        var cmd = new SignupCmd(user.Id.ToString(), user.Email, user.Password);
        await _handler.Handle(cmd);

        _userRepositoryMock.Verify(x => x.Create(It.IsAny<User>()), Times.Once);
        Assert.Equal(user.Id, expectedUser!.Id);
        Assert.Equal(user.Email, expectedUser.Email);
        Assert.Equal(expectedHashedPassword, expectedUser.Password);
        Assert.False(expectedUser!.IsActive);
    }

    [Fact]
    public async Task Should_Emit_User_Created_Event()
    {
        IEnumerable<DomainEvent>? capturedEvents = null;

        _eventBus
            .Setup(x => x.Publish(It.IsAny<List<DomainEvent>>()))
            .Callback<IEnumerable<DomainEvent>>(events => capturedEvents = events)
            .Returns(Task.CompletedTask);

        User user = UserMother.Inactive();
        SignupCmd cmd = new(Id: user.Id.ToString(), Email: user.Email, Password: user.Password);

        UserCreated expectedEvent = new(
            UserId: user.Id,
            Email: user.Email,
            IsActive: user.IsActive
        );

        await _handler.Handle(cmd);

        _eventBus.Verify(x => x.Publish(It.IsAny<List<DomainEvent>>()), Times.Once);

        Assert.NotNull(capturedEvents);
        Assert.Single(capturedEvents);

        UserCreated userCreatedEvent = (UserCreated)capturedEvents!.First();
        Assert.NotNull(userCreatedEvent);
        Assert.IsType<UserCreated>(userCreatedEvent);
        Assert.Equal(expectedEvent.UserId, userCreatedEvent.UserId);
        Assert.Equal(expectedEvent.Email, userCreatedEvent.Email);
        Assert.Equal(expectedEvent.IsActive, userCreatedEvent.IsActive);
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

        var cmd = new SignupCmd(Guid.NewGuid().ToString(), user.Email, "ValidPass123");

        var exception = await Record.ExceptionAsync(() => _handler.Handle(cmd));
        Assert.Null(exception);

        _userRepositoryMock.Verify(x => x.Create(It.IsAny<User>()), Times.Never);
    }
}
