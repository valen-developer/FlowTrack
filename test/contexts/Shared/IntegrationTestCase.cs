using dotenv.net;
using FlowTrack.Shared.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.Shared.Test;

public abstract class IntegrationTestCase
{
    protected readonly ServiceCollection serviceCollection = new();
    private ServiceProvider? serviceProvider;
    private IServiceScope? serviceScope;

    private readonly Mock<IDateTimeProvider> datetimeProviderMock = new();

    public IntegrationTestCase()
    {
        string? envPath = FindEnvFilePath();

        if (envPath is null)
            return;

        DotEnvOptions options = new(envFilePaths: [envPath]);
        DotEnv.Load(options);

        datetimeProviderMock.SetupGet(m => m.Now).Returns(new DateTime(2024, 1, 1));
        serviceCollection.AddSingleton<IDateTimeProvider>(datetimeProviderMock.Object);
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

    private void EnsureProviderBuilt()
    {
        if (serviceProvider is not null)
            return;

        serviceProvider = serviceCollection.BuildServiceProvider();
        serviceScope = serviceProvider.CreateScope();
    }

    private static string? FindEnvFilePath()
    {
        foreach (string startPath in GetCandidateStartPaths())
        {
            string? envPath = FindEnvFrom(startPath);

            if (envPath is not null)
                return envPath;
        }

        return null;
    }

    private static IEnumerable<string> GetCandidateStartPaths()
    {
        yield return Directory.GetCurrentDirectory();
        yield return AppContext.BaseDirectory;
    }

    private static string? FindEnvFrom(string startPath)
    {
        DirectoryInfo? currentDirectory = new(startPath);

        while (currentDirectory is not null)
        {
            string envPath = Path.Combine(currentDirectory.FullName, ".env");

            if (File.Exists(envPath))
                return envPath;

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }
}
