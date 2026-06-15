namespace FlowTrack.Shared.Domain.Bus.Event
{
    public interface IExternalEventBus
    {
        Task Publish<T>(T @event)
            where T : DomainEvent;
    }
}
