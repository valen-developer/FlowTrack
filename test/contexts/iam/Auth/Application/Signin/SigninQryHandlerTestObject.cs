using System.Linq.Expressions;
using FlowTrack.Iam.Auth.Application.Signin;
using FlowTrack.Iam.Test.User;
using FlowTrack.Iam.User.Domain;
using FlowTrack.Shared.Domain;
using Moq;
using DomainUser = FlowTrack.Iam.User.Domain.User;

namespace FlowTrack.Iam.Test.Auth.Application.Signin;

internal sealed class SigninQryHandlerTestObject
{
    private const string REFRESH_JWT_EXPIRE_MINUTES_KEY = "REFRESH_TOKEN_EXPIRE_MINUTES";
    private const string ACCESS_JWT_EXPIRE_MINUTES_KEY = "ACCESS_TOKEN_EXPIRE_MINUTES";

    public readonly SigninQryHandler handler;
    private readonly Mock<IUserRepository> userRepositoryMock = new();
    private readonly Mock<IBcrypt> bcryptMock = new();
    private readonly Mock<IEnvStore> envStoreMock = new();
    private readonly Mock<IJWTService> jwtServiceMock = new();

    public SigninQryHandlerTestObject()
    {
        handler = new SigninQryHandler(
            userRepositoryMock.Object,
            bcryptMock.Object,
            envStoreMock.Object,
            jwtServiceMock.Object
        );
    }

    public SigninQryHandlerTestObject DefaultMocks()
    {
        var defaultUser = UserMother.Random();
        userRepositoryMock.Setup(r => r.FindByEmail(It.IsAny<string>())).ReturnsAsync(defaultUser);
        bcryptMock.Setup(b => b.Compare(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        envStoreMock.Setup(e => e.Get(It.IsAny<string>())).Returns("test");

        envStoreMock.Setup(e => e.Get(ACCESS_JWT_EXPIRE_MINUTES_KEY)).Returns("10"); // 1 hour
        envStoreMock.Setup(e => e.Get(REFRESH_JWT_EXPIRE_MINUTES_KEY)).Returns("10"); // 30 days

        return this;
    }

    public SigninQryHandlerTestObject WithAccessTokenExpirationMinutesEnv(int? minutes)
    {
        envStoreMock.Setup(e => e.Get(ACCESS_JWT_EXPIRE_MINUTES_KEY)).Returns(minutes?.ToString());

        return this;
    }

    public SigninQryHandlerTestObject WithRefreshTokenExpirationMinutesEnv(int? minutes)
    {
        envStoreMock.Setup(e => e.Get(REFRESH_JWT_EXPIRE_MINUTES_KEY)).Returns(minutes?.ToString());

        return this;
    }

    public void AssertJWTServiceCalledWith(
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
}
