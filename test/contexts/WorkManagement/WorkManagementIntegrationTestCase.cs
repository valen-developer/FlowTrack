namespace FlowTrack.WorkManagement.Test;

public abstract class WorkManagementIntegrationTestCase(WorkManagementIntegrationFixture fixture)
    : IClassFixture<WorkManagementIntegrationFixture>
{
    public readonly WorkManagementIntegrationFixture _fixture = fixture;
}
