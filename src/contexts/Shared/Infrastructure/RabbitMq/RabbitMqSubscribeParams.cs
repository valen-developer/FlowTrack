namespace FlowTrack.Shared.Infrastructure.RabbitMq
{
    public sealed record RabbitMqSubscribeParams
    {
        public string QueueName { get; init; } = default!;
        public string ExchangeName { get; init; } = default!;
        public string[] RoutingKeys { get; init; } = default!;
    }
}
