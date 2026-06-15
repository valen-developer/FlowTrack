namespace FlowTrack.Shared.Domain
{
    public abstract class AggregatedRoot
    {
        private readonly List<DomainEvent> _domainEvents = [];

        public List<DomainEvent> PullDomainEvents()
        {
            var events = _domainEvents.ToList();
            _domainEvents.Clear();
            return events;
        }

        public void Record(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
    }
}
