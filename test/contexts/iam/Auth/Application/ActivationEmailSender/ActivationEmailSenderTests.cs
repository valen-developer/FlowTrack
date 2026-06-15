using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.Iam.Test.Auth.Application
{
    public class ActivationEmailSenderTests
    {
        private readonly Mock<IJWTService> _jwtServiceMock = new();
        private readonly Mock<IEnvStore> _envStoreMock = new();
        private readonly Mock<IMailer> _mailerMock = new();

        private readonly ActivationEmailSender _activationEmailSender;

        public ActivationEmailSenderTests()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_jwtServiceMock.Object);
            services.AddSingleton(_envStoreMock.Object);
            services.AddSingleton(_mailerMock.Object);
            services.AddScoped<ActivationEmailGenerator>();

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
                new Dictionary<string, string> { { "id", user.Id.Value } }
            );

            var ActivationEmailSenderParams = new ActivationEmailSenderParams(
                UserId: user.Id.Value,
                Email: user.Email.Value
            );

            _envStoreMock
                .Setup(e => e.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()))
                .Returns(expectedTokenSecret);

            _envStoreMock
                .Setup(e => e.Get(IamEnvironmentKeysEnum.IAM_URL_OF_ACTIVATION.ToString()))
                .Returns("https://example.com/activate");

            await _activationEmailSender.Send(ActivationEmailSenderParams);

            _jwtServiceMock.Verify(
                j =>
                    j.Generate(
                        It.Is<JWTPayload>(p => p.Claims["id"] == user.Id.Value),
                        It.Is<JWTOptions>(o => o.Secret == expectedTokenSecret)
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task Should_Throw_If_Token_Secret_Is_Missing()
        {
            var user = UserMother.Random();

            var ActivationEmailSenderParams = new ActivationEmailSenderParams(
                UserId: user.Id.Value,
                Email: user.Email.Value
            );

            _envStoreMock
                .Setup(e => e.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()))
                .Returns((string?)null);

            var exception = await Assert.ThrowsAsync<EnvVariableMissed>(() =>
                _activationEmailSender.Send(ActivationEmailSenderParams)
            );

            Assert.Equal(
                $"Environment variable {IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET} is required",
                exception.Message
            );
        }

        [Fact]
        public async Task Should_Send_Activation_Mail()
        {
            var user = UserMother.Random();
            var expetectedToken = "test_token";
            var expectedUrlOfActivation = $"https://example.com/activate";

            _envStoreMock
                .Setup(e => e.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()))
                .Returns("test_secret");

            _envStoreMock
                .Setup(e => e.Get(IamEnvironmentKeysEnum.IAM_URL_OF_ACTIVATION.ToString()))
                .Returns(expectedUrlOfActivation);

            _jwtServiceMock
                .Setup(j => j.Generate(It.IsAny<JWTPayload>(), It.IsAny<JWTOptions>()))
                .Returns(expetectedToken);

            var ActivationEmailSenderParams = new ActivationEmailSenderParams(
                UserId: user.Id.Value,
                Email: user.Email.Value
            );

            await _activationEmailSender.Send(ActivationEmailSenderParams);

            _mailerMock.Verify(
                m =>
                    m.Send(
                        It.Is<Mail>(m =>
                            m.To == user.Email.Value
                            && m.Subject == "Activate your account"
                            && m.Body.Contains($"{expectedUrlOfActivation}?token={expetectedToken}")
                        )
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task Should_Throw_If_Activation_URL_Is_Missing()
        {
            var user = UserMother.Random();

            _envStoreMock
                .Setup(e => e.Get(IamEnvironmentKeysEnum.ACTIVATE_TOKEN_SECRET.ToString()))
                .Returns("test_secret");

            _envStoreMock
                .Setup(e => e.Get(IamEnvironmentKeysEnum.IAM_URL_OF_ACTIVATION.ToString()))
                .Returns((string?)null);

            var ActivationEmailSenderParams = new ActivationEmailSenderParams(
                UserId: user.Id.Value,
                Email: user.Email.Value
            );

            var exception = await Assert.ThrowsAsync<EnvVariableMissed>(() =>
                _activationEmailSender.Send(ActivationEmailSenderParams)
            );

            Assert.Equal(
                $"Environment variable {IamEnvironmentKeysEnum.IAM_URL_OF_ACTIVATION} is required",
                exception.Message
            );
        }
    }
}
