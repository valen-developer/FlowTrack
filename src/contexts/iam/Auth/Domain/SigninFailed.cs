using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.Iam.Domain;

public class SigninFailed()
    : UnAuthenticatedException(
        "Invalid Signin credentials.",
        "exception.iam.auth.signin_failed"
    ) { }
