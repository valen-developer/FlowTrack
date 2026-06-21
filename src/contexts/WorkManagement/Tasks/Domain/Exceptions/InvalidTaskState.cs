using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.WorkManagement.Tasks.Domain;

internal class InvalidTaskState()
    : InvalidException(
        $"Invalid task state. Valid state are: {TaskState.VALID_STATES}",
        "exception.task.state.invalid"
    );
