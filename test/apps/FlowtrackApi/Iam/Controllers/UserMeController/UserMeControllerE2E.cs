using System.Net;
using System.Net.Http.Json;
using FlowTrack.Iam.Application;
using FlowTrack.Iam.Infrastructure;
using FlowTrack.Iam.Schemas;
using FlowTrack.Iam.Test;
using Microsoft.Extensions.DependencyInjection;

namespace FlowtrackApi.Test;

[Collection(nameof(FlowtrackApiCollection))]
public class UserMeControllerE2E(FlowtrackApiFixture fixture) : FlowtrackApiE2E(fixture)
{
    [Fact]
    public async Task Should_Return_User_Info()
    {
        var user = UserMother.Random();
        var userEntity = UserEntity.FromDomain(user);
        userEntity.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        var userDao =
            Services.GetService<UserDao>()
            ?? throw new InvalidOperationException("UserDao service not found");

        await userDao.Insert(userEntity);

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
        Assert.Equal(user.Id.ToString(), userInfo.Id);
        Assert.Equal(user.Email, userInfo.Email);
    }
}
