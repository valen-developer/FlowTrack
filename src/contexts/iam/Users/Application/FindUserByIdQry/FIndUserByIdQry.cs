using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Users.Application;

public record FindUserByIdQry(string Id) : IQuery<User>;
