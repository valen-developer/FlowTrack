using FlowTrack.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FlowTrack.Iam.Infrastructure;

public class UserDao(IamDbContext iamDbContext)
{
    public async Task Insert(UserEntity userEntity)
    {
        iamDbContext.Users.Add(userEntity);
        await iamDbContext.SaveChangesAsync();
    }

    public async Task<UserEntity?> FindByEmail(string email)
    {
        return await iamDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}
