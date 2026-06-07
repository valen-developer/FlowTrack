namespace FlowTrack.Iam.Application;

public interface IActivationEmailSender
{
    Task Send(ActivationEmailSenderParams @params);
}
