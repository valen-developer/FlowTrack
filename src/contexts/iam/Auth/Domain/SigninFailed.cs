namespace FlowTrack.Iam.Auth.Domain;

public class SigninFailed : Exception
{
    public readonly string Code = "exception.iam.auth.signin_failed";

    public SigninFailed()
        : base("Invalid Signin credentials.") { }
}
