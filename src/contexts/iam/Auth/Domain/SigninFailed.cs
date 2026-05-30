using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.Iam.Auth.Domain;

public class SigninFailed()
    : DomainException("Invalid Signin credentials.", "exception.iam.auth.signin_failed") { }
