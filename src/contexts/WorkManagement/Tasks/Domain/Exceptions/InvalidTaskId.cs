using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.WorkManagement.Tasks.Domain;

public sealed class InvalidTaskId()
    : InvalidException("Invalid task id.", "exception.task.id.invalid") { }
