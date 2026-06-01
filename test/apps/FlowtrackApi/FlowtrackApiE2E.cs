namespace FlowtrackApi;

public abstract class FlowtrackApiE2E(FlowtrackApiFixture fixture)
    : IClassFixture<FlowtrackApiFixture>
{
    private readonly FlowtrackApiFixture _fixture = fixture;
    protected HttpClient HttpClient => _fixture.HttpClient;
}
