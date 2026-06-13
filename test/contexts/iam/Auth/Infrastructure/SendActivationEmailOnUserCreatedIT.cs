using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.Iam.Test.Infrastructure;

public class SendActivationEmailOnUserCreatedIT : IamIntegrationTestCase
{
    private EventBus _domainEventBus;
    private Mock<IMailer> _mailerMock = new();

    public SendActivationEmailOnUserCreatedIT(IamIntegrationFixture fixture)
        : base(fixture)
    {
        fixture.serviceCollection.DiscoverServices(["FlowTrack*.dll"]);
        fixture.serviceCollection.AddSingleton(_mailerMock.Object);

        _domainEventBus = fixture.GetService<EventBus>();
    }

    [Fact]
    public async Task Should_Send_Activation_Email_When_User_Is_Signupped()
    {
        var user = UserMother.Random();
        var userSignupedEvent = new UserSignupped(user.Id.Value, user.Email.Value);

        await _domainEventBus.Publish(userSignupedEvent);

        _mailerMock.Verify(m => m.Send(It.IsAny<Mail>()), Times.Once);
    }
}
