using FlowTrack.Shared.Domain.Bus.Query;
using FlowTrack.Shared.Domain.FilterCriterias;
using FlowTrack.WorkManagement.Workspaces.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Application;

internal class FindWorkspacesByOwnerQryHandler(IWorkspaceSearchEngine searchEngine)
    : IQueryHandler<FindWorkspacesByOwnerQry, List<Workspace>>
{
    public async Task<List<Workspace>> Handle(FindWorkspacesByOwnerQry query)
    {
        var filters = new Filters([
            new(new("OwnerId"), new(FilterOperators.Equals), new(query.OwnerId)),
        ]);

        var criteria = new FilterCriteria(filters, Order.None);

        var workspaces = await searchEngine.Find(criteria);

        return workspaces;
    }
}
