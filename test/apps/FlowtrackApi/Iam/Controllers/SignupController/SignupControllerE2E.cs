using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace FlowtrackApi.Test.Iam.Controllers.Auth;

[Collection(nameof(FlowtrackApiCollection))]
public class SignupControllerE2E(FlowtrackApiFixture fixture) : FlowtrackApiE2E(fixture)
{
    [Fact]
    public async Task Should_Save_User_In_DB()
    {
        UserDao userDao =
            Services.GetService<UserDao>()
            ?? throw new InvalidOperationException(
                "UserDao not registered in the service provider."
            );

        var request = new
        {
            Id = Guid.NewGuid().ToString(),
            Email = "validemail@email.com",
            Password = "validPassword123",
        };
        var response = await HttpClient.PostAsJsonAsync("/auth/signup", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var user = await userDao.FindById(request.Id);
        Assert.NotNull(user);
        Assert.Equal(request.Email, user.Email);
    }
}
