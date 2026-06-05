using System.Net;
using System.Net.Http.Json;
using FlowTrack.Iam.Application;
using FlowTrack.Iam.Domain;
using FlowTrack.Iam.Infrastructure;
using FlowTrack.Iam.Test;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FlowtrackApi.Iam;

public class SinginControllerE2E(FlowtrackApiFixture fixture) : FlowtrackApiE2E(fixture)
{
    [Fact]
    public async Task Should_Set_Auth_Cookies()
    {
        var bcrypt =
            Services.GetService<IBcrypt>()
            ?? throw new InvalidOperationException("BCrypt service not found");

        var userDao =
            Services.GetService<UserDao>()
            ?? throw new InvalidOperationException("UserDao service not found");

        var user = UserMother.Active();
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
        var userEntity = UserEntity.FromDomain(user);
        userEntity.Password = hashedPassword;

        await userDao.Insert(userEntity);

        var authTokenGenerator =
            Services.GetService<AuthTokenGenerator>()
            ?? throw new InvalidOperationException("AuthTokenGenerator service not found");

        var signinSuccess = authTokenGenerator.Generate(user);

        var request = new { Email = user.Email, Password = user.Password };
        var response = await HttpClient.PostAsJsonAsync("/auth/signin", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cookies = response.Headers.GetValues("Set-Cookie");
        Assert.NotNull(cookies);

        var expectedAccessTokenCookie = $"ACCESS_TOKEN={signinSuccess.AccessToken}";
        var expectedRefreshTokenCookie = $"REFRESH_TOKEN={signinSuccess.RefreshToken}";

        Assert.Contains(cookies, cookie => cookie.StartsWith(expectedAccessTokenCookie));
        Assert.Contains(cookies, cookie => cookie.StartsWith(expectedRefreshTokenCookie));
    }

    [Fact]
    public async Task Should_Return_Unauthorized_For_Invalid_Credentials()
    {
        var signinFailed = new SigninFailed();
        var request = new { Email = "email@email.com", Password = "wrongpassword" };
        var response = await HttpClient.PostAsJsonAsync("/auth/signin", request);

        var responseBody = await response.Content.ReadFromJsonAsync<HttpErrorResponse>();

        Assert.NotNull(responseBody);
        Assert.Equal(signinFailed.Message, responseBody.ErrorMessage);
        Assert.Equal(signinFailed.Code, responseBody.ErrorCode);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
