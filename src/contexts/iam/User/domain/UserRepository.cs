namespace FlowTrack.Iam.User.Domain;

public interface IUserRepository
{
    Task<User?> FindByEmail(string email);
}
