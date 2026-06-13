namespace FlowTrack.Iam.Users.Domain;

public interface IUserRepository
{
    Task<User?> FindByEmail(string email);
    Task<User?> FindById(string id);
    Task Create(User user);
    Task Update(User user);
}
