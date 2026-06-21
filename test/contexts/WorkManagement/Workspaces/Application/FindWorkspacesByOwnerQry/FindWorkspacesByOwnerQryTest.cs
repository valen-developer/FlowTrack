using FlowTrack.Shared.Domain.FilterCriterias;
using FlowTrack.WorkManagement.Workspaces.Application;
using FlowTrack.WorkManagement.Workspaces.Domain;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.WorkManagement.Workspaces.Test;

public class FindWorkspacesByOwnerQryTest
{
    private Mock<IWorkspaceSearchEngine> _workspaceSearchEngineMock = new();
    private FindWorkspacesByOwnerQryHandler _handler;

    public FindWorkspacesByOwnerQryTest()
    {
        var services = new ServiceCollection();

        services.AddSingleton(_workspaceSearchEngineMock.Object);
        services.AddScoped<FindWorkspacesByOwnerQryHandler>();

        var serviceProvider = services.BuildServiceProvider();
        _handler = serviceProvider.GetRequiredService<FindWorkspacesByOwnerQryHandler>();
    }

    [Fact]
    public async Task Should_Find_On_Search_Engine_By_Owner_Id()
    {
        var ownerId = Guid.NewGuid().ToString();

        var filters = new Filters([new(new("OwnerId"), new(FilterOperators.Equals), new(ownerId))]);
        var criteria = new FilterCriteria(filters, Order.None);

        var query = new FindWorkspacesByOwnerQry(ownerId);

        await _handler.Handle(query);

        _workspaceSearchEngineMock.Verify(x => x.Find(criteria), Times.Once);
    }
}
