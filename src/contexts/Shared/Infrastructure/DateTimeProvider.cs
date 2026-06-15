using FlowTrack.Shared.Domain;

namespace FlowTrack.Shared.Infrastructure
{
    [Provider(typeof(IDateTimeProvider))]
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
