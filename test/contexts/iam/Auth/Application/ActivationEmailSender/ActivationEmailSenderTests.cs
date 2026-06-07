using FlowTrack.Iam.Application;
using FlowTrack.Shared.Domain;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.Iam.Test;

public class ActivationEmailSenderTests
{
    private readonly Mock<IJWTService> _jwtServiceMock = new();
    private readonly Mock<IEnvStore> _envStoreMock = new();

    private readonly ActivationEmailSender _activationEmailSender;

    public ActivationEmailSenderTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_jwtServiceMock.Object);
        services.AddSingleton(_envStoreMock.Object);

        services.AddScoped<ActivationEmailSender>();

        var serviceProvider = services.BuildServiceProvider();
        _activationEmailSender = serviceProvider.GetRequiredService<ActivationEmailSender>();
    }

    [Fact]
    public async Task Should_Generate_A_Token()
    {
        var user = UserMother.Random();

        var expectedTokenSecret = "test_secret";
        var jwtOptions = new JWTOptions(
            secret: expectedTokenSecret,
            expirationMinutes: 60 * 24 * 7
        );

        var jwtPayload = new JWTPayload(
            new Dictionary<string, string> { { "id", user.Id.ToString() } }
        );

        var ActivationEmailSenderParams = new ActivationEmailSenderParams(
            UserId: user.Id,
            Email: user.Email
        );

        _envStoreMock
            .Setup(e => e.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()))
            .Returns(expectedTokenSecret);

        await _activationEmailSender.Send(ActivationEmailSenderParams);

        _jwtServiceMock.Verify(
            j =>
                j.Generate(
                    It.Is<JWTPayload>(p => p.Claims["id"] == user.Id.ToString()),
                    It.Is<JWTOptions>(o => o.Secret == expectedTokenSecret)
                ),
            Times.Once
        );
    }
}
