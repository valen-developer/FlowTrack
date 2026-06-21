using System.Net;
using System.Net.Http.Json;
using FlowTrack.WorkManagement.Workspaces.Domain;
using FlowTrack.WorkManagement.Workspaces.Infrastructure;
using FlowTrack.WorkManagement.Workspaces.Test;
using FlowTrackWorkManagementApi.Workspaces.Schemas;

namespace FlowTrackWorkManagementApiTest.Workspaces.Controllers;

[Collection(nameof(FlowTrackWorkManagementE2ECollection))]
public class WorkspacesGETControllerE2E(FlowTrackWorkManagementApiFixture fixture)
    : WorkspaceE2E(fixture)
{
    [Fact]
    public async Task Should_Get_Workspaces()
    {
        var ownerId = Guid.NewGuid().ToString();
        List<Workspace> expectedWorkspaces = [WorkspaceMother.WithOwner(ownerId)];
        var searchDocs = expectedWorkspaces.Select(WorkspaceSearchDocument.FromDomain).ToList();
        await _fixture.Containers.IndexDocs("workspaces", searchDocs);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/workspace");
        As(ownerId, request);
        var response = await HttpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var workspacesResponse = await response.Content.ReadFromJsonAsync<
            List<WorkspaceResponse>
        >();

        Assert.Equal(
            expectedWorkspaces.Select(WorkspaceResponse.FromWorkspace),
            workspacesResponse
        );
    }
}
