using FlowTrack.Shared.Domain.Bus.Command;
using FlowTrack.Shared.Domain.Bus.Event;
using FlowTrack.WorkManagement.Tasks.Domain;

namespace FlowTrack.WorkManagement.Tasks.Application;

internal sealed class CreateTaskCmdHandler(ITaskRepository repository, EventBus eventBus)
    : ICommandHandler<CreateTaskCmd>
{
    public async Task Handle(CreateTaskCmd command)
    {
        var task = Tasky.Create(
            Id: new(command.Id),
            OwnerId: new(command.OwnerId),
            WorkspaceId: new(command.WorkspaceId),
            Title: new(command.Title),
            Description: new(command.Description),
            State: TaskState.FromString(command.State)
        );

        await repository.Save(task);

        await eventBus.Publish(task.PullDomainEvents());
    }
}
