using FlowTrack.Shared.Domain.Bus.Command;

namespace FlowTrack.WorkManagement.Tasks.Application;

internal sealed record CreateTaskCmd(string Id, string Title, string Description, string State)
    : ICommand;
