namespace FlowTrack.Iam.Auth.Application
{
    internal record SignupCmd(string Id, string Email, string Password) : ICommand;
}
