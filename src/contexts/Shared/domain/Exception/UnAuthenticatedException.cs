using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.Shared.Domain;

public abstract class UnAuthenticatedException(string message, string code)
    : DomainException(message, code) { }
