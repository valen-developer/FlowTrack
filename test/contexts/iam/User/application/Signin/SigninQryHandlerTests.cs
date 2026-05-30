using Iam.User.Domain;
using Moq;
using Shared.domain;

namespace Iam.User.application.Signin;

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
        var user = new Iam.User.Domain.User(
            id: Guid.NewGuid(),
            email: query.Email,
            password: "hashedpassword"
        );

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));

        await handler.Handle(query);

        userRepositoryMock.Verify(r => r.FindByEmail(query.Email), Times.Once);
    }

    [Fact]
    public async Task Should_Compare_Password()
    {
        var query = new SigninQry("testuser", "password123");
        var user = new Iam.User.Domain.User(
            id: Guid.NewGuid(),
            email: query.Email,
            password: "hashedpassword"
        );

        userRepositoryMock.Setup(r => r.FindByEmail(query.Email)).Returns(Task.FromResult(user));

        await handler.Handle(query);

        bcryptMock.Verify(b => b.Compare(query.Password, user.Password), Times.Once);
    }
}
