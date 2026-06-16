namespace FlowTrack.WorkManagement.Test;

[Collection("WorkManagementIntegration")]
public abstract class WorkManagementIntegrationTestCase
{
    public readonly WorkManagementIntegrationFixture _fixture;

    public WorkManagementIntegrationTestCase(WorkManagementIntegrationFixture fixture)
    {
        _fixture = fixture;
    }
}
