namespace Iam.User.application.Signin;



public class SigninQryHandlerTests
{

  private readonly SigninQryHandler handler;

  public SigninQryHandlerTests()
  {
    handler = new SigninQryHandler();
  }



  [Fact]
  public async Task Handle_Should_Have_Handle_Method()
  {

    var query = new SigninQry("testuser", "password123");

    await handler.Handle(query);
  }



}
