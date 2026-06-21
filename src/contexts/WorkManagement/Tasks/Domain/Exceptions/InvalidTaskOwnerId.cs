using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.WorkManagement.Tasks.Domain;

internal class InvalidTaskOwnerId()
    : InvalidException("Invalid task owner id.", "exception.task.owner_id.invalid") { }
