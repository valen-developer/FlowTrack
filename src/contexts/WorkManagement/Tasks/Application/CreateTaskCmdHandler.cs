using FlowTrack.Shared.Domain.Bus.Command;
using FlowTrack.WorkManagement.Tasks.Domain;

namespace FlowTrack.WorkManagement.Tasks.Application;

internal sealed class CreateTaskCmdHandler(ITaskRepository repository)
    : ICommandHandler<CreateTaskCmd>
{
    public async Task Handle(CreateTaskCmd command)
    {
        var task = Tasky.Create(
            Id: new(command.Id),
            Title: new(command.Title),
            Description: new(command.Description),
            State: TaskState.FromString(command.State)
        );

        await repository.Save(task);
    }
}
