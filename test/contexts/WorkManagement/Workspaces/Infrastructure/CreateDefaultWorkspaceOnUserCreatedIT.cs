using FlowTrack.Shared.Domain.Bus.Event;
using FlowTrack.Shared.Domain.Iam.Users;
using FlowTrack.WorkManagement.Test;
using FlowTrack.WorkManagement.Workspaces.Domain;
using FlowTrack.WorkManagement.Workspaces.Infrastructure.Persistence;

namespace FlowTrack.WorkManagement.Workspaces.Test.Infrastructure
{
    public class CreateDefaultWorkspaceOnUserCreatedIT : WorkManagementIntegrationTestCase
    {
        private readonly EventBus _eventBus;

        public CreateDefaultWorkspaceOnUserCreatedIT(WorkManagementIntegrationFixture fixture)
            : base(fixture)
        {
            _eventBus = fixture.GetService<EventBus>();
        }

        [Fact]
        public async Task Should_Create_Default_Workspace_When_User_Is_Created()
        {
            var userCreatedEvent = new UserCreated(
                UserId: Guid.NewGuid().ToString(),
                Email: "email@email.com",
                IsActive: true
            );

            await _fixture.EnsureServicesAsync();
            await _eventBus.Publish(userCreatedEvent);

            List<WorkspaceEntity> workspaces = [];
            await WaitForAsync(async () =>
            {
                workspaces = await _fixture.ExecuteQueryAsync<WorkspaceEntity>(
                    $"SELECT * FROM workspaces WHERE \"OwnerId\" = '{userCreatedEvent.UserId}'"
                );

                return workspaces.Count == 1;
            });

            Assert.Single(workspaces);

            var defaultWorkspace = workspaces.FirstOrDefault();
            Assert.Equal(Workspace.DefaultName, defaultWorkspace?.Name);
        }
    }
}
