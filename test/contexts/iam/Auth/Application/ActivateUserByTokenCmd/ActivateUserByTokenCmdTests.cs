using FlowTrack.Iam.Application;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.Iam.Test;

public class ActivateUserByTokenCmdTests
{
    private readonly ActivateUserByTokenCmdHandler _handler;
    private readonly Mock<IJWTService> _jwtServiceMock = new();
    private readonly Mock<IEnvStore> _envStoreMock = new();

    public ActivateUserByTokenCmdTests()
    {
        var service = new ServiceCollection();
        service.AddSingleton(_jwtServiceMock.Object);
        service.AddSingleton(_envStoreMock.Object);

        service.AddScoped<ActivateUserByTokenCmdHandler>();

        _handler = service
            .BuildServiceProvider()
            .GetRequiredService<ActivateUserByTokenCmdHandler>();
    }

    [Fact]
    public async Task Should_Validate_The_Token()
    {
        var token = "valid_token";
        var secret = "secret_key";
        var userId = Guid.NewGuid();

        _envStoreMock
            .Setup(s => s.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()))
            .Returns(secret);

        var cmd = new ActivateUserByTokenCmd(token);
        await _handler.Handle(cmd);

        _jwtServiceMock.Verify(s => s.Verify(token, secret), Times.Once);
    }
}
