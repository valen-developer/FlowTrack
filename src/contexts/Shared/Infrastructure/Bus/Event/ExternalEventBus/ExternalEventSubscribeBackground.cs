using FlowTrack.Shared.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlowTrack.Shared.Infrastructure;

public sealed class ExternalEventSubscribeBackground(
    IEnvStore env,
    IJsonToDomainEventMapper jsonToDomainEventMapper,
    RabbitMqSubscriber suscriber,
    ExternalEventSubscriberInformation subscriberInformation,
    IServiceScopeFactory serviceScopeFactory
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = serviceScopeFactory.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<DomainEventDispatcher>();
        var subscribeTasks = subscriberInformation.Subscribers.Select(subscriberInfo =>
            HandleSusbcription(subscriberInfo, dispatcher)
        );
        await Task.WhenAll(subscribeTasks);
    }

    private async Task HandleSusbcription(
        ExternalEventSubscriberInfo subscriberInfo,
        DomainEventDispatcher dispatcher
    )
    {
        var exchangeName = env.Get("EXTERNAL_EVENT_EXCHANGE_NAME") ?? "";
        var queueName = subscriberInfo.QueueName;
        var routingKey = subscriberInfo.EventCode;

        var subcriberParams = new RabbitMqSubscribeParams()
        {
            ExchangeName = exchangeName,
            QueueName = queueName,
            RoutingKeys = [routingKey],
        };

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
                    await dispatcher.DispatchExternal(subscriberInfo, domainEvent);

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
