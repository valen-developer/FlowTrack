using System.Text;
using System.Text.Json;
using FlowTrack.Shared.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog.Context;

namespace FlowTrack.Shared.Infrastructure.Bus.Event.ExternalEventBus
{
    public sealed class ExternalEventSubscribeBackground(
        IEnvStore env,
        IEnumerable<JsonToDomainEventMapper> jsonToDomainEventMappers,
        RabbitMqSubscriber suscriber,
        ExternalEventSubscriberInformation subscriberInformation,
        IServiceScopeFactory serviceScopeFactory,
        RabbitMqConnection connection,
        IDomainLogger logger
    ) : BackgroundService
    {
        private const int MaxRetries = 3;
        private static readonly int[] RetryDelaysMs = [5000, 15000, 45000];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscribeTasks = subscriberInformation.Subscribers.Select(subscriberInfo =>
                HandleSusbcription(subscriberInfo)
            );
            await Task.WhenAll(subscribeTasks);
        }

        private async Task HandleSusbcription(ExternalEventSubscriberInfo subscriberInfo)
        {
            var exchangeName = env.Get("EXTERNAL_EVENT_EXCHANGE_NAME") ?? "";
            var queueName = subscriberInfo.QueueName;
            var routingKey = subscriberInfo.EventCode;
            var retryExchangeName = $"{queueName}.retry";
            var retryQueueName = $"{queueName}.retry";
            var dlxName = $"{queueName}.dlx";
            var dlqName = $"{queueName}.dlq";

            await DeclareRetryInfrastructure(
                exchangeName,
                retryExchangeName,
                retryQueueName,
                routingKey
            );
            await DeclareDeadLetterInfrastructure(dlxName, dlqName, routingKey);

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
                    var message = Encoding.UTF8.GetString(body);

                    // Extraer CorrelationId del meta del mensaje JSON
                    var correlationId = ExtractCorrelationId(message);
                    CorrelationContext.Set(correlationId);

                    using var _ = LogContext.PushProperty("CorrelationId", correlationId);

                    try
                    {
                        var domainEvent = jsonToDomainEventMappers
                            .Select(m => m.Map(message))
                            .FirstOrDefault(e => e != null);
                        if (domainEvent == null)
                        {
                            logger.Warning(new LogMessage(
                                Action: "Event consumed",
                                Message: $"Unmappable event {routingKey}, sending to DLQ",
                                Attributes: new { RoutingKey = routingKey }
                            ));
                            await PublishToDlq(channel, dlxName, routingKey, args);
                            return;
                        }

                        // Crear un scope por cada mensaje para evitar
                        // conflictos de concurrencia en DbContext
                        using var messageScope = serviceScopeFactory.CreateScope();
                        var dispatcher =
                            messageScope.ServiceProvider.GetRequiredService<DomainEventDispatcher>();

                        await dispatcher.DispatchExternal(subscriberInfo, domainEvent);

                        await channel.BasicAckAsync(args.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(new LogMessage(
                            Action: "Event consumed",
                            Message: $"Error processing event {routingKey} from queue {queueName}",
                            Attributes: new { RoutingKey = routingKey, QueueName = queueName }
                        ), ex);
                        await HandleProcessingFailure(
                            channel,
                            args,
                            retryExchangeName,
                            routingKey,
                            dlxName
                        );
                    }
                }
            );
        }

        private static string? ExtractCorrelationId(string messageJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(messageJson);
                var root = doc.RootElement;
                if (
                    root.TryGetProperty("data", out var data)
                    && data.TryGetProperty("meta", out var meta)
                    && meta.TryGetProperty("correlation_id", out var correlationId)
                )
                {
                    return correlationId.GetString();
                }
            }
            catch
            {
                // Si el JSON no se puede parsear, no hay CorrelationId
            }
            return null;
        }

        private async Task DeclareRetryInfrastructure(
            string mainExchange,
            string retryExchange,
            string retryQueue,
            string routingKey
        )
        {
            await using var channel = await connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(retryExchange, ExchangeType.Direct, durable: true);
            await channel.QueueDeclareAsync(
                queue: retryQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    ["x-dead-letter-exchange"] = mainExchange,
                }
            );
            await channel.QueueBindAsync(retryQueue, retryExchange, routingKey);
        }

        private async Task DeclareDeadLetterInfrastructure(
            string dlxName,
            string dlqName,
            string routingKey
        )
        {
            await using var channel = await connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(dlxName, ExchangeType.Direct, durable: true);
            await channel.QueueDeclareAsync(
                dlqName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );
            await channel.QueueBindAsync(dlqName, dlxName, routingKey);
        }

        private async Task HandleProcessingFailure(
            IChannel channel,
            BasicDeliverEventArgs args,
            string retryExchange,
            string routingKey,
            string dlxName
        )
        {
            var retryCount = GetRetryCount(args);

            if (retryCount < MaxRetries)
            {
                var newRetryCount = retryCount + 1;
                var delayMs = RetryDelaysMs[retryCount];

                var props = new BasicProperties
                {
                    Headers = new Dictionary<string, object?> { ["x-retry-count"] = newRetryCount },
                    Expiration = delayMs.ToString(),
                };

                await channel.BasicPublishAsync(
                    exchange: retryExchange,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: props,
                    body: args.Body.ToArray()
                );

                await channel.BasicAckAsync(args.DeliveryTag, false);

                logger.Warning(new LogMessage(
                    Action: "Event consumed",
                    Message: $"Event {routingKey} failed. Scheduled retry {newRetryCount}/{MaxRetries} with delay {delayMs}ms",
                    Attributes: new
                    {
                        RoutingKey = routingKey,
                        RetryCount = newRetryCount,
                        MaxRetries = MaxRetries,
                        DelayMs = delayMs
                    }
                ));
            }
            else
            {
                logger.Error(new LogMessage(
                    Action: "Event consumed",
                    Message: $"Event {routingKey} sent to DLQ after exhausting {MaxRetries} retries",
                    Attributes: new { RoutingKey = routingKey, MaxRetries = MaxRetries }
                ));

                await PublishToDlq(channel, dlxName, routingKey, args);
            }
        }

        private async Task PublishToDlq(
            IChannel channel,
            string dlxName,
            string routingKey,
            BasicDeliverEventArgs args
        )
        {
            var props = new BasicProperties();
            if (args.BasicProperties.Headers is { } headers)
            {
                props.Headers = new Dictionary<string, object?>(headers);
            }

            await channel.BasicPublishAsync(
                exchange: dlxName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: props,
                body: args.Body.ToArray()
            );

            await channel.BasicAckAsync(args.DeliveryTag, false);
        }

        private static int GetRetryCount(BasicDeliverEventArgs args)
        {
            if (
                args.BasicProperties.Headers is { } headers
                && headers.TryGetValue("x-retry-count", out var value)
                && value is int retryCount
            )
            {
                return retryCount;
            }
            return 0;
        }
    }
}
