using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.Shared.Test
{
    public abstract class IntegrationTestCase
    {
        public readonly ServiceCollection serviceCollection = new();
        protected ServiceProvider? serviceProvider;
        protected IServiceScope? serviceScope;

        private readonly Mock<IDateTimeProvider> datetimeProviderMock = new();

        public IntegrationTestCase(Dictionary<string, string>? env = null)
        {
            datetimeProviderMock.SetupGet(m => m.Now).Returns(DateTime.UtcNow);
            serviceCollection.AddSingleton<IDateTimeProvider>(datetimeProviderMock.Object);

            LoadEnv(env);
        }

        private static void LoadEnv(Dictionary<string, string>? env)
        {
            if (env is null)
                return;

            foreach (var kvp in env)
            {
                Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
            }
        }

        public T GetService<T>()
            where T : class
        {
            EnsureProviderBuilt();
            return serviceScope!.ServiceProvider.GetRequiredService<T>();
        }

        public void AddScoped<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            serviceCollection.AddScoped<TService, TImplementation>();
        }

        public void AddTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            serviceCollection.AddTransient<TService, TImplementation>();
        }

        public async Task WaitForMockAsync<T>(
            Mock<T> mock,
            System.Linq.Expressions.Expression<Action<T>> expression,
            int timeoutMs = 5000,
            int pollIntervalMs = 100
        )
            where T : class
        {
            var start = DateTime.UtcNow;
            while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
            {
                try
                {
                    mock.Verify(expression, Moq.Times.Once);
                    return;
                }
                catch (MockException) { }
                await Task.Delay(pollIntervalMs);
            }
            mock.Verify(expression, Moq.Times.Once);
        }

        private void EnsureProviderBuilt()
        {
            if (serviceProvider is not null)
                return;

            serviceProvider = serviceCollection.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();
        }
    }
}
