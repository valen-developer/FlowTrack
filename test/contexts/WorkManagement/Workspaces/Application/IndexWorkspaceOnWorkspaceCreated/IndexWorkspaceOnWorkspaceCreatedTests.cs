using FlowTrack.Shared.Domain.FilterCriterias;
using FlowTrack.WorkManagement.Workspaces.Application;
using FlowTrack.WorkManagement.Workspaces.Domain;
using FlowTrack.WorkManagement.Workspaces.Test;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.WorkManagement.Workspaces.Test.Application
{
    public class IndexWorkspaceOnWorkspaceCreatedTests
    {
        private readonly Mock<IWorkspaceRepository> _repositoryMock = new();
        private readonly Mock<IWorkspaceSearchEngine> _searchEngineMock = new();
        private readonly IndexWorkspaceOnWorkspaceCreated _handler;

        public IndexWorkspaceOnWorkspaceCreatedTests()
        {
            var services = new ServiceCollection();

            services.AddSingleton(_searchEngineMock.Object);
            services.AddSingleton(_repositoryMock.Object);

            services.AddScoped<IndexWorkspaceOnWorkspaceCreated>();

            var serviceProvider = services.BuildServiceProvider();
            _handler = serviceProvider.GetRequiredService<IndexWorkspaceOnWorkspaceCreated>();
        }

        [Fact]
        public async Task Should_Index_In_Search_Engine()
        {
            var workspace = WorkspaceMother.WithId(Guid.NewGuid().ToString());

            var @event = new WorkspaceCreated(
                Id: Guid.NewGuid().ToString(),
                OwnerId: Guid.NewGuid().ToString(),
                Name: "Test Workspace"
            );

            var filters = new Filters([
                new(new("Id"), new(FilterOperators.Equals), new(@event.Id)),
            ]);
            var criteria = new FilterCriteria(filters, Order.None);

            _repositoryMock.Setup(r => r.MatchingOne(criteria)).ReturnsAsync(workspace);

            await _handler.On(@event);

            _searchEngineMock.Verify(s => s.Index(workspace), Times.Once);
        }
    }
}
