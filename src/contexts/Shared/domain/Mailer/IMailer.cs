namespace FlowTrack.Shared.Domain.Mailer
{
    public abstract class IMailer
    {
        public abstract Task Send(Mail mail);
    }
}
