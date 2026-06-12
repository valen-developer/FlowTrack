using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Infrastructure;

[Provider(typeof(IUserRepository), Lifetime.Scoped)]
public class EfUserRepository(UserDao userDao) : IUserRepository
{
    private readonly UserDao userDao = userDao;

    public async Task<User?> FindByEmail(string email)
    {
        var userEntity = await userDao.FindByEmail(email);
        if (userEntity is null)
        {
            return null;
        }

        var user = userEntity.ToDomain();
        return user;
    }

    public async Task<User?> FindById(string id)
    {
        var userEntity = await userDao.FindById(id);
        if (userEntity is null)
        {
            return null;
        }

        var user = userEntity.ToDomain();
        return user;
    }

    public Task Create(User user)
    {
        var userEntity = UserEntity.FromDomain(user);
        return userDao.Insert(userEntity);
    }
}
