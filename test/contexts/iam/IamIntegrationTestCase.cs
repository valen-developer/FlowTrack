namespace FlowTrack.Iam.Test
{
    public abstract class IamIntegrationTestCase : IClassFixture<IamIntegrationFixture>
    {
        public readonly IamIntegrationFixture _fixture;

        public IamIntegrationTestCase(IamIntegrationFixture fixture)
        {
            _fixture = fixture;

            fixture.serviceCollection.DiscoverServices([
                "FlowTrack.Iam*.dll",
                "FlowTrack.Shared*.dll",
            ]);
        }

        protected Transaction GenerateTransaction()
        {
            var dbcontext = _fixture.GetService<IamDbContext>();
            return new EfCoreTransaction(dbcontext);
        }

        internal async Task AddUserToDatabase(User user)
        {
            var dbcontext = _fixture.GetService<IamDbContext>();
            var userDao = _fixture.GetService<UserDao>();
            var userEntity = UserEntity.FromDomain(user);
            userEntity.Password = BCrypt.Net.BCrypt.HashPassword(user.Password.Value);
            await userDao.Insert(userEntity);

            dbcontext.SaveChanges();
        }
    }
}
