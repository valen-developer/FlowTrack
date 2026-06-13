using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.WorkManagement.Workspaces.Domain.Exceptions;

internal class InvalidWorkspaceId()
    : InvalidException(
        "The provided workspace ID is invalid.",
        "exception.workspace.id.invalid"
    ) { }
