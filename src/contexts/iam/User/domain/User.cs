using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Domain;

public class User(Guid id, string email, string password, bool isActive = false) : AggregatedRoot
{
    public Guid Id { get; } = id;
    public string Email { get; } = email;
    public string Password { get; } = password;
    public bool IsActive { get; } = isActive;

    public static User Create(string id, string email, string password, bool isActive)
    {
        var user = new User(Guid.Parse(id), email, password, isActive);
        UserCreated userCreatedEvent = new(
            UserId: user.Id,
            Email: user.Email,
            IsActive: user.IsActive
        );

        user.Record(userCreatedEvent);

        return user;
    }
}
