using FlowTrack.Shared.Domain.Bus.Event;
using FlowTrack.Shared.Domain.Dic;
using FlowTrack.Shared.Domain.FilterCriterias;
using FlowTrack.WorkManagement.Workspaces.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Application;

[Service]
[DomainEventSubscriber(typeof(WorkspaceCreated))]
internal class IndexWorkspaceOnWorkspaceCreated(
    IWorkspaceRepository repository,
    IWorkspaceSearchEngine searchEngine
)
{
    [DomainEventListener]
    public async Task On(WorkspaceCreated @event)
    {
        var filters = new Filters([new(new("Id"), new(FilterOperators.Equals), new(@event.Id))]);
        var criteria = new FilterCriteria(filters, Order.None);

        var workspace = await repository.MatchingOne(criteria);

        if (workspace is not null)
        {
            await searchEngine.Index(workspace);
        }
    }
}
