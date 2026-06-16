namespace FlowTrack.Shared.Infrastructure.Bus.Event.DomainEventBus;

public sealed record DomainEventQueueItem(DomainEvent Event, string CorrelationId);
