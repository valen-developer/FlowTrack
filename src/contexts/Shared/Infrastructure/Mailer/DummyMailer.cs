namespace FlowTrack.Shared.Infrastructure.Mailer
{
    [Provider(typeof(IMailer))]
    public sealed class DummyMailer(IDomainLogger logger) : IMailer
    {
        public override Task Send(Mail mail)
        {
            logger.Info(
                new LogMessage(
                    Action: "Mail sent",
                    Message: $"[DummyMailer] Sending mail to {mail.To} with subject '{mail.Subject}'",
                    Attributes: new
                    {
                        To = mail.To,
                        Subject = mail.Subject,
                        Body = mail.Body,
                        BodyLength = mail.Body?.Length ?? 0,
                    }
                )
            );
            return Task.CompletedTask;
        }
    }
}
