using NativeException = System.Exception;

namespace FlowTrack.Shared.Domain.Exception;

public abstract class DomainException(string message, string code) : NativeException(message)
{
    public string Code { get; } = code;
}
