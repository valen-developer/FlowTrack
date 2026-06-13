using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FlowTrack.Shared.Infrastructure.RabbitMq;

[Service(Lifetime.Singleton)]
public class RabbitMqSubscriber(RabbitMqConnection connection)
{
    public async Task SubscribeAsync(
        RabbitMqSubscribeParams subscribeParams,
        Func<IChannel, BasicDeliverEventArgs, Task> onMessageReceived
    )
    {
        var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: subscribeParams.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null
        );

        await channel.QueueDeclareAsync(
            queue: subscribeParams.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        foreach (var routingKey in subscribeParams.RoutingKeys)
        {
            await channel.QueueBindAsync(
                queue: subscribeParams.QueueName,
                exchange: subscribeParams.ExchangeName,
                routingKey: routingKey
            );
        }

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, args) =>
        {
            await onMessageReceived(channel, args);
        };

        await channel.BasicConsumeAsync(
            queue: subscribeParams.QueueName,
            autoAck: false,
            consumer: consumer
        );
    }
}
