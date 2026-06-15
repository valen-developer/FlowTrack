namespace FlowTrack.Shared.Infrastructure.RabbitMq
{
    public sealed record RabbitMqConnectionParams
    {
        public string Host { get; init; } = default!;
        public int Port { get; init; }
        public string UserName { get; init; } = default!;
        public string Password { get; init; } = default!;
        public string ClientProvidedName { get; init; } = default!;
    }
}
