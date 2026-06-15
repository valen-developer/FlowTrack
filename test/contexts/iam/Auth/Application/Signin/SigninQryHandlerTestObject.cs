using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.Iam.Test.Auth.Application
{
    internal sealed class SigninQryHandlerTestObject
    {
        public const string ACCESS_JWT_SECRET_KEY = "ACCESS_TOKEN_SECRET";
        public const string REFRESH_JWT_SECRET_KEY = "REFRESH_TOKEN_SECRET";
        public const string REFRESH_JWT_EXPIRE_MINUTES_KEY = "REFRESH_TOKEN_EXPIRE_MINUTES";
        public const string ACCESS_JWT_EXPIRE_MINUTES_KEY = "ACCESS_TOKEN_EXPIRE_MINUTES";

        public readonly SigninQryHandler handler;
        private readonly Mock<IUserRepository> userRepositoryMock = new();
        private readonly Mock<IBcrypt> bcryptMock = new();
        private readonly Mock<IEnvStore> envStoreMock = new();
        private readonly Mock<IJWTService> jwtServiceMock = new();

        public SigninQryHandlerTestObject()
        {
            var services = new ServiceCollection();
            services.AddSingleton(userRepositoryMock.Object);
            services.AddSingleton(bcryptMock.Object);
            services.AddSingleton(envStoreMock.Object);
            services.AddSingleton(jwtServiceMock.Object);
            services.AddScoped<AuthTokenGenerator>();
            services.AddScoped<SigninQryHandler>();

            var serviceProvider = services.BuildServiceProvider();
            handler = serviceProvider.GetRequiredService<SigninQryHandler>();
        }

        public SigninQryHandlerTestObject DefaultMocks()
        {
            var defaultUser = UserMother.Active();
            userRepositoryMock
                .Setup(r => r.FindByEmail(It.IsAny<string>()))
                .ReturnsAsync(defaultUser);
            bcryptMock.Setup(b => b.Compare(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            envStoreMock.Setup(e => e.Get(ACCESS_JWT_SECRET_KEY)).Returns("access-secret");
            envStoreMock.Setup(e => e.Get(REFRESH_JWT_SECRET_KEY)).Returns("refresh-secret");
            envStoreMock.Setup(e => e.Get(ACCESS_JWT_EXPIRE_MINUTES_KEY)).Returns("10");
            envStoreMock.Setup(e => e.Get(REFRESH_JWT_EXPIRE_MINUTES_KEY)).Returns("10");

            jwtServiceMock
                .Setup(j => j.Generate(It.IsAny<JWTPayload>(), It.IsAny<JWTOptions>()))
                .Returns("token");

            return this;
        }

        internal SigninQryHandlerTestObject WithUserByEmail(string email, User? value)
        {
            userRepositoryMock.Setup(r => r.FindByEmail(email)).Returns(Task.FromResult(value));

            return this;
        }

        internal SigninQryHandlerTestObject WithInvalidPassword(string password)
        {
            bcryptMock.Setup(b => b.Compare(password, It.IsAny<string>())).Returns(false);

            return this;
        }

        internal SigninQryHandlerTestObject WithAccessToken(string token)
        {
            var accessTokenSecret = envStoreMock.Object.Get(ACCESS_JWT_SECRET_KEY);

            jwtServiceMock
                .Setup(j =>
                    j.Generate(
                        It.IsAny<JWTPayload>(),
                        It.Is<JWTOptions>(o => o.Secret == accessTokenSecret)
                    )
                )
                .Returns(token);

            return this;
        }

        internal SigninQryHandlerTestObject WithRefreshToken(string refreshToken)
        {
            var refreshTokenSecret = envStoreMock.Object.Get(REFRESH_JWT_SECRET_KEY);
            jwtServiceMock
                .Setup(j =>
                    j.Generate(
                        It.IsAny<JWTPayload>(),
                        It.Is<JWTOptions>(o => o.Secret == refreshTokenSecret)
                    )
                )
                .Returns(refreshToken);

            return this;
        }

        internal SigninQryHandlerTestObject WithAccessTokenEnv(string? secret)
        {
            envStoreMock.Setup(e => e.Get(ACCESS_JWT_SECRET_KEY)).Returns(secret);

            return this;
        }

        internal SigninQryHandlerTestObject WithRefreshTokenSecretEnv(string? secret)
        {
            envStoreMock.Setup(e => e.Get(REFRESH_JWT_SECRET_KEY)).Returns(secret);

            return this;
        }

        internal SigninQryHandlerTestObject WithAccessTokenExpirationMinutesEnv(int? minutes)
        {
            envStoreMock
                .Setup(e => e.Get(ACCESS_JWT_EXPIRE_MINUTES_KEY))
                .Returns(minutes?.ToString());

            return this;
        }

        internal SigninQryHandlerTestObject WithRefreshTokenExpirationMinutesEnv(int? minutes)
        {
            envStoreMock
                .Setup(e => e.Get(REFRESH_JWT_EXPIRE_MINUTES_KEY))
                .Returns(minutes?.ToString());

            return this;
        }

        internal void AssertJWTServiceCalledWith(
            Func<JWTPayload, bool> payloadPredicate,
            Func<JWTOptions, bool> optionsPredicate
        )
        {
            jwtServiceMock.Verify(
                s =>
                    s.Generate(
                        It.Is<JWTPayload>(p => payloadPredicate(p)),
                        It.Is<JWTOptions>(o => optionsPredicate(o))
                    ),
                Times.Once
            );
        }

        internal void AssertIsSigninFailed(Exception ex)
        {
            Assert.IsType<SigninFailed>(ex, exactMatch: false);
        }
    }
}
