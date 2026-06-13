namespace FlowTrack.Shared.Domain.Exception;

public abstract class InternalException(string message, string code)
    : DomainException(message, code) { }
