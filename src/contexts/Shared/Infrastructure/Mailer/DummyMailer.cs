using FlowTrack.Shared.Domain;

namespace FlowTrack.Shared.Infrastructure;

[Provider(typeof(IMailer))]
public sealed class DummyMailer : IMailer
{
    public override Task Send(Mail mail)
    {
        // No-op
        return Task.CompletedTask;
    }
}
