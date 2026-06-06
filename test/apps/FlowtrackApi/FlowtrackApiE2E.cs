namespace FlowtrackApi.Test;

[Collection(nameof(FlowtrackApiCollection))]
public abstract class FlowtrackApiE2E(FlowtrackApiFixture fixture)
{
    private readonly FlowtrackApiFixture _fixture = fixture;
    protected HttpClient HttpClient => _fixture.HttpClient;
    protected IServiceProvider Services => _fixture.Services;
}
