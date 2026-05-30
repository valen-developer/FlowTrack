using FlowTrack.Shared.Domain.Bus.Query;

namespace FlowTrack.Iam.Auth.Application.Signin;

public record SigninQry(string Email, string Password) : IQuery<object>;
