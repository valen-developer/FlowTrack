namespace FlowTrack.Shared.Domain;

public abstract class InternalException(string message, string code)
    : DomainException(message, code) { }
