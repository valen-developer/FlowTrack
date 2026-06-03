namespace FlowTrack.Shared.Infrastructure;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}
