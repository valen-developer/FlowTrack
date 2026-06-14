using FlowTrack.Shared.Domain.Bus.Command;
using FlowTrack.Shared.Domain.Contexts;
using FlowTrack.Shared.Domain.Dic;
using FlowTrack.WorkManagement.Workspaces.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.WorkManagement.Workspaces.Application;

[Service]
internal class CreateDefaultWorkspaceCmdHandler(
    WorkspaceCreator workspaceCreator,
    [FromKeyedServices("WORK_MANAGEMENT")] Context context
) : ICommandHandler<CreateDefaultWorkspaceCmd>
{
    public async Task Handle(CreateDefaultWorkspaceCmd command)
    {
        await context.Transaction.RunInTransaction(async () =>
        {
            var workspace = Workspace.CreateDefault(
                new WorkspaceId(Guid.NewGuid().ToString()),
                new WorkspaceOwnerId(command.UserId)
            );

            await workspaceCreator.Create(workspace);

            return true;
        });
    }
}
