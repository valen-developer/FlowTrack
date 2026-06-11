using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Application;

[Service]
[DomainEventSubscriber(typeof(UserCreated))]
public sealed class SendActivationMailOnUserCreated(IActivationEmailSender emailSender)
{
    [DomainEventListener]
    public async Task On(UserCreated @event)
    {
        var isActive = @event.IsActive;
        if (isActive)
        {
            return;
        }

        await emailSender.Send(
            new ActivationEmailSenderParams(new Guid(@event.UserId), @event.Email)
        );
    }
}
