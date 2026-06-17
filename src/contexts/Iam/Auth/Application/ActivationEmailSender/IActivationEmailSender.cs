namespace FlowTrack.Iam.Auth.Application
{
    internal interface IActivationEmailSender
    {
        Task Send(ActivationEmailSenderParams @params);
    }
}
