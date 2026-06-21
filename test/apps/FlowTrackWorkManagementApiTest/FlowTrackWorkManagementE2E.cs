using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Infrastructure.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrackWorkManagementApiTest;

public abstract class FlowTrackWorkManagementE2E(FlowTrackWorkManagementApiFixture fixture)
{
    protected readonly FlowTrackWorkManagementApiFixture _fixture = fixture;
    protected HttpClient HttpClient => _fixture.HttpClient;
    protected IServiceProvider Services => _fixture.Services;

    public void As(string userId, HttpRequestMessage request)
    {
        var env = Services.GetRequiredService<IEnvStore>();
        var jwtService = Services.GetRequiredService<IJWTService>();

        var jwtOptions = new JWTOptions(
            env.Get(AuthEnvironmentKeysEnum.ACCESS_TOKEN_SECRET.ToString()) ?? "",
            60 * 24
        );

        var jwtPayload = new JWTPayload(new Dictionary<string, string> { { "id", userId } });

        var accessToken = jwtService.Generate(jwtPayload, jwtOptions);
        request.Headers.Add("Cookie", $"ACCESS_TOKEN={accessToken}");
    }
}
