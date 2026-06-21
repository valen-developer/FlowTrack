using FlowTrack.Shared.Domain.Contexts;
using FlowTrack.Shared.Infrastructure;
using FlowTrack.Shared.Infrastructure.Bus.Event;
using FlowTrack.Shared.Infrastructure.Bus.Event.ExternalEventBus;
using FlowTrack.Shared.Infrastructure.Transactions;
using FlowTrack.Shared.Test;
using FlowTrack.WorkManagement.Shared.Domain;
using FlowTrack.WorkManagement.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlowTrack.WorkManagement.Test
{
    public class WorkManagementIntegrationFixture : IntegrationTestCase, IAsyncLifetime
    {
        private bool _hostedServicesStarted;
        private readonly WorkManagementTestContainers _containers = new();

        public WorkManagementTestContainers Containers => _containers;

        public WorkManagementIntegrationFixture()
            : base(env: [])
        {
            serviceCollection.AddKeyedScoped(
                "WORK_MANAGEMENT",
                (sp, _) =>
                {
                    var dbContext = sp.GetRequiredService<WorkManagementDbContext>();
                    var transaction = new EfCoreTransaction(dbContext);
                    return new Context(transaction);
                }
            );
        }

        public async Task DisposeAsync()
        {
            if (_hostedServicesStarted)
            {
                var hostedServices = serviceProvider!.GetServices<IHostedService>().Reverse();
                foreach (var hs in hostedServices)
                    await hs.StopAsync(CancellationToken.None);
            }

            if (serviceProvider is not null)
            {
                serviceScope?.Dispose();
                await serviceProvider.DisposeAsync();
            }

            await _containers.DisposeAsync();
        }

        public async Task InitializeAsync()
        {
            await _containers.StartAsync();

            Environment.SetEnvironmentVariable(
                WorkManagementEnvironmentKeysEnum.WORK_MANAGEMENT_ELASTICSEARCH_URL.ToString(),
                _containers.ElasticsearchUrl
            );

            serviceCollection.AddDbContext<WorkManagementDbContext>(options =>
                options.UseNpgsql(_containers.PostgresConnectionString)
            );

            serviceCollection.AddLogging(builder => builder.AddConsole());

            Environment.SetEnvironmentVariable("RABBITMQ_HOST", _containers.RabbitMqHost);
            Environment.SetEnvironmentVariable(
                "RABBITMQ_PORT",
                _containers.RabbitMqPort.ToString()
            );
            Environment.SetEnvironmentVariable("RABBITMQ_USERNAME", "guest");
            Environment.SetEnvironmentVariable("RABBITMQ_PASSWORD", "guest");
            Environment.SetEnvironmentVariable("EXTERNAL_EVENT_EXCHANGE_NAME", "domain_events");

            serviceCollection.AddHostedService<ExternalEventSubscribeBackground>();
            serviceCollection.AddHostedService<DomainEventSubscribeBackground>();

            serviceCollection.DiscoverServices([
                "FlowTrack.WorkManagement*.dll",
                "FlowTrack.Shared*.dll",
            ]);
            using var provider = serviceCollection.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WorkManagementDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        public async Task EnsureServicesAsync()
        {
            if (serviceProvider is null)
            {
                serviceProvider = serviceCollection.BuildServiceProvider();
                serviceScope = serviceProvider.CreateScope();
            }

            if (!_hostedServicesStarted)
            {
                _hostedServicesStarted = true;
                var hostedServices = serviceProvider.GetServices<IHostedService>();
                foreach (var hs in hostedServices)
                    await hs.StartAsync(CancellationToken.None);
                await Task.Delay(100);
            }
        }
    }
}
