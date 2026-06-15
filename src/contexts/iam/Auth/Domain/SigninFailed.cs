namespace FlowTrack.Iam.Auth.Domain
{
    internal class SigninFailed()
        : UnAuthenticatedException(
            "Invalid Signin credentials.",
            "exception.iam.auth.signin_failed"
        ) { }
}
