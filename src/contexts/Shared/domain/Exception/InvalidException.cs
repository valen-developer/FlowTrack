using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.Shared.Domain;

public abstract class InvalidException(string message, string code)
    : DomainException(message, code) { }
