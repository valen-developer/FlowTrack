using FlowTrack.Shared.Domain;
using RabbitMQ.Client;

namespace FlowTrack.Shared.Infrastructure;

[Provider(typeof(IExternalEventBus))]
public class RabbitMqExternalEventBus(RabbitMqConnection rabbitConnection, IEnvStore env)
    : IExternalEventBus
{
    public async Task Publish<T>(T @event)
        where T : DomainEvent
    {
        var channel = await rabbitConnection.CreateChannelAsync();
        var exchangeName = env.Get("EXTERNAL_EVENT_EXCHANGE_NAME") ?? "domain_events";

        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: true);

        var routingKey = @event.GetCode();

        var body = MapToJsonApiSchema(@event);
        var bodyBytes = System.Text.Encoding.UTF8.GetBytes(body);

        var props = new RabbitMQ.Client.BasicProperties
        {
            Headers = new Dictionary<string, object?> { ["x-retry-count"] = 0 },
        };

        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: props,
            body: bodyBytes
        );
    }

    private string MapToJsonApiSchema(DomainEvent @event)
    {
        var id = Guid.NewGuid().ToString();
        var type = @event.GetCode();

        var json = new
        {
            data = new
            {
                id,
                type,
                code = @event.GetCode(),
                ocurred_at = @event.OccurredOn.ToUniversalTime(),
                attributes = (object)@event,
                meta = new { },
            },
        };

        return System.Text.Json.JsonSerializer.Serialize(json);
    }
}
