namespace FlowTrack.Iam.Domain;

public class User(Guid id, string email, string password, bool isActive = false)
{
    public Guid Id { get; } = id;
    public string Email { get; } = email;
    public string Password { get; } = password;
    public bool IsActive { get; } = isActive;

    public static User Create(string id, string email, string password, bool isActive)
    {
        return new User(Guid.Parse(id), email, password, isActive);
    }
}
