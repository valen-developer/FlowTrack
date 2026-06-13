using FlowTrack.Shared.Domain.Bus.Command;
using FlowTrack.Shared.Domain.Iam.Users;

namespace FlowTrack.WorkManagement.Workspaces.Application;

internal sealed class CreateDefaultWorkspaceOnUserCreated(ICommandBus commandBus)
{
    public async Task On(UserCreated @event)
    {
        var command = new CreateDefaultWorkspaceCmd(@event.UserId);
        await commandBus.Dispatch(command);
    }
}
