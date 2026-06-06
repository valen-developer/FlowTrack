namespace FlowTrack.Iam.Test;

public abstract class IamIntegrationTestCase(IamIntegrationFixture fixture)
    : IClassFixture<IamIntegrationFixture>
{
    public readonly IamIntegrationFixture _fixture = fixture;
}
