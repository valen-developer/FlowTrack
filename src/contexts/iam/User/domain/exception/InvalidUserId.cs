using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Domain;

public sealed class InvalidUserId : InvalidException
{
    public InvalidUserId()
        : base("Invalid user id. It must be a valid UUID string.", "exception.user.id.invalid") { }
}
