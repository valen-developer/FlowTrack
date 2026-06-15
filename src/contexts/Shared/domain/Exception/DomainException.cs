namespace FlowTrack.Shared.Domain.Exception
{
    public abstract class DomainException(string message, string code) : System.Exception(message)
    {
        public string Code { get; } = code;
    }
}
