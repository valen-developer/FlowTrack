using FlowTrack.Shared.Domain;

namespace FlowTrack.Shared.Infrastructure.Mailer;

[Provider(typeof(IMailer))]
public sealed class DummyMailer : IMailer
{
    public override Task Send(Mail mail)
    {
        Console.WriteLine(
            $"[DummyMailer] Sending mail to {mail.To} with subject '{mail.Subject}' and body '{mail.Body}'"
        );
        return Task.CompletedTask;
    }
}
