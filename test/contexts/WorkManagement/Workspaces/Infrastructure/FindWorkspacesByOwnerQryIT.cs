using FlowTrack.WorkManagement.Test;
using FlowTrack.WorkManagement.Workspaces.Application;
using FlowTrack.WorkManagement.Workspaces.Domain;
using FlowTrack.WorkManagement.Workspaces.Infrastructure;

namespace FlowTrack.WorkManagement.Workspaces.Test.Infrastructure;

public class FindWorkspacesByOwnerQryIT : WorkManagementIntegrationTestCase
{
    private readonly FindWorkspacesByOwnerQryHandler _handler;

    public FindWorkspacesByOwnerQryIT(WorkManagementIntegrationFixture fixture)
        : base(fixture)
    {
        _handler = fixture.GetService<FindWorkspacesByOwnerQryHandler>();
    }

    [Fact]
    public async Task Should_Find_Workspaces_By_Owner()
    {
        await _fixture.EnsureServicesAsync();

        var ownerId = Guid.NewGuid().ToString();
        List<Workspace> expectedWorkspaces = [WorkspaceMother.WithOwner(ownerId)];
        List<WorkspaceSearchDocument> indexedDocs =
        [
            .. expectedWorkspaces.Select(WorkspaceSearchDocument.FromDomain),
        ];

        await _fixture.Containers.IndexDocs("workspaces", indexedDocs);
        var elasticQuery = new { query = new { match = new { ownerId } } };
        var internalResult =
            await _fixture.Containers.ExecuteQueryOnSearchEngine<WorkspaceSearchDocument>(
                "workspaces",
                elasticQuery
            );

        var query = new FindWorkspacesByOwnerQry(ownerId);
        var result = await _handler.Handle(query);

        Assert.Equal(expectedWorkspaces.Count, result.Count);
        Assert.Equal(internalResult, indexedDocs);
    }
}
