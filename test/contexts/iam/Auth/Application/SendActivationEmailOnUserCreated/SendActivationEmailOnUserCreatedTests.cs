using FlowTrack.Shared.Domain.Iam.Users;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.Iam.Test.Auth.Application;

public class SendActivationEmailOnUserSignuppedTests
{
    private readonly SendActivationMailOnUserCreated _sendActivationEmailOnUserCreated;
    private readonly Mock<IActivationEmailSender> _activationEmailSenderMock = new();

    public SendActivationEmailOnUserSignuppedTests()
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
        var userCreatedEvent = new UserCreated(
            UserId: user.Id.Value,
            Email: user.Email.Value,
            IsActive: user.IsActive
        );

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
}
