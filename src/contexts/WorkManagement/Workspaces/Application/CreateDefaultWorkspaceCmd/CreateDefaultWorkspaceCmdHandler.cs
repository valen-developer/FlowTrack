using FlowTrack.Shared.Domain.Bus.Command;
using FlowTrack.Shared.Domain.Bus.Event;
using FlowTrack.Shared.Domain.FilterCriterias;
using FlowTrack.WorkManagement.Workspaces.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Application;

internal class CreateDefaultWorkspaceCmdHandler(WorkspaceCreator workspaceCreator)
    : ICommandHandler<CreateDefaultWorkspaceCmd>
{
    public async Task Handle(CreateDefaultWorkspaceCmd command)
    {
        var workspace = Workspace.CreateDefault(
            new WorkspaceId(Guid.NewGuid().ToString()),
            new WorkspaceOwnerId(command.UserId)
        );

        await workspaceCreator.Create(workspace);
    }
}
