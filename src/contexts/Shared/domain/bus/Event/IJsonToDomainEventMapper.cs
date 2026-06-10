namespace FlowTrack.Shared.Domain;

public interface IJsonToDomainEventMapper
{
    DomainEvent? Map(string json);
}
