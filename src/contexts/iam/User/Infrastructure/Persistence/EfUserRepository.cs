using FlowTrack.Iam.Domain;

namespace FlowTrack.Iam.Infrastructure;

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

    public async Task<User?> FindById(Guid id)
    {
        var userEntity = await userDao.FindById(id);
        if (userEntity is null)
        {
            return null;
        }

        var user = userEntity.ToDomain();
        return user;
    }
}
