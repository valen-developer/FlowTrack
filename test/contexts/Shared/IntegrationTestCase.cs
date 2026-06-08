using dotenv.net;
using FlowTrack.Shared.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.Shared.Test;

public abstract class IntegrationTestCase
{
    public readonly ServiceCollection serviceCollection = new();
    private ServiceProvider? serviceProvider;
    private IServiceScope? serviceScope;

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

    private void EnsureProviderBuilt()
    {
        if (serviceProvider is not null)
            return;

        serviceProvider = serviceCollection.BuildServiceProvider();
        serviceScope = serviceProvider.CreateScope();
    }
}
