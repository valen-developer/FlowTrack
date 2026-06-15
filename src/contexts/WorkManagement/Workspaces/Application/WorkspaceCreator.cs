using FlowTrack.Shared.Domain.Bus.Event;
using FlowTrack.Shared.Domain.Dic;
using FlowTrack.Shared.Domain.FilterCriterias;
using FlowTrack.WorkManagement.Workspaces.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Application
{
    [Service]
    internal class WorkspaceCreator(IWorkspaceRepository repository, EventBus eventBus)
    {
        public async Task Create(Workspace workspace)
        {
            var filters = new Filters([
                new(new("ownerId"), new(FilterOperators.Equals), new(workspace.OwnerId.Value)),
                new(new("name"), new(FilterOperators.Equals), new(workspace.Name.Value)),
            ]);

            var criteria = new FilterCriteria(filters, Order.None);

            var matchingWorkspaces = await repository.Matching(criteria);
            var existingWorkspace = matchingWorkspaces.FirstOrDefault();

            if (existingWorkspace is not null)
                return;

            await repository.Save(workspace);

            await eventBus.Publish(workspace.PullDomainEvents());
        }
    }
}
