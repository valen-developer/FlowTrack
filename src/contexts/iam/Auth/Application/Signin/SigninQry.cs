using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain.Bus.Query;

namespace FlowTrack.Iam.Application;

public record SigninQry(string Email, string Password) : IQuery<SigninSuccess>;
