namespace FlowTrack.Shared.Domain.Exception
{
    public abstract class InvalidException(string message, string code)
        : DomainException(message, code) { }
}
