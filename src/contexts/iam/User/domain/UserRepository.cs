namespace FlowTrack.Iam.Domain;

public interface IUserRepository
{
    Task<User?> FindByEmail(string email);
    Task<User?> FindById(Guid id);
    Task Create(User user);
}
