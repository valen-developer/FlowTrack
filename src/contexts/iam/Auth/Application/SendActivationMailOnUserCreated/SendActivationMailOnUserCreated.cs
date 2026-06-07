using FlowTrack.Iam.Domain;

namespace FlowTrack.Iam.Application;

public sealed class SendActivationMailOnUserCreated(IActivationEmailSender emailSender)
{
    public async Task On(UserCreated @event)
    {
        await emailSender.Send(new ActivationEmailSenderParams(@event.UserId, @event.Email));
    }
}
