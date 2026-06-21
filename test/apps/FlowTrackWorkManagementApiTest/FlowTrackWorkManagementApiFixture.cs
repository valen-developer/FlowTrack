using Microsoft.AspNetCore.Mvc.Testing;

namespace FlowTrackWorkManagementApiTest;

public class FlowTrackWorkManagementApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public HttpClient HttpClient { get; private set; } = null!;

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    public async Task InitializeAsync() { }
}
