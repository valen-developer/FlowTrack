using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlowTrack.Shared.Infrastructure.Bus.Event;

public sealed class DomainEventSubscribeBackground(
    IServiceScopeFactory serviceScopeFactory,
    InMemoryDomainEventQueue queue
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var domainEvents = queue.DequeueAll();
            foreach (var @event in domainEvents)
            {
                using var scope = serviceScopeFactory.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<DomainEventDispatcher>();
                await dispatcher.DispatchAsync(@event);
            }

            await Task.Delay(500, stoppingToken);
        }
    }
}
