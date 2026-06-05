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

    public SignupCmdHandlerTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton(_userRepositoryMock.Object);
        services.AddScoped<SignupCmdHandler>();

        _handler = services.BuildServiceProvider().GetRequiredService<SignupCmdHandler>();
    }

    [Fact]
    public async Task Should_Save_An_Inactive_User()
    {
        User user = UserMother.Inactive();
        User? expectedUser = null;

        _userRepositoryMock
            .Setup(x => x.Create(It.IsAny<User>()))
            .Callback<User>(u => expectedUser = u);

        var cmd = new SignupCmd(user.Id.ToString(), user.Email, user.Password);
        await _handler.Handle(cmd);

        _userRepositoryMock.Verify(x => x.Create(It.IsAny<User>()), Times.Once);
        Assert.Equivalent(expectedUser, user);
        Assert.False(expectedUser!.IsActive);
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
