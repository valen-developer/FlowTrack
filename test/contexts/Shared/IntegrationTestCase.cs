using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.Shared.Test;

public abstract class IntegrationTestCase
{
    private readonly ServiceCollection serviceCollection = new();
    private readonly ServiceProvider serviceProvider;
    private readonly IServiceScope serviceScope;

    public IntegrationTestCase()
    {
        serviceProvider = serviceCollection.BuildServiceProvider();
        serviceScope = serviceProvider.CreateScope();
    }

    public T GetService<T>()
        where T : class
    {
        return serviceScope.ServiceProvider.GetRequiredService<T>();
    }

    public void AddScoped<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        serviceCollection.AddScoped<TService, TImplementation>();
    }
}
