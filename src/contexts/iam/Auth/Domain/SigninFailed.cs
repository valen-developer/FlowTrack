using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.Iam.Domain;

public class SigninFailed()
    : DomainException("Invalid Signin credentials.", "exception.iam.auth.signin_failed") { }
