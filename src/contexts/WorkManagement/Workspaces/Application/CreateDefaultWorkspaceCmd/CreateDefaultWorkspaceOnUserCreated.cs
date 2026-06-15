using FlowTrack.Shared.Domain.Bus.Command;
using FlowTrack.Shared.Domain.Bus.Event;
using FlowTrack.Shared.Domain.Dic;
using FlowTrack.Shared.Domain.Iam.Users;

namespace FlowTrack.WorkManagement.Workspaces.Application
{
    [Service]
    [DomainEventSubscriber(typeof(UserCreated))]
    internal sealed class CreateDefaultWorkspaceOnUserCreated(ICommandBus commandBus)
    {
        [DomainEventListener]
        public async Task On(UserCreated @event)
        {
            var command = new CreateDefaultWorkspaceCmd(@event.UserId);
            await commandBus.Dispatch(command);
        }
    }
}
