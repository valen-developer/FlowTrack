namespace FlowTrack.Shared.Domain;

public interface IExternalEventBus
{
    Task Publish<T>(T @event)
        where T : DomainEvent;
}
