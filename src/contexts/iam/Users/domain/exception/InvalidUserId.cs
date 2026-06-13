namespace FlowTrack.Iam.Users.Domain;

internal sealed class InvalidUserId : InvalidException
{
    public InvalidUserId()
        : base("Invalid user id. It must be a valid UUID string.", "exception.user.id.invalid") { }
}
