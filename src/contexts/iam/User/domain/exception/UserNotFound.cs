using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Domain;

public sealed class UserNotFound : NotFoundException
{
    public UserNotFound()
        : base("User not found", "exception.user.not_found") { }
}
