using FlowTrack.Shared.Infrastructure;

namespace FlowTrack.WorkManagement.Test
{
    public abstract class WorkManagementIntegrationTestCase
        : IClassFixture<WorkManagementIntegrationFixture>
    {
        public readonly WorkManagementIntegrationFixture _fixture;

        public WorkManagementIntegrationTestCase(WorkManagementIntegrationFixture fixture)
        {
            _fixture = fixture;

            fixture.serviceCollection.DiscoverServices([
                "FlowTrack.WorkManagement*.dll",
                "FlowTrack.Shared*.dll",
            ]);
        }
    }
}
