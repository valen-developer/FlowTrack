using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Domain;

public class User(UserId id, UserEmail email, UserPassword password, bool isActive = false)
    : AggregatedRoot
{
    public UserId Id { get; } = id;
    public UserEmail Email { get; } = email;
    public UserPassword Password { get; } = password;
    public bool IsActive { get; } = isActive;

    public static User Create(UserId id, UserEmail email, UserPassword password, bool isActive)
    {
        var user = new User(id, email, password, isActive);
        UserCreated userCreatedEvent = new(
            UserId: user.Id.Value,
            Email: user.Email.Value,
            IsActive: user.IsActive
        );

        user.Record(userCreatedEvent);

        return user;
    }
}
