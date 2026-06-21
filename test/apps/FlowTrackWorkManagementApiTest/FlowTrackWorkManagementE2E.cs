namespace FlowTrackWorkManagementApiTest;

public abstract class FlowTrackWorkManagementE2E(FlowTrackWorkManagementApiFixture fixture)
{
    protected readonly FlowTrackWorkManagementApiFixture _fixture = fixture;
    protected HttpClient HttpClient => _fixture.HttpClient;
    protected IServiceProvider Services => _fixture.Services;
}
