using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Application;

[Service]
[DomainEventSubscriber(typeof(UserSignupped))]
public sealed class SendActivationMailOnUserCreated(IActivationEmailSender emailSender)
{
    [DomainEventListener]
    public async Task On(UserSignupped @event)
    {
        await emailSender.Send(
            new ActivationEmailSenderParams(new Guid(@event.UserId), @event.Email)
        );
    }
}
