using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Auth.Application;

[Service]
[DomainEventSubscriber(typeof(UserSignupped))]
public sealed class SendActivationMailOnUserSignupped(IActivationEmailSender emailSender)
{
    [DomainEventListener]
    public async Task On(UserSignupped @event)
    {
        await emailSender.Send(new ActivationEmailSenderParams(@event.UserId, @event.Email));
    }
}
