using FlowTrack.Iam.Auth.Application.Signin;
using FlowTrack.Iam.Test.User;
using FlowTrack.Iam.User.Domain;
using FlowTrack.Shared.Domain;
using Moq;

namespace FlowTrack.Iam.Test.Auth.Application.Signin;

public class SigninQryHandlerTests
{
    private readonly string ACCESS_JWT_SECRET_KEY = "ACCESS_TOKEN_SECRET";

    private readonly SigninQryHandler handler;
    private readonly Mock<IUserRepository> userRepositoryMock = new();
    private readonly Mock<IBcrypt> bcryptMock = new();
    private readonly Mock<IEnvStore> envStoreMock = new();

    public SigninQryHandlerTests()
    {
        handler = new SigninQryHandler(
            userRepositoryMock.Object,
            bcryptMock.Object,
            envStoreMock.Object
        );
    }

    [Fact]
    public async Task Should_Find_User_In_Repository()
    {
        var query = new SigninQry("testuser", "password123");
        var user = UserMother.Random();

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));

        await handler.Handle(query);

        userRepositoryMock.Verify(r => r.FindByEmail(query.Email), Times.Once);
    }

    [Fact]
    public async Task Should_Compare_Password()
    {
        var query = new SigninQry("testuser", "password123");
        var user = UserMother.Random();

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));

        await handler.Handle(query);

        bcryptMock.Verify(b => b.Compare(query.Password, user.Password), Times.Once);
    }

    [Fact]
    public async Task Should_Extract_Access_Token_Secret_From_Env()
    {
        var query = new SigninQry("testuser", "password123");
        var user = UserMother.Random();

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));
        bcryptMock.Setup(b => b.Compare(query.Password, user.Password)).Returns(true);

        await handler.Handle(query);

        envStoreMock.Verify(e => e.Get(ACCESS_JWT_SECRET_KEY), Times.Once);
    }
}
