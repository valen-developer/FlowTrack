using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.Shared.Domain;

public class UnAuthenticatedException(
    string message = "Unauthenticated",
    string code = "exception.iam.auth.unauthenticated"
) : DomainException(message, code) { }
