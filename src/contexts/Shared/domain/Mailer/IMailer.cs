namespace FlowTrack.Shared.Domain;

public abstract class IMailer
{
    public abstract Task Send(Mail mail);
}
