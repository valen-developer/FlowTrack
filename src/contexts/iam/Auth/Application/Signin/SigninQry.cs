using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Auth.Application;

public record SigninQry(string Email, string Password) : IQuery<SigninSuccess>;
