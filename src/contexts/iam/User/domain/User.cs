namespace FlowTrack.Iam.Domain;

public class User(Guid id, string email, string password)
{
    public Guid Id { get; } = id;
    public string Email { get; } = email;
    public string Password { get; } = password;
}
