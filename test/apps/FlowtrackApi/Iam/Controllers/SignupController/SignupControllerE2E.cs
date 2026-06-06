using System.Net;
using System.Net.Http.Json;

namespace FlowtrackApi.Iam;

public class SignupControllerE2E(FlowtrackApiFixture fixture) : FlowtrackApiE2E(fixture)
{
    [Fact]
    public async Task Should_return_201_Created()
    {
        var request = new { Email = "validEmail@email.com", Password = "validPassword123" };
        var response = await HttpClient.PostAsJsonAsync("/auth/signup", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
