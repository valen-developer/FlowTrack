using FlowTrack.Shared.Domain.Bus.Command;
using FlowTrack.Shared.Domain.FilterCriterias;
using FlowTrack.WorkManagement.Workspaces.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Application;

internal class CreateDefaultWorkspaceCmdHandler(IWorkspaceRepository repository)
    : ICommandHandler<CreateDefaultWorkspaceCmd>
{
    public async Task Handle(CreateDefaultWorkspaceCmd command)
    {
        var filters = new Filters([
            new(new("ownerId"), new(FilterOperators.Equals), new(command.UserId)),
            new(new("name"), new(FilterOperators.Equals), new(Workspace.DefaultName)),
        ]);
        var criteria = new FilterCriteria(filters, Order.None);

        var matchingWorkspaces = await repository.Matching(criteria);
        var existingWorkspace = matchingWorkspaces.FirstOrDefault();

        if (existingWorkspace is not null)
            return;

        var workspace = Workspace.CreateDefault(
            new WorkspaceId(Guid.NewGuid().ToString()),
            new WorkspaceOwnerId(command.UserId)
        );

        await repository.Save(workspace);
    }
}
