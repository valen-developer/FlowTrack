using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Domain;

public class SigninFailed()
    : UnAuthenticatedException(
        "Invalid Signin credentials.",
        "exception.iam.auth.signin_failed"
    ) { }
