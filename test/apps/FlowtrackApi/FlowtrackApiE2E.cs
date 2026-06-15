using Microsoft.Extensions.DependencyInjection;

namespace FlowtrackApi.Test
{
    [Collection(nameof(FlowtrackApiCollection))]
    public abstract class FlowtrackApiE2E(FlowtrackApiFixture fixture)
    {
        protected readonly FlowtrackApiFixture _fixture = fixture;
        protected HttpClient HttpClient => _fixture.HttpClient;
        protected IServiceProvider Services => _fixture.Services;

        internal async Task AddUserToDatabase(User user)
        {
            var dbContext =
                Services.GetService<IamDbContext>()
                ?? throw new InvalidOperationException("IamDbContext service not found");

            var bcrypt =
                Services.GetService<IBcrypt>()
                ?? throw new InvalidOperationException("BCrypt service not found");

            var userDao =
                Services.GetService<UserDao>()
                ?? throw new InvalidOperationException("UserDao service not found");

            var hashedPassword = bcrypt.Hash(user.Password.Value);
            var userEntity = UserEntity.FromDomain(user);
            userEntity.Password = hashedPassword;

            await userDao.Insert(userEntity);

            dbContext.SaveChanges();
        }
    }
}
