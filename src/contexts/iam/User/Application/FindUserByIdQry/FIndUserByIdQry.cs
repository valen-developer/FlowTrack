using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain.Bus.Query;

namespace FlowTrack.Iam.Application;

public record FindUserByIdQry(Guid Id) : IQuery<User>;
