using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Users.Domain;

internal sealed class UserNotFound : NotFoundException
{
    public UserNotFound()
        : base("User not found", "exception.user.not_found") { }
}
