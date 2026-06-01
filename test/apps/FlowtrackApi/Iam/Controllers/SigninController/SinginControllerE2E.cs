using System.Net;
using System.Net.Http.Json;
using FlowTrack.Iam.Test;

namespace FlowtrackApi.Iam;

public class SinginControllerE2E(FlowtrackApiFixture fixture) : FlowtrackApiE2E(fixture)
{
    [Fact]
    public async Task Should_Set_Auth_Cookies()
    {
        var user = UserMother.Random();

        var response = await HttpClient.PostAsJsonAsync(
            "/auth/signin",
            new { email = user.Email, password = user.Password }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cookies = response.Headers.GetValues("Set-Cookie");
        Assert.NotNull(cookies);

        // Make a request to /auth/signin with valid credentials
        // Get a 200 status code
        // Get the access token and refresh token from the response cookies
    }
}
