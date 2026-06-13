namespace FlowTrack.Shared.Domain.Exception;

public class UnAuthenticatedException(
    string message = "Unauthenticated",
    string code = "exception.iam.auth.unauthenticated"
) : DomainException(message, code) { }
