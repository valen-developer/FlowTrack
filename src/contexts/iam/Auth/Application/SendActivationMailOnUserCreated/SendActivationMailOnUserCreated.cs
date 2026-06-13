using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Auth.Application;

[Service]
[DomainEventSubscriber(typeof(UserCreated))]
public sealed class SendActivationMailOnUserCreated(IActivationEmailSender emailSender)
{
    [DomainEventListener]
    public async Task On(UserCreated @event)
    {
        await emailSender.Send(new ActivationEmailSenderParams(@event.UserId, @event.Email));
    }
}
