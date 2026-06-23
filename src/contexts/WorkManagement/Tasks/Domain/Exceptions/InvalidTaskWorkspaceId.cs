using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.WorkManagement.Tasks.Domain;

internal class InvalidTaskWorkspaceId()
    : InvalidException("Invalid workspace id", "exception.task.workspace_id.invalid") { }
