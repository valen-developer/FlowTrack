using Microsoft.Extensions.Logging;

namespace FlowTrack.Shared.Infrastructure.Mailer
{
    [Provider(typeof(IMailer))]
    public sealed class DummyMailer(ILogger<DummyMailer> logger) : IMailer
    {
        public override Task Send(Mail mail)
        {
            logger.LogInformation(
                "[DummyMailer] Sending mail to {To} with subject '{Subject}' and body '{Body}'",
                mail.To,
                mail.Subject,
                mail.Body
            );
            return Task.CompletedTask;
        }
    }
}
