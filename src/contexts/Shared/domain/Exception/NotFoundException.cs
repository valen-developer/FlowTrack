namespace FlowTrack.Shared.Domain.Exception;

public abstract class NotFoundException(string message, string code)
    : DomainException(message, code) { }
