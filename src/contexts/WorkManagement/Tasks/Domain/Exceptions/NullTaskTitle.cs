using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.WorkManagement.Tasks.Domain;

internal sealed class NullTaskTitle()
    : InvalidException("Task title can not be null.", "exception.task.title.null");
