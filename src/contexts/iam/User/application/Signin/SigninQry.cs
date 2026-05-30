using Shared.Domain.Bus.Query;

namespace Iam.User.application.Signin;

public record SigninQry(string Email, string Password) : IQuery<object>;
