using Iam.User.Domain;
using Moq;

namespace Iam.User.application.Signin;

public class SigninQryHandlerTests
{
    private readonly SigninQryHandler handler;
    private readonly Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();

    public SigninQryHandlerTests()
    {
        handler = new SigninQryHandler(userRepositoryMock.Object);
    }

    [Fact]
    public async Task Should_Have_Handle_Method()
    {
        var query = new SigninQry("testuser", "password123");

        await handler.Handle(query);
    }

    [Fact]
    public async Task Should_Find_User_In_Repository()
    {
        var query = new SigninQry("testuser", "password123");
        await handler.Handle(query);
        // Verify with Moq
        userRepositoryMock.Verify(r => r.FindByEmail(query.Email), Times.Once);
    }
}
