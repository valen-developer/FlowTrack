namespace FlowTrack.Iam.Auth.Application;

public interface IActivationEmailSender
{
    Task Send(ActivationEmailSenderParams @params);
}
