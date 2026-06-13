using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace FlowtrackApi.Test.Iam.Controllers.Users;

[Collection(nameof(FlowtrackApiCollection))]
public class UserMeControllerE2E(FlowtrackApiFixture fixture) : FlowtrackApiE2E(fixture)
{
    [Fact]
    public async Task Should_Return_User_Info()
    {
        var user = UserMother.Random();
        await AddUserToDatabase(user);

        var authTokenGenerator =
            Services.GetService<AuthTokenGenerator>()
            ?? throw new InvalidOperationException("AuthTokenGenerator service not found");

        var signinSuccess = authTokenGenerator.Generate(user);

        HttpClient.DefaultRequestHeaders.Add(
            "Cookie",
            $"ACCESS_TOKEN={signinSuccess.AccessToken}; REFRESH_TOKEN={signinSuccess.RefreshToken}"
        );

        var response = await HttpClient.GetAsync("/user/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var userInfo = await response.Content.ReadFromJsonAsync<UserMeResponse>();
        Assert.NotNull(userInfo);
        Assert.Equal(user.Id.Value, userInfo.Id);
        Assert.Equal(user.Email.Value, userInfo.Email);
    }

    private async Task AddUserToDatabase(User user)
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
