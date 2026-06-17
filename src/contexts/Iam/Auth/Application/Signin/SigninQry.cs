namespace FlowTrack.Iam.Auth.Application
{
    internal record SigninQry(string Email, string Password) : IQuery<SigninSuccess>;
}
