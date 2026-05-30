using FlowTrack.Iam.Auth.Application.Signin;
using FlowTrack.Iam.Test.User;
using FlowTrack.Iam.User.Domain;
using FlowTrack.Shared.Domain;
using Moq;

namespace FlowTrack.Iam.Test.Auth.Application.Signin;

public class SigninQryHandlerTests
{
    private readonly SigninQryHandler handler;
    private readonly Mock<IUserRepository> userRepositoryMock = new();
    private readonly Mock<IBcrypt> bcryptMock = new();

    public SigninQryHandlerTests()
    {
        handler = new SigninQryHandler(userRepositoryMock.Object, bcryptMock.Object);
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
}
