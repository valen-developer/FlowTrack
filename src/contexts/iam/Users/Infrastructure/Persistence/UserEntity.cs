namespace FlowTrack.Iam.Users.Infrastructure.Persistence;

public class UserEntity
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required bool IsActive { get; set; }

    public static UserEntity FromDomain(User user)
    {
        return new UserEntity()
        {
            Id = new Guid(user.Id.Value),
            Email = user.Email.Value,
            Password = user.Password.Value,
            IsActive = user.IsActive,
        };
    }

    public User ToDomain()
    {
        return new User(
            new UserId(Id.ToString()),
            new UserEmail(Email),
            new UserPassword(Password),
            IsActive
        );
    }
}
