using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Application;

public record FindUserByIdQry(string Id) : IQuery<User>;
