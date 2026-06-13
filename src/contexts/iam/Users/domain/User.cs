namespace FlowTrack.Iam.Users.Domain;

internal class User(UserId id, UserEmail email, UserPassword password, bool isActive = false)
    : AggregatedRoot
{
    public UserId Id { get; } = id;
    public UserEmail Email { get; } = email;
    public UserPassword Password { get; } = password;
    public bool IsActive { get; private set; } = isActive;

    public static User Create(UserId id, UserEmail email, UserPassword password)
    {
        var user = new User(id, email, password, false);
        UserCreated userCreatedEvent = new(
            UserId: user.Id.Value,
            Email: user.Email.Value,
            IsActive: user.IsActive
        );

        user.Record(userCreatedEvent);

        return user;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;

        var activatedEvent = new UserActivated(Id: Id.Value);

        Record(activatedEvent);
    }
}
