namespace FlowTrack.Iam.Domain;

public interface IUserRepository
{
    Task<User?> FindByEmail(string email);
}
