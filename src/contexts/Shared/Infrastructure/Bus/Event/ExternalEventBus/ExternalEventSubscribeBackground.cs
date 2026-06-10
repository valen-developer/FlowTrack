using FlowTrack.Shared.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlowTrack.Shared.Infrastructure;

public sealed class ExternalEventSubscribeBackground(
    IEnvStore env,
    IJsonToDomainEventMapper jsonToDomainEventMapper,
    RabbitMqSubscriber suscriber,
    IServiceScopeFactory serviceScopeFactory
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var routingKeys = (env.Get("EXTERNAL_EVENT_ROUTING_KEYS") ?? "").Split(",") ?? [];

        var subcriberParams = new RabbitMqSubscribeParams()
        {
            ExchangeName = env.Get("EXTERNAL_EVENT_EXCHANGE_NAME") ?? "",
            QueueName = env.Get("EXTERNAL_EVENT_QUEUE_NAME") ?? "",
            RoutingKeys = routingKeys,
        };

        var scope = serviceScopeFactory.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<DomainEventDispatcher>();

        await suscriber.SubscribeAsync(
            subcriberParams,
            async (channel, args) =>
            {
                var body = args.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);

                try
                {
                    var domainEvent = jsonToDomainEventMapper.Map(message);
                    if (domainEvent == null)
                    {
                        await channel.BasicAckAsync(args.DeliveryTag, false);
                        return;
                    }
                    await dispatcher.DispatchAsync(domainEvent);

                    await channel.BasicAckAsync(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");

                    await channel.BasicNackAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: true
                    );
                }
            }
        );
    }
}
