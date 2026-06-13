using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace FlowtrackApi.Test;

public class UserActivationPostControllerE2E(FlowtrackApiFixture fixture) : FlowtrackApiE2E(fixture)
{
    [Fact]
    public async Task Should_Activate_User_By_Token_In_Body()
    {
        var user = UserMother.Inactive();
        await AddUserToDatabase(user);

        var activationToken = GetActivationToken(user);

        var request = new { Token = activationToken };
        var response = await HttpClient.PostAsJsonAsync("/user-activations", request);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

        var userFromDb = await GetUserFromDatabase(user.Id.Value);

        Assert.True(userFromDb.IsActive);
    }

    private string GetActivationToken(User user)
    {
        var envStore =
            Services.GetService<IEnvStore>()
            ?? throw new InvalidOperationException("EnvStore service not found");

        var jwtService =
            Services.GetService<IJWTService>()
            ?? throw new InvalidOperationException("JWTService service not found");

        var secret =
            envStore.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString())
            ?? throw new InvalidOperationException(
                "Activation token secret not found in environment variables"
            );

        var tokenPayload = new JWTPayload(
            new Dictionary<string, string> { { "id", user.Id.Value.ToString() } }
        );

        var tokenOptions = new JWTOptions(secret, 60);

        var token = jwtService.Generate(tokenPayload, tokenOptions);

        return token;
    }

    private async Task<User> GetUserFromDatabase(string userId)
    {
        var sql = $"SELECT * FROM \"users\" WHERE \"Id\" = '{userId}'";

        var sqlResult = await _fixture.ExecuteQueryAsync<UserEntity>(sql);

        var userEntity = sqlResult.FirstOrDefault();

        if (userEntity == null)
        {
            throw new InvalidOperationException(
                $"User with ID {userId} not found in the database."
            );
        }

        return userEntity.ToDomain();
    }
}
