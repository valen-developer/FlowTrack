namespace FlowTrack.Shared.Domain.Bus.Event;

public interface IJsonToDomainEventMapper
{
    DomainEvent? Map(string json);
}
