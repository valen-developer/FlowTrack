namespace FlowTrack.Shared.Infrastructure;

public sealed record RabbitMqSubscribeParams
{
    public string QueueName { get; init; } = default!;
    public string ExchangeName { get; init; } = default!;
    public string[] RoutingKeys { get; init; } = default!;
}
