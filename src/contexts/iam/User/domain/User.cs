using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Domain;

public class User(UserId id, UserEmail email, UserPassword password, bool isActive = false)
    : AggregatedRoot
{
    public UserId Id { get; } = id;
    public UserEmail Email { get; } = email;
    public UserPassword Password { get; } = password;
    public bool IsActive { get; } = isActive;

    public static User Signup(UserId id, UserEmail email, UserPassword password)
    {
        var user = new User(id, email, password, false);
        UserSignupped userCreatedEvent = new(UserId: user.Id.Value, Email: user.Email.Value);

        user.Record(userCreatedEvent);

        return user;
    }
}
