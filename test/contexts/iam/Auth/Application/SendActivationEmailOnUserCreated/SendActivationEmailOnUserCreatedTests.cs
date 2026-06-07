using FlowTrack.Iam.Application;
using FlowTrack.Iam.Domain;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.Iam.Test;

public class SendActivationEmailOnUserCreatedTests
{
    private readonly SendActivationMailOnUserCreated _sendActivationEmailOnUserCreated;
    private readonly Mock<IActivationEmailSender> _activationEmailSenderMock = new();

    public SendActivationEmailOnUserCreatedTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_activationEmailSenderMock.Object);
        services.AddScoped<SendActivationMailOnUserCreated>();

        _sendActivationEmailOnUserCreated = services
            .BuildServiceProvider()
            .GetRequiredService<SendActivationMailOnUserCreated>();
    }

    [Fact]
    public async Task Should_Call_ActivationEmailSender()
    {
        var user = UserMother.Random();
        var userCreatedEvent = new UserCreated(UserId: user.Id, Email: user.Email, IsActive: false);

        var expectedParams = new ActivationEmailSenderParams(
            UserId: userCreatedEvent.UserId,
            Email: userCreatedEvent.Email
        );

        await _sendActivationEmailOnUserCreated.On(userCreatedEvent);

        _activationEmailSenderMock.Verify(
            sender => sender.Send(expectedParams),
            Times.Once,
            "Expected ActivationEmailSender.Send to be called once with the correct parameters."
        );
    }

    [Fact]
    public async Task Should_Not_Call_ActivationEmailSender_When_User_Is_Active()
    {
        var user = UserMother.Random();
        var userCreatedEvent = new UserCreated(UserId: user.Id, Email: user.Email, IsActive: true);

        await _sendActivationEmailOnUserCreated.On(userCreatedEvent);

        _activationEmailSenderMock.Verify(
            sender => sender.Send(It.IsAny<ActivationEmailSenderParams>()),
            Times.Never,
            "Expected ActivationEmailSender.Send to not be called when the user is already active."
        );
    }
}
