using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.Shared.Domain;

public abstract class NotFoundException(string message, string code)
    : DomainException(message, code) { }
