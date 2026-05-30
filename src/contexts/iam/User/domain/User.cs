namespace Iam.User.Domain;

public class User(Guid id, String email, String password)
{
    public Guid Id { get; } = id;
    public String Email { get; } = email;
    public String Password { get; } = password;
}
