namespace FlowTrack.Shared.Domain
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}
