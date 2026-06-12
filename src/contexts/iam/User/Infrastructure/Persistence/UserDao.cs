using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace FlowTrack.Iam.Infrastructure;

[Service(Lifetime.Scoped)]
public class UserDao(IamDbContext iamDbContext)
{
    public async Task Insert(UserEntity userEntity)
    {
        iamDbContext.Users.Add(userEntity);
    }

    public async Task<UserEntity?> FindByEmail(string email)
    {
        return await iamDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<UserEntity?> FindById(string id)
    {
        return await iamDbContext.Users.FindAsync(new Guid(id));
    }

    public async Task Update(UserEntity userEntity)
    {
        var tracked = await iamDbContext.Users.FindAsync(userEntity.Id) ?? throw new UserNotFound();
        iamDbContext.Entry(tracked).CurrentValues.SetValues(userEntity);
    }
}
