using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrackIamApi.Test.Iam.Controllers.Users;

[Collection(nameof(FlowTrackIamApiCollection))]
public class UserMeControllerE2E(FlowTrackIamApiFixture fixture) : FlowTrackIamApiE2E(fixture)
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
}
