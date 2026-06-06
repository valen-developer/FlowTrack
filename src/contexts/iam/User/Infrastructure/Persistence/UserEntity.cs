using FlowTrack.Iam.Domain;

namespace FlowTrack.Iam.Infrastructure;

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
            Id = user.Id,
            Email = user.Email,
            Password = user.Password,
            IsActive = user.IsActive,
        };
    }

    public User ToDomain()
    {
        return new User(Id, Email, Password, IsActive);
    }
}
