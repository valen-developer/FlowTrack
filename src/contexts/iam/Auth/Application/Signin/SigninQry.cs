using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Application;

public record SigninQry(string Email, string Password) : IQuery<SigninSuccess>;
