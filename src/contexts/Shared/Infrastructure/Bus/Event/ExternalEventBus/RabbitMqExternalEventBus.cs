using FlowTrack.Shared.Domain;
using RabbitMQ.Client;

namespace FlowTrack.Shared.Infrastructure;

[Provider(typeof(IExternalEventBus), Lifetime.Singleton)]
public class RabbitMqExternalEventBus(RabbitMqConnection rabbitConnection, IEnvStore env)
    : IExternalEventBus
{
    public async Task Publish<T>(T @event)
        where T : DomainEvent
    {
        var channel = await rabbitConnection.CreateChannelAsync();
        var exchangeName = "domain_events";

        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: true);

        var routingKey = @event.Code;

        var body = MapToJsonApiSchema(@event);
        var bodyBytes = System.Text.Encoding.UTF8.GetBytes(body);

        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            body: bodyBytes
        );
    }

    private string MapToJsonApiSchema(DomainEvent @event)
    {
        var id = Guid.NewGuid().ToString();
        var generalType = env.Get("DOMAIN_EVENT_GENERAL_TYPE") ?? "domain_event";
        var type = $"{generalType}.{@event.Code}";

        var json = new
        {
            data = new
            {
                id,
                type,
                ocurred_at = @event.OccurredOn.ToUniversalTime(),
                attributes = @event,
                meta = new { },
            },
        };

        return System.Text.Json.JsonSerializer.Serialize(json);
    }
}
