using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain;
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
    public async Task Should_Send_Activation_Email_When_User_Is_Created()
    {
        var user = UserMother.Random();
        var userCreatedEvent = new UserCreated(user.Id, user.Email, false);

        await _domainEventBus.Publish(userCreatedEvent);

        _mailerMock.Verify(m => m.Send(It.IsAny<Mail>()), Times.Once);
    }
}
