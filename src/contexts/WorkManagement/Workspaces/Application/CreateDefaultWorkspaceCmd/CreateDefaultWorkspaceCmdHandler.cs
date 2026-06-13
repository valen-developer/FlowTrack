using FlowTrack.Shared.Domain.Bus.Command;
using FlowTrack.WorkManagement.Workspaces.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Application;

internal class CreateDefaultWorkspaceCmdHandler(IWorkspaceRepository repository)
    : ICommandHandler<CreateDefaultWorkspaceCmd>
{
    public async Task Handle(CreateDefaultWorkspaceCmd command)
    {
        var workspace = Workspace.CreateDefault(
            new WorkspaceId(Guid.NewGuid().ToString()),
            new WorkspaceOwnerId(command.UserId)
        );

        await repository.Save(workspace);
    }
}
