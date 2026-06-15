using FlowTrack.Shared.Domain.Bus.Event;
using FlowTrack.WorkManagement.Shared.Infrastructure;
using FlowTrack.WorkManagement.Test;
using FlowTrack.WorkManagement.Workspaces.Domain;
using FlowTrack.WorkManagement.Workspaces.Infrastructure;
using FlowTrack.WorkManagement.Workspaces.Infrastructure.Persistence;

namespace FlowTrack.WorkManagement.Workspaces.Test.Infrastructure;

public class IndexWorkspaceOnWorkspaceCreatedIT : WorkManagementIntegrationTestCase
{
    private readonly EventBus _eventBus;

    public IndexWorkspaceOnWorkspaceCreatedIT(WorkManagementIntegrationFixture fixture)
        : base(fixture)
    {
        _eventBus = fixture.GetService<EventBus>();
    }

    [Fact]
    public async Task Should_Index_Workspace_In_Search_Engine()
    {
        var workspace = WorkspaceMother.Random();
        await AddWorkspaceToDatabase(workspace);

        var @event = new WorkspaceCreated(
            Id: workspace.Id.Value,
            Name: workspace.Name.Value,
            OwnerId: workspace.OwnerId.Value
        );

        await _eventBus.Publish(@event);

        var elasticQuery = new
        {
            query = new
            {
                @bool = new
                {
                    must = new object[]
                    {
                        new { match = new { name = workspace.Name.Value } },
                        new { match = new { ownerId = workspace.OwnerId.Value } },
                        new { match = new { id = workspace.Id.Value } },
                    },
                },
            },
        };

        List<WorkspaceSearchDocument> searchEngineResponse = [];
        await WaitForAsync(
            async () =>
            {
                try
                {
                    searchEngineResponse =
                        await _fixture.ExecuteQueryOnSearchEngine<WorkspaceSearchDocument>(
                            "workspaces",
                            elasticQuery
                        );

                    return searchEngineResponse.Count > 0;
                }
                catch
                {
                    return false;
                }
            },
            timeoutMs: 5000,
            pollIntervalMs: 250
        );

        Assert.NotNull(searchEngineResponse);
        Assert.Single(searchEngineResponse);

        var indexedWorkspace = searchEngineResponse.First();
        Assert.Equal(workspace.Id.Value, indexedWorkspace.Id);
    }

    private async Task AddWorkspaceToDatabase(Workspace workspace)
    {
        var dbContext = _fixture.GetService<WorkManagementDbContext>();
        await dbContext.Workspaces.AddAsync(WorkspaceEntity.FromDomain(workspace));
        await dbContext.SaveChangesAsync();
    }
}
